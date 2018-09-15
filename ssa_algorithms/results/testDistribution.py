from scipy import stats
import json

with open('results.json') as f:
    data = json.load(f)
keyArr = list(map(lambda x: x["keywordsLvl"], data))
waterArr = list(map(lambda x: x["WaterLvl"], data))
deviationArr = list(map(lambda x: x["devition"], data))

keyMean = sum(keyArr) / float(len(keyArr))
keyArr[:] = [x - keyMean for x in keyArr]

waterMean = sum(waterArr) / float(len(waterArr))
waterArr[:] = [x - waterMean for x in waterArr]

deviationMean = sum(deviationArr) / float(len(deviationArr))
deviationArr[:] = [x - deviationMean for x in deviationArr]

with open('results2.json') as f:
    dataTst = json.load(f)

result = stats.shapiro(keyArr)
print (result)