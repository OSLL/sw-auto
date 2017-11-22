import nltk

from nltk.corpus import reuters
from nltk.corpus import wordnet

reuters_words = [w.lower() for w in reuters.words()]
words = set(reuters_words)
counts = [(w, reuters_words.count(w)) for w in words]

[(w, c) for (w, c) in counts if c > 5000]

import scipy.stats as ss
 
amb = [(w, c, len(wordnet.synsets(w))) for (w, c) in counts if len(wordnet.synsets(w)) > 0]
 
amb_p_rank = ss.rankdata([p for (w, c, p) in amb])
amb_c_rank = ss.rankdata([c for (w, c, p) in amb])
 
amb_ranked = zip(amb, amb_p_rank, amb_c_rank)
 
import math
import numpy

numpy.corrcoef(amb_c_rank, [math.log(c) for (w, c, p) in amb])

import matplotlib
rev = [l-r+1 for r in amb_c_rank]
 
plt.plot([math.log(c) for c in rev], [math.log(c) for (w, c, p) in amb], 'ro')