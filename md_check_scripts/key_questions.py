# -*- coding: UTF-8 -*-
import argparse, os
import mistune
from pathlib import Path
from bs4 import BeautifulSoup

PURPOSE_STATEMENT_MIN = 300

def CheckRepo(repoPath):
    print('Checking for directory "' + repoPath + '"')
    directory = Path(repoPath)
    if directory.is_dir():
        print(' Directory'.ljust(26) +  '...\tExists!') # \u2713
        CheckGroupDirectory(repoPath + '\\3303')
        CheckGroupDirectory(repoPath + '\\3304')
        CheckGroupDirectory(repoPath + '\\3381')
    else:
        print("Directory doesn't exist!")

def CheckGroupDirectory(groupFolderPath):
    print('Checking for directory "' + groupFolderPath + '"')
    directory = Path(groupFolderPath)
    if directory.is_dir():
        print(' Directory'.ljust(26) +  '...\tExists!') # \u2713
        #find all nameFolders
    else:
        print("Directory doesn't exist!")

def CheckNamedDirectory(nameFolderPath):
    print('Checking for directory "' + nameFolderPath + '"')
    directory = Path(nameFolderPath)

    if directory.is_dir():
        print(' Directory'.ljust(26) +  '...\tExists!') # \u2713
        CheckForPaperBase(nameFolderPath + '\\paper_base')
        CheckForFactResult(nameFolderPath)
        CheckPurposeStatementFiles(nameFolderPath)
        CheckForAnalogs(nameFolderPath)     
    else:
        print("Directory doesn't exist!")

def CheckForPaperBase(paperBaseFolderPath):
    #.pdf file(s)
    print("CheckForPaperBase")

def CheckForFactResult(factResFolderPath):
    #1200 symbols
    print("CheckForFactResult")

def CheckForAnalogs(analogsFolderPath):
    #2000symbols, min 3 analogs, min 3 criterias, references
    print("CheckForAnalogs")
    pass

def CheckPurposeStatementFiles(nameFolderPath):
    print('Checking for purpose-statement files...')
    if (
        CheckForFile(nameFolderPath,"problem.md") &
        CheckForFile(nameFolderPath,"research_object.md") &
        CheckForFile(nameFolderPath,"research_subject.md") &
        CheckForFile(nameFolderPath,"goal.md") &
        CheckForFile(nameFolderPath,"tasks.md") &
        CheckForFile(nameFolderPath,"relevance.md") ):
        print('All required files exist!\nCounting symbols...')            
        purposeSymbNum = CountPurposeTextSymbols()
        if purposeSymbNum >= PURPOSE_STATEMENT_MIN:
            print("Number of symbols in purpose statement's".ljust(50) + '...\tGood.')
        else:
            print("Number of symbols in purpose statement's".ljust(50) + '...\tNot enough. Min = ' + \
                str(PURPOSE_STATEMENT_MIN))
    else:
        print('\nPlease, add all required files to the directory!')

def CountPurposeTextSymbols():
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
    return allSymbolsNum

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
    soup = ParseMd(repo_path + '\\' + filename)
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

parser = argparse.ArgumentParser()
parser.add_argument('path', help='path to directory with .md files')
args = parser.parse_args()
repo_path = args.path

CheckRepo(repo_path)