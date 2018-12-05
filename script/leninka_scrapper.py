#!/usr/bin/env python3
# -*- coding: UTF-8 -*-

import urllib
from BeautifulSoup import BeautifulSoup as BS
import os
import sys

disciplineLinks = ["https://cyberleninka.ru/article/c/avtomatika-vychislitelnaya-tehnika/",
    "https://cyberleninka.ru/article/c/informatika/"]
baseLink = "https://cyberleninka.ru"
baseFolder = "papers/"
baseName = "paper"

fileLoader = urllib.URLopener()
counter = 1
for disc in disciplineLinks:
    for page in range (1,50):
        html = urllib.urlopen(disc1 + str(page))
        soup = BS(html)
        elems = [baseLink + x['href'] + '.pdf' for x in soup.findAll('a') if x['href'].find("article/n/") != -1]
        for elem in elems:
            fileInfo = fileLoader.retrieve(elem, baseFolder + baseName + str(counter) + ".pdf")
            if os.stat(fileInfo[0]).st_size < 15000: #file less than appr. 15kb
                sys.exit("Captcha!")
            counter+=1
