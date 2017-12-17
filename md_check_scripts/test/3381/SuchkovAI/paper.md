# Фильтрационный алгоритм поиска аддитивных цепочек
## Аннотация

На сегодняшний день остро встаёт вопрос о быстрых вычислениях чисел большого порядка. Вычисление полиномов большой степени необходимо во многих сферах информатики, вычислительной математики и компьютерной алгебры. Наилучшим способом вычисления полиномов, известный человеку, является вычисление с помощью аддитивной цепочки. Единственный недостаток метода заключается в отсутствии точного алгоритма, генерирующую аддитивную цепочку минимальной длины для заданного числа за короткое время. Алгоритм, представленный в данной статье, работает со звёздными аддитивными цепочками и находит цепочку минимальной длины, основываясь на её свойствах.

## Введение

В век информационных технологий такой ресурс, как память, уже не является проблемой. Единственным ресурсом, который человек пытается сократить - это время. В задачах нахождения полинома большой степени за минимальное количество операций, время является основополагающим критерием для определения наилучшего алгоритма. 

Целью данной работы является нахождение способов вычисления полиномов большой степени за минимальное число операций.

Одним из решений данного вопроса является представление степени в виде аддитивной цепочки. Представление числа в виде аддитивной цепочки - актуальна для решения задач обработки и хранения информации. Аддитивные цепочки активно используются в различных областях информатики, а также в области эллиптической криптографии. На сегодняшний день – это наиболее эффективный способ нахождения степени за минимальное число операций, известный человеку.

## Обзор предметной области

Аддитивной цепочкой для некоторого числа ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) называют последовательность натуральных чисел

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%201%20%3D%20c_0%2C%20c_1%2C%20%5Cdots%20%2Cc_m%20%3D%20N),

обладающих тем свойством, что

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_i%20%3D%20c_j%20&plus;%20c_k)

для некоторых ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20k%5Cleq%20j%20%3C%20i%2C%20%5Cforall%20i%3D%5Coverline%7B1..m%7D).

Данное понятие было придумано Арнольдом Шольцом. Наименьшая длина аддитивной цепочки, заканчивающаяся числом ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N), обозначается как ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29%20%3D%20m).

Например, {1, 2, 3, 5, 7, 14} - минимальная цепочка для 14, т.е. ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%2814%29%20%3D%205) [1].

Очевидно, что наименьшее число умножений, необходимое для возведения в ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N)-ую степень, равно ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29).

Наиболее известный и часто используемый вид аддитивной цепочки называется звёздная (линейная). Звёздной цепочкой называют аддитивную цепочку включающую только звёздные шаги, т.е. каждый член ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_i) представляет собой сумму ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%7Bi-1%7D) и предшествующего ему ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_k%2C%200%20%5Cleq%20k%20%5Cleq%20i%20-%201). Для обозначения минимальной длины звёздной цепочки для ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) используется ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%5E*%28N%29); очевидно, что ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29%20%5Cleq%20l%5E*%28N%29).

До сих пор неизвестно поведение функции ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29). Известно лишь, что 

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Clim_%7BN%20%5Cto%20%5Cinfty%7D%20%5Cfrac%7Bl%28N%29%7D%7B%5Clambda%28N%29%7D%20%3D%20%5Clim_%7BN%20%5Cto%20%5Cinfty%7D%20%5Cfrac%7Bl%5E*%28N%29%7D%7B%5Clambda%28N%29%7D%20%3D%201).

Доказательство данного удтверждения приведено в [2, 4, 5]. Но, тем не менее, можно ограничить её для искомого числа. Индуктивно легко доказать, что ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_i%20%5Cleq%202%5Ei) для аддитивной цепочки, отсюда следует, что ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Clog_2N%20%5Cleq%20m). Таким образом

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29%20%5Cgeq%20%5Cleft%20%5Clceil%20%5Clog_2N%20%5Cright%20%5Crceil%20%3D%20%5Cunderline%20l%28N%29)

Назовём аддитивную цепочку тривиальной, если ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29%20%3D%20%5Cunderline%20l%28N%29). Таким образом, учитывая верхнюю границу для бинарного метода, описанного в [1-3], который является минимаксным методом, длина минимальной аддитивной цепочки находится в пределах

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cunderline%20l%28N%29%20%5Cleq%20l%28N%29%20%5Cleq%20%5Coverline%20l%28N%29),

где ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Coverline%20l%28N%29%20%3D%20%5Clambda%28N%29%20&plus;%20%5Cnu%28N%29%20-%201), ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Clambda%28N%29%20%3D%20%5Cleft%20%5Clfloor%20%5Clog_2N%20%5Cright%20%5Crfloor), ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cnu%28N%29) - вес Хэмминга для числа в бинарном представлении.

### Brauer A. T. On addition chains

В [5] описывается алгоритм Брауэра, который представляет из себя разложение числа в ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%202%5Ek)-ую систему счисления (при ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20k%3D1) - схема Горнера, бинарный метод). Аддитивные цепочки, получаемые данным алгоритмом, относят к Брауэровским цепочкам, которые являются подвидом звёздных (линейных) цепочек.

### Yao A. C. On the evaluation of powers

В [6] описывается алгоритм Яо. В данном алгоритме используется разложение в ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%202%5Ek)-ую систему счисления, но при составлении цепочки используется суммирование коэффициентов при цифрах числа в ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%202%5Ek)-ой системе счисления.

### Bernstein D. J. Pippenger's exponentiation algorithm

В [7] кратко охарктеризовываются вышеуказанные алгоритмы, но основное внимание уделяется алгоритму Пиппенджера, который основан на разбиении на подмножества искомого(-ых) числа(-ел). 

## Критерии сравнения аналогов

### Скорость выполнения

Время работы заданного алгоритма должно быть минимальным.

### Точность выполнения

Абсолютная разность между длинами полученной цепочки и действительной не должна больше некоторого числа: ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%7C%20l%28N%29%20-%20l_%7Bmin%7D%28N%29%20%5Cright%20%7C%20%3C%20%5Cvarepsilon).

### Стабильность алгоритма

Для всякого заданного числа алгоритм выдаёт аддитивную цепочку и притом только одну.

## Таблица сравнения по критериям

Аналог\Критерий|Скорость выполнения|Точность выполнения|Стабильность алгоритма
-|-|-|-
Brauer's algorithm|+|-|+
Yao's algorithm|+|-|+
Pippenger's algorithm|+|-+|+

## Выводы по итогам сравнения

Анализируя таблицу, можно сделать вывод, что представленные алгоритмы имеют высокую скорость выполнения и стабильность, но они не являлются точными алгоритмами, а, скорее, приближёнными. Основным преимуществом для них является быстрая скорость работы. Для чисел порядка ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Csim%20%28%5E%7B4%7D2%29%5E%7B32%7D) данные алгоритмы выполняют за долю секунды. К сожалению, полученные аддитивные цепочки для больших чисел не являются минимальными, даже при всех возможных модификациях и упрощениях алгоритмов. Таким образом, необходимо разработать точный алгоритм, т.е. генерирующий для заданного числа минимальную аддитивную цепочку. Но в таком случае точные алгоритмы будут тратить больше времени, по сравнению с приближёнными.

## Выбор метода решения

Результаты сравнения аналогов показывают, не существует точных алгоритмов нахождения аддитивной цепочки минимальной длины. И единственным способом нахождения является перебор. Понятно, что сложность перебора всех вариантов аддитивной цепочки длины ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) достигает ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20o%28N%21%29), что крайне долго.

Необходимо разработать алгоритм поиска аддитивной цепочки, который бы за минимально возможное время для заданного числа находил аддитивную цепочку минимальной длины.

В качестве исследования были выбраны звёздные цепочки. Почему? Во-первых, они уменьшают число сочетаний слагаемых: если для обычной аддитивной цепочки для получения требуется рассмотреть ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cbinom2i) возможных пар слагаемых, то для ЗЦ требуется всего лишь ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20i) слагаемых, поскольку одно из них у нас фиксировано. А во-вторых, при задании следующего определения, вычисление звёздных цепочек становится гораздо удобнее и проще.

## Описание метода решения

Пусть дана звёздная цепочка длины ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20m-1) вида ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20c_i%20%5Cright%20%5C%7D_%7Bi%3D1%7D%5Em) ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_1%20%3D%201). Для каждой звёздной цепочки существует вектор добавок ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%3D1%7D%5E%7Bm-1%7D) длины ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20m-1), такой, что

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20r_i%20%3D%20%5C%7Bx%20%3A%201%20%5Cleq%20x%20%5Cleq%20i%5C%7D%2C%20%5C%2C%20c_i%20%3D%20c_%7Bi%20-%201%7D%20&plus;%20c_%7Br_%7Bi%20-%201%7D%7D%2C%20%5C%2C%202%20%5Cleq%20i%20%5Cleq%20m%20-%201).

Таким образом, ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20r_1) всегда равен 1, ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20r_2%20%3D%20%5C%7B1%3B2%5C%7D) и так далее. Последний элемент вектора добавок принимает значения от 1 до ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20m-1). Естественно считать, что наибольшая звёздная цепочка будет иметь вид ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c%20%3D%20%5Cleft%20%5C%7B%201%3B%202%3B%204%3B%20%5Cdots%20%3B%202%5E%7Bm%20-%201%7D%5Cright%20%5C%7D), а её вектор добавок - ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20r%20%3D%20%5Cleft%20%5C%7B%201%3B%202%3B%203%3B%20%5Cdots%20%3B%20m%20-%201%5Cright%20%5C%7D); наименьшая звёздная цепочка имеет вид ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c%20%3D%20%5Cleft%20%5C%7B%201%3B%202%3B%203%3B%20%5Cdots%20%3B%20m%20%5Cright%20%5C%7D), а её вектор добавок - ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20r%20%3D%20%5Cleft%20%5C%7B%201%3B%201%3B%201%3B%20%5Cdots%20%3B%201%20%5Cright%20%5C%7D).

Исходя из этого, можно сделать вывод, что объём множеств различных звёздных цепочек длины ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20m-1) равно ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%28m-1%29%21), т.е. ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5C%23C%5C%7Bl%5E*%28N%29%20%3D%20m%20-%201%5C%7D%20%3D%20%28m%20-%201%29%21).

С помощью заданных понятий был разработан алгоритм, основанный на свойствах вектора добавок.

Пусть задано некое число ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N). Необходимо найти минимальную звёздную цепочку, такую, что ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_m%20%3D%20N).

Представим заданное число в бинарном представлении: ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N%20%3D%20%5Csum_%7Bi%20%3D%201%7D%5Es2%5E%7Bk_i%7D). Прямой алгоритм (перебор) заключается в следующем: перебираются вектора добавок вида ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%3D1%7D%5E%7Bm-1%7D), по данным векторам добавок строится звёздная цепочка. Если для вектора добавок данной длины звёздная цепочка не найдена, увеличиваем вектор на одну позицию. На некотором шаге ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20m%20%5Cleq%20%5Clambda%28N%29%20&plus;%20%5Cnu%28N%29%20-%201) звёздная цепочка будет найдена. Таким образом необходимо выполнить ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Csum_%7Bm%20%3D%20%5Clambda%28N%29%7D%5E%7B%5Clambda%28N%29%20&plus;%20%5Cnu%28N%29%20-%201%7Dm%21) операций перебора. Это достаточно много и займет много времени.

Можно заметить, что при изменении последнего элемента вектора добавок с ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20m-1) до 1, то и последний элемент звёздной цепочки монотонно убывает. Таким образом, можно уменьшать не весь вектор добавок сразу, а, например, половину. Рассмотрим вектор добавок вида ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq%20%5Ccup%20%5Cleft%20%5C%7B%20%5Crho_j%20%5Cright%20%5C%7D_%7Bj%20%3D%20q%20&plus;%201%7D%5Em), где ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq) - фиксированный вектор, a ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Crho_j%20%3D%20%5Cleft%20%5C%7B%20x%20%3A%201%20%5Cleq%20x%20%5Cleq%20j%20%5Cright%20%5C%7D). Таких наборов получится ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cfrac%7Bm%21%7D%7Bq%21%7D) штук Монотонность при этом, конечно, исчезнет, но наибольшее значение ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_m) получится для вектора добавок ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq%20%5Ccup%20%5Cleft%20%5C%7B%20q%20&plus;%201%2C%20%5Cdots%2C%20m%20%5Cright%20%5C%7D) а наименьшее - для вектора добавок ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq%20%5Ccup%20%5Cleft%20%5C%7B%201%2C%20%5Cdots%2C%201%20%5Cright%20%5C%7D) Для того, чтобы вычислить максимальные и минимальные значения, достаточно знать ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%7Bq%20&plus;%201%7D) последнее число цепочки для вектора добавок ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq). Таким образом, ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%7B%5Cmin%7D%20%3D%20c_%7Bq%20&plus;%201%7D%20&plus;%20m%20-%20q), а ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%7B%5Cmax%7D%20%3D%20c_%7Bq%20&plus;%201%7D%20%5Ccdot%202%5E%7Bm%20-%20q%7D).

Алгоритм дробления вектора добавок:

1) Внешний цикл по длинам цепочек: ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cunderline%20l%28N%29%20%5Cleq%20l%28N%29%20%5Cleq%20%5Coverline%20l%28N%29). Выбирается число ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20q%2C%20%5C%2C%201%20%5Cleq%20q%20%5Cleq%20m%20-%201). Пусть ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20q%20%3D%20%5Cfrac%20m2).

2) Внутренний цикл перебора всех ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq) (![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20q%21) шагов). На каждом шаге при фиксированом ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq) находим ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%5Cmin) и ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%5Cmax):

a) Если ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_m%20%3D%20N) - задача решена;

b) Если ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) не находится между ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%5Cmin) и ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%5Cmax), то переходим к следующему набору ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq);

c) Если ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) находится между ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%5Cmin) и ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_%5Cmax), то организуем внутренний цикл перебора всех векторов добавок ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20%5Crho_j%20%5Cright%20%5C%7D_%7Bj%20%3D%20q%20&plus;%201%7D%5Em). Таких наборов ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cfrac%7Bm%21%7D%7Bq%21%7D);

d) Если обнаруживается ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_m%20%3D%20N) - задача решена;

e) Если в цикле таких векторов не оказалось, то переходим к следующему (по введённой упорядоченности) вектору добавок ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cleft%20%5C%7B%20r_i%20%5Cright%20%5C%7D_%7Bi%20%3D%201%7D%5Eq).

3) Если все наборы фиксированной длины исчерпаны, то увеличиваем их длину во внешнем цикле.

Данный алгоритм затрачивает меньше времени, чем для простого перебора векторов добавок, но тем не менее, достаточно велико. Модифицировать данный алгоритм можно с помощью выявления фиксированной длины звёздной цепочки с помощью теоремы описанной в [2]. Это значительно сократит перебор, особенно для особых случаев, ведь нам будет известна минимальная длина аддитивной цепочки, следовательно, нет причины перебирать вектор добавок меньшей и/или большей длины. Если число не удовлетворяет ни одному случаю из теоремы, то можно дробить вектор добавок не на 2 части, а на 3, 4 и т.д., в зависимости от ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Cunderline%20l%28N%29) и ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20%5Coverline%20l%28N%29), что тоже сокращает время поиска.

Некоторые оптимисты считают оправданным предположение о том, что ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29%20%3D%20l%5E*%28N%29) Однако, это оказалось неправдой. Трудно было поверить, что по данной аддитивной цепочке минимальной длины нельзя найти цепочку, той же длины, содержащую только звёздные шаги. В 1958 году Вальтер Хансен в [8] привёл доказательство теоремы о том, что для определённых больших значений ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) выполняется условие ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29%20%3C%20l%5E*%28N%29). Минимальное ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) для которого это условие выполняется равно 12509.

## Заключение

В ходе выполнения работы были разработаны и реализованы алгоритмы нахождения аддитивных цепочек минимальной длины для заданного числа. Разработанные в ходе работы алгоритм дробления вектора являются точными и уникальны тем, что они достаточно просты в реализации и легко программируемы. Данные алгоритмы работают только с звёздными цепочками. Как известно, для достаточно больших чисел звёздные цепочки теряют свою минимальность, но в рамках рассматриваемых чисел, можно полагать, что звёздная цепочка есть минимальная. Данный алгоритм выполняет свои задачи в точности и находят минимальные аддитивные цепочки за относительно короткое время, таким образом, можно полагать, что поставленная цель была достигнута. Данный алгоритм и/или его результаты можно использовать различных областях информатики, вычислительного и математического анализа, эллиптической криптографии. В ходе дальнейшей работы можно будет выявить некую закономерность между суммой элементов вектора добавки и заданным числом, модифицировать и совершенствовать алгоритмы, снижая время работы к минимуму, искать новые алгоритмы нахождения минимальных аддитивных цепочек для заданного числа.

## Список литературы

1. Гашков С. Б. Задача об аддитивных цепочках и ее обобщения // Математическое просвещение. 2011, вып. 15. С. 138-153.
2. Д. Кнут. Искусство программирования Т. 2. М.: Вильямс, 2001. 503-524 с.
3. Элементарное введение в эллиптическую криптографию: Алгебраические и алгоритмические основы / А. А. Болотов, С. Б. Гашков, А. Б. Фролов, А. А. Часовских. М.: КомКнига, 2006. 196-209 с.
4. Bergeron F., Berstel J., Brlek S. Efficient Computation of Addition Chains // Journal de Theorie des Nombres de Bordeaux. 1994, вып. 6. С. 21-38.
5. Brauer A. T. On addition chains // Bulletin of the American Mathematical Society. 1939, вып. 45. С. 736-739.
6. Yao A. C. On the evaluation of powers // SIAM Journal of Computing. 1993, вып. 5. С. 100-103.
7. Bernstein D. J. Pippenger’s exponentiation algorithm // URL: http://cr.yp.to/papers.html#pippenger. 2002. C. 21.
8. Hansen W. Zum Scholz-Brauerschen Problem // Journal für reine und angewandte Mathematik. 1959, вып. 202. С. 129–136.