# -*- coding: UTF-8 -*-
import argparse, os
import mistune
from pathlib import Path
from bs4 import BeautifulSoup

def CheckForFile(dirpath, filename):
    file = Path(dirpath+'/' + filename)
    
    if file.is_file():
        print(' ' + filename.ljust(25) + '...\tExists!')
        return True
    else:
        print(' ' + filename.ljust(25) + "...\tDoesn't exist!")
        return False

def ParseMd(file):
    f = open(file, 'r')
    mdText = f.read()
    htmlText = mistune.markdown(mdText)
    soup = BeautifulSoup(htmlText, 'html.parser')
    return soup

def CountTextSymbols(filename):
    soup = ParseMd(path + '/' + filename)
    pars = soup.find_all('p')
    allText = ""
    for t in pars:
        allText += t.text
    NONCOUNT_LETTERS = " ,.:;!?"
    symbolsNum = len([letter for letter in allText if letter not in NONCOUNT_LETTERS])
    if symbolsNum == 0:
        print(' ' + filename.ljust(25) + '...\tEmpty!')
    else:
        print(' ' + filename.ljust(25) + '...\t' + str(symbolsNum) + ' symbols')
    return symbolsNum

def CountAllTextSymbols():
    allSymbolsNum = CountTextSymbols("problem.md") + \
            CountTextSymbols("research_object.md") + \
            CountTextSymbols("research_subject.md") + \
            CountTextSymbols("goal.md") + \
            CountTextSymbols("tasks.md") + \
            CountTextSymbols("relevance.md")
    if allSymbolsNum == 0:
        print('All files are empty!')
    else:
        print('All files'.ljust(26) + '...\t' + str(allSymbolsNum) + ' symbols')

parser = argparse.ArgumentParser()
parser.add_argument('path', help='path to directory with .md files')
args = parser.parse_args()
path = args.path

print('Checking for directory...')

directory = Path(path)
if directory.is_dir():
    print(' Directory'.ljust(26) +  '...\tExists!') # \u2713
    print('Checking for files...')
    if (
        CheckForFile(path,"problem.md") &
        CheckForFile(path,"research_object.md") &
        CheckForFile(path,"research_subject.md") &
        CheckForFile(path,"goal.md") &
        CheckForFile(path,"tasks.md") &
        CheckForFile(path,"relevance.md") ):
        print('All required files exist!\nCounting symbols...')
        
        CountAllTextSymbols()
    else:
        print('\nPlease, add all required files to the directory!')
else:
    print("Directory doesn't exist!")
    
