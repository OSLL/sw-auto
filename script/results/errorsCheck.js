var data2016 = require('./bach/results2016');
var data2017 = require('./bach/results2017');
var fs = require('fs');

var _data16 = data2016.results.slice(0,-3);
var _data17 = data2017.results.slice(0,-3);

var data = _data16.concat(_data17)

sum = function (array) {
    var num = 0;
    for (var i = 0, l = array.length; i < l; i++) num += array[i];
    return num;
};

mean = function (array) {
    return sum(array) / array.length;
};

variance = function (array) {
    var meanV = mean(array);
    return mean(array.map(function (num) {
        return Math.pow(num - meanV, 2);
    }));
};

standardDeviation = function (array) {
    return Math.sqrt(variance(array));
};

const waterData = data.map(function (item) {
    return item.WaterLvl;
});

const keywordsLvlData = data.map(function (item) {
    return item.keywordsLvl;
});

const deviationData = data.map(function (item) {
    return item.devition;
});

const avgKeywordsLvl = mean(keywordsLvlData);
const avgWaterLvl = mean(waterData);
const avgDeviation = mean(deviationData);

const keywordsVar = standardDeviation(keywordsLvlData);
const waterVar = standardDeviation(waterData);
const deviationVar = standardDeviation(deviationData);

// console.log("avgKeywordsLvl:", avgKeywordsLvl, "var:", keywordsVar);
// console.log("avgWaterLvl:", avgWaterLvl, "var:", waterVar);
// console.log("avgDeviation:", avgDeviation, "var:", deviationVar);
// console.log("interval Keywords: [", avgKeywordsLvl - keywordsVar + ",", avgKeywordsLvl + keywordsVar + "]");
// console.log("interval Water: [", avgWaterLvl - waterVar + ",", avgWaterLvl + waterVar + "]");
// console.log("interval Deviation: [", avgDeviation - deviationVar + ",", avgDeviation + deviationVar + "]");
// console.log(data.length)

var keyMin = 6;
var keyMax = 14;
var waterMin = 14;
var waterMax = 20;
var varMin = 5.5;
var varMax = 9.5;

var _keyMin = 3.5;
var _keyMax = 13;
var _waterMin = 13.5;
var _waterMax = 19.5;
var _varMin = 6;
var _varMax = 22.5;

var errors = 0;
var _errors = 0;

var checkNums = [0,0,0,0]

data.forEach(element => {
    var checks = 0;
    var _checks = 0;
    if (keyMin <= element.keywordsLvl && element.keywordsLvl <= keyMax) {
        checks++;
    }
    if (waterMin <= element.WaterLvl && element.WaterLvl <= waterMax) {
        checks++;
    }
    if (varMin <= element.devition && element.devition <= varMax) {
        checks++;
    }
    if (_keyMin <= element.keywordsLvl && element.keywordsLvl <= _keyMax) {
        _checks++;
    }
    if (_waterMin <= element.WaterLvl && element.WaterLvl <= _waterMax) {
        _checks++;
    }
    if (_varMin <= element.devition && element.devition <= _varMax) {
        _checks++;
    }
    console.log(element.grade, ' - ', checks, ', ', _checks);
    switch (element.grade) {
        case 3:
            if (checks > 1) {
                errors++;
            }
            if (_checks > 1) {
                _errors++;
            }
            break;
        case 4:
            if (checks > 2 || checks < 1) {
                errors++;
            }
            if (_checks > 2 || _checks < 1) {
                _errors++;
            }
            break;
        case 5:
            if (checks < 2) {
                errors++;
            }
            if (_checks < 2) {
                _errors++;
            }
            break;
    }
    switch (checks) {
        case 0:
            checkNums[0]++;
            break;
        case 1:
            checkNums[1]++;
            break;
        case 2:
            checkNums[2]++;
            break;
        case 3:
            checkNums[3]++;
            break;
    }
});

console.log(checkNums)
console.log(errors)
console.log(_errors)

// var str = JSON.stringify(data);

// fs.writeFile("./results2.json", str, (err) => {
//     if (err) {
//         console.error(err);
//         return;
//     };
// });