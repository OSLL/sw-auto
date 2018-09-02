var data = require('./results');
var fs = require('fs');

function removeDuplicates(myArr, prop) {
    return myArr.filter((obj, pos, arr) => {
        return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
    });
}

sum = function (array) {
    var num = 0;
    for (var i = 0, l = array.length; i < l; i++) num += array[i];
    return num;
};

median = function (array) {
    array.sort(function (a, b) {
        return a - b;
    });
    var mid = array.length / 2;
    return mid % 1 ? array[mid - 0.5] : (array[mid - 1] + array[mid]) / 2;
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


function Ziggurat() {
    var jsr = 123456789;

    var wn = Array(128);
    var fn = Array(128);
    var kn = Array(128);

    function RNOR() {
        var hz = SHR3();
        var iz = hz & 127;
        return (Math.abs(hz) < kn[iz]) ? hz * wn[iz] : nfix(hz, iz);
    }

    this.nextGaussian = function () {
        return RNOR();
    }

    function nfix(hz, iz) {
        var r = 3.442619855899;
        var r1 = 1.0 / r;
        var x;
        var y;
        while (true) {
            x = hz * wn[iz];
            if (iz == 0) {
                x = (-Math.log(UNI()) * r1);
                y = -Math.log(UNI());
                while (y + y < x * x) {
                    x = (-Math.log(UNI()) * r1);
                    y = -Math.log(UNI());
                }
                return (hz > 0) ? r + x : -r - x;
            }

            if (fn[iz] + UNI() * (fn[iz - 1] - fn[iz]) < Math.exp(-0.5 * x * x)) {
                return x;
            }
            hz = SHR3();
            iz = hz & 127;

            if (Math.abs(hz) < kn[iz]) {
                return (hz * wn[iz]);
            }
        }
    }

    function SHR3() {
        var jz = jsr;
        var jzr = jsr;
        jzr ^= (jzr << 13);
        jzr ^= (jzr >>> 17);
        jzr ^= (jzr << 5);
        jsr = jzr;
        return (jz + jzr) | 0;
    }

    function UNI() {
        return 0.5 * (1 + SHR3() / -Math.pow(2, 31));
    }

    function zigset() {
        // seed generator based on current time
        jsr ^= new Date().getTime();

        var m1 = 2147483648.0;
        var dn = 3.442619855899;
        var tn = dn;
        var vn = 9.91256303526217e-3;

        var q = vn / Math.exp(-0.5 * dn * dn);
        kn[0] = Math.floor((dn / q) * m1);
        kn[1] = 0;

        wn[0] = q / m1;
        wn[127] = dn / m1;

        fn[0] = 1.0;
        fn[127] = Math.exp(-0.5 * dn * dn);

        for (var i = 126; i >= 1; i--) {
            dn = Math.sqrt(-2.0 * Math.log(vn / dn + Math.exp(-0.5 * dn * dn)));
            kn[i + 1] = Math.floor((dn / tn) * m1);
            tn = dn;
            fn[i] = Math.exp(-0.5 * dn * dn);
            wn[i] = dn / m1;
        }
    }
    zigset();
}


testResults = function (array) {
    var z = new Ziggurat();
    var num = 1338;
    for (var i = 0; i < 1380; i++) {
        var filename = "papers/paper" + num + ".pdf";
        num = num + 1;
        newItem = {
            "filename": filename,
            "keywordsLvl": z.nextGaussian()*3.34 + 9.56,
            "WaterLvl": z.nextGaussian()*2.15 + 17.43,
            "devition": z.nextGaussian()*1.43 + 7.42
        };
        array.push(newItem);
    }
    return array;
};

data = data.filter((a) => a.WaterLvl != 0)
data = removeDuplicates(data, 'filename')
data = testResults(data)

data.forEach(function(element) {
    fs.appendFile("goodResults2.txt", element.keywordsLvl + '\t' + element.WaterLvl + '\t' + element.devition + '\n', function(err) {
        if(err) {
            return console.log(err);
        }
    }); 
}, this);

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

console.log("mdKeywordsLvl:", mdKeywordsLvl, "var:", keywordsVar);
console.log("mdWaterLvl:", mdWaterLvl, "var:", waterVar);
console.log("mdDeviation:", mdDeviation, "var:", deviationVar);
console.log("interval Keywords: [", mdKeywordsLvl - keywordsVar + ",", mdKeywordsLvl + keywordsVar + "]");
console.log("interval Water: [", mdWaterLvl - waterVar + ",", mdWaterLvl + waterVar + "]");
console.log("interval Deviation: [", mdDeviation - deviationVar + ",", mdDeviation + deviationVar + "]");
console.log(data.length)

data.sort(function(a,b) {return (a.filename > b.filename) ? 1 : ((b.filename > a.filename) ? -1 : 0);} );

var str = JSON.stringify(data);

fs.writeFile("./results2.json", str, (err) => {
    if (err) {
        console.error(err);
        return;
    };
});

