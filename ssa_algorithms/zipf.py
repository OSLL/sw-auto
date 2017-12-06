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
    mdFiles = [f for f in files if f.endswith(".md")]
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
    for _word in wordList:
        currForm = morph.parse(_word)[0]
        nounForm = currForm.inflect({'sing','nomn'})

        try:
            word = nounForm.word
        except:
            word = currForm.word
        
        if word in counts:
            counts[word] += 1
        else:
            counts[word] = 1
    return counts

def GetStats(dirPath):
    CheckForDir(dirPath)
    allText = GetAllText(dirPath)
    wordList = re.sub("[^\w]", " ",  allText).split()
    wordList = [w.lower() for w in wordList]
    counts = CountWords(wordList)

    for word, freq in counts.items():
        if freq > 2:
            print(word + ": " + str(freq))
   
    amb = [(w, c) for (w, c) in counts.items()]    
    amb_c_rank = ss.rankdata([c for (w, c) in amb])
    amb_sorted = sorted(amb, key=lambda x: x[1], reverse=True)

    #numpy.corrcoef(amb_c_rank, [math.log(c) for (w, c) in amb])
    rev = [len(amb_c_rank)-r+1 for r in amb_c_rank]    
    #plt.plot([math.log(c) for c in rev], [math.log(c) for (w, c) in amb], 'ro')

    x = range(0, len(amb_sorted[0:5]))
    y = [c for (w, c) in amb_sorted[0:5]]
    my_xticks = [w for (w, c) in amb_sorted[0:5]]
    plt.xticks(x, my_xticks)
    plt.plot(x, y)
    plt.show()

parser = argparse.ArgumentParser()
parser.add_argument('path', help='path to directory with .md files')
args = parser.parse_args()
dir_path = args.path

GetStats(dir_path)