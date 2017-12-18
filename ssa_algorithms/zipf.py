#!/usr/bin/env python3
# -*- coding: UTF-8 -*-

import os
import codecs
import mistune
import csv
import argparse
from glob import glob
from pathlib import Path
from bs4 import BeautifulSoup
import re
import pymorphy2
import scipy.stats as ss
import math
import numpy
import matplotlib.pyplot as plt
from nltk.corpus import stopwords

def CheckForDir(dirpath):
    dir = Path(dirpath)

    if dir.is_dir():
        print('     ' + str(dir).ljust(25) + '...\tExists!')
        return True
    else:
        print('     ' + str(dir).ljust(25) + "...\tDoesn't exist!")
        return False

def CheckForFile(dirpath, filename):
    file = Path(os.path.join(dirpath,filename))

    if file.is_file():
        print('     ' + filename.ljust(25) + '...\tExists!')
        return True
    else:
        print('     ' + filename.ljust(25) + "...\tDoesn't exist!")
        return False

def ParseMd(file):
    f = open(str(file), 'r', encoding="utf-8-sig")
    try:
        mdText = f.read()
    except:
        mdText = " "
    
    htmlText = mistune.markdown(mdText)
    soup = BeautifulSoup(htmlText, 'html.parser')
    return soup

def GetAllText(dirpath):
    files = os.listdir(dirpath)
    mdFiles = [f for f in files if f.endswith("paper.md")]
    mdFiles = [os.path.join(dirpath, f) for f in mdFiles]
    allText = ''
    for file in mdFiles:
        allText += GetText(file) + ' '
    return allText

def GetText(filename):
    soup = ParseMd(filename)
    #print(filename)
    #print(soup) #encode("utf-8-sig")
    pars = soup.find_all(['p','li'])
    allText = " "
    for t in pars:
        allText += t.text + ' '
    return allText

def CountWords(wordList):
    morph = pymorphy2.MorphAnalyzer()
    counts = {}
    normalFormWordList = []
    for _word in wordList:
        currForm = morph.parse(_word)[0]
        nounForm = currForm.inflect({'sing','nomn'})

        try:
            word = nounForm.word
        except:
            word = currForm.word
        
        normalFormWordList.append(word)

    ruStopWords = set(stopwords.words('russian'))
    enStopWords = set(stopwords.words('english'))

    filteredWordList = [_word for _word in normalFormWordList if _word not in ruStopWords]
    filteredWordList = [_word for _word in filteredWordList if _word not in enStopWords]
    filteredWordList = [_word for _word in filteredWordList if not _word.isdigit()]

    for _word in filteredWordList:
        if _word in counts:
            counts[_word] += 1
        else:
            counts[_word] = 1
    
    return counts

def checkWater(wordList):
    ruStopWords = set(stopwords.words('russian'))
    stopWords = [_word for _word in wordList if _word in ruStopWords]
    waterLevel = len(stopWords)/len(wordList) * 100
    return (len(stopWords), waterLevel)

def GetYPlot(data):
    _data = []
    _max = max(data)
    for i in range(0,len(data)):
        _data.append(_max/(i+1))
    return _data

def GetStandartDeviation(data):
    maxElem = max(data)
    perfectData = []
    deviation = 0
    for i in range (0, len(data)):
        perfectData.append(data[0]/(i+1))
    for i in range (0, len(data)):
        deviation += math.pow(data[i]-perfectData[i],2)
    return math.sqrt(deviation/len(data))

def GetTestResults():
    results = [ (24.36548223350254,2.3869048043157406), (20.056899004267425,5.904047182531645), \
    (14.676616915422885,6.8405488439403035),(22.485207100591715,5.941721356463498), \
    (19.74852071005917,6.167598935904716), (24.305949008498583,6.183000683206467), \
    (20.476460578559276,7.18112890717097), (19.353984643897274,7.938768037444853), \
    (20.76502732240437,5.085302021852815), (15.081206496519723,7.424254455924562), \
    (18.95497498610339,8.641253207882684), (19.163578613022764,8.756846223351458), \
    (20.752895752895753,5.184431966858193), (23.007623007623007,5.983537361734219), \
    (18.98428053204353,7.885213268411863), (17.59927797833935,5.860974748675821), \
    (21.070234113712374,8.096431783376046)]

    #??mean = ss.Mean(results)

    print(mean)

def GetStats(dirPath):
    CheckForDir(dirPath)
    allText = GetAllText(dirPath)
    wordList = re.sub("[^\w]", " ",  allText).split()
    wordList = [w.lower() for w in wordList]
    water = checkWater(wordList)
    counts = CountWords(wordList)

    for word, freq in counts.items():
        if freq > 10:
            print(word + ": " + str(freq))

    print("Stopwords in text: " + str(water[0]))
    print("Waterlevel: " + str(water[1]) + "%")  
   
    amb = [(w, c) for (w, c) in counts.items()]    
    amb_c_rank = ss.rankdata([c for (w, c) in amb])
    amb_sorted = sorted(amb, key=lambda x: x[1], reverse=True)

    x = range(0, len(amb_sorted[0:]))
    y = [c for (w, c) in amb_sorted[0:]]
    y2 = GetYPlot([c for (w, c) in amb_sorted[0:]])

    # _max = max([c for (w, c) in amb_sorted[0:10]])
    # _min = min(y2)

    # diff = (_max - _min)/9
    # y3 = []
    # for i in range(0,10):
    #     y3.append(_max - i*diff)

    
    deviation = GetStandartDeviation([c for (w, c) in amb_sorted if c >= 5])#GetStandartDeviation(y3)
    print("deviation: " + str(deviation))

    # my_xticks = [w for (w, c) in amb_sorted[0:]]
    # plt.xticks(x, my_xticks)
    plt.plot(x, y)
    plt.plot(x, y2)
    
    plt.show()

parser = argparse.ArgumentParser()
parser.add_argument('path', help='path to directory with .md files')
args = parser.parse_args()
dir_path = args.path

#GetStats(dir_path)
GetTestResults()