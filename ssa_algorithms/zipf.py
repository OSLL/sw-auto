#!/usr/bin/env python3
# -*- coding: UTF-8 -*-
# import nltk
import os
import codecs
import mistune
import csv
import argparse
from glob import glob
from pathlib import Path
from bs4 import BeautifulSoup
import re
# import scipy.stats as ss
# import math
# import numpy
# import matplotlib

# from nltk.corpus import reuters
# from nltk.corpus import wordnet

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

def GetText(filename):
    soup = ParseMd(filename)
    pars = soup.find_all(['p','li'])
    allText = ""
    for t in pars:
        allText += t.text + ' '
    return allText

def GetStats(filename):
    CheckForFile(os.getcwd(), filename)
    allText = GetText(os.path.join(os.getcwd(),filename))
    wordList = re.sub("[^\w]", " ",  allText).split()
    wordList = [w.lower() for w in wordList]
    words = set(wordList)
    counts = [(w, wordList.count(w)) for w in words]

    for (w, c) in counts:
        if c > 1:
            print(w + ": " + str(c))  
    
    # amb = [(w, c, len(wordnet.synsets(w))) for (w, c) in counts if len(wordnet.synsets(w)) > 0]
    
    # amb_p_rank = ss.rankdata([p for (w, c, p) in amb])
    # amb_c_rank = ss.rankdata([c for (w, c, p) in amb])
    
    # amb_ranked = zip(amb, amb_p_rank, amb_c_rank)    

    # numpy.corrcoef(amb_c_rank, [math.log(c) for (w, c, p) in amb])
    
    # rev = [l-r+1 for r in amb_c_rank]
    
    # plt.plot([math.log(c) for c in rev], [math.log(c) for (w, c, p) in amb], 'ro')

parser = argparse.ArgumentParser()
parser.add_argument('path', help='path to directory with .md files')
args = parser.parse_args()
file_path = args.path

GetStats(file_path)