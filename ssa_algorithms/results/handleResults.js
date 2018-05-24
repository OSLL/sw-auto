var data = require('./results');
var fs = require('fs');

function removeDuplicates(myArr, prop) {
    return myArr.filter((obj, pos, arr) => {
        return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
    });
}

sum = function(array) {
    var num = 0;
    for (var i = 0, l = array.length; i < l; i++) num += array[i];
    return num;
};

median = function(array) {
    array.sort(function(a, b) {
        return a - b;
    });
    var mid = array.length / 2;
    return mid % 1 ? array[mid - 0.5] : (array[mid - 1] + array[mid]) / 2;
};

mean = function(array) {
    return sum(array) / array.length;
};

variance = function(array) {
    var meanV = mean(array);
    return mean(array.map(function(num) {
        return Math.pow(num - meanV, 2);
    }));
};

standardDeviation = function(array) {
    return Math.sqrt(variance(array));
},

data = data.filter((a)=> a.WaterLvl != 0)
data = removeDuplicates(data,'filename')

// data.forEach(function(element) {
//     fs.appendFile("goodResults.txt", element.keywordsLvl + '\t' + element.WaterLvl + '\t' + element.devition + '\n', function(err) {
//         if(err) {
//             return console.log(err);
//         }
//     }); 
// }, this);

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

const mdKeywordsLvl = median(keywordsLvlData);
const mdWaterLvl = median(waterData);
const mdDeviation = median(deviationData);

const keywordsVar = standardDeviation(keywordsLvlData);
const waterVar = standardDeviation(waterData);
const deviationVar = standardDeviation(deviationData);

console.log("mdKeywordsLvl:", mdKeywordsLvl, "var:",keywordsVar);
console.log("mdWaterLvl:", mdWaterLvl, "var:",waterVar);
console.log("mdDeviation:", mdDeviation, "var:",deviationVar);
console.log("interval Keywords: [", mdKeywordsLvl - keywordsVar + ",",mdKeywordsLvl + keywordsVar + "]");
console.log("interval Water: [", mdWaterLvl - waterVar + ",",mdWaterLvl + waterVar + "]");
console.log("interval Deviation: [", mdDeviation - deviationVar + ",",mdDeviation + deviationVar + "]");


