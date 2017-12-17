## Сравнение аналогов

Аддитивной цепочкой для некоторого числа ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N) называют последовательность натуральных чисел

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%201%20%3D%20c_0%2C%20c_1%2C%20%5Cdots%20%2Cc_m%20%3D%20N),

обладающих тем свойством, что

![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20c_i%20%3D%20c_j%20&plus;%20c_k)

для некоторых ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20k%5Cleq%20j%20%3C%20i%2C%20%5Cforall%20i%3D%5Coverline%7B1..m%7D).

Данное понятие было придумано Арнольдом Шольцом. Наименьшая длина аддитивной цепочки, заканчивающаяся числом ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20N), обозначается как ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%28N%29%20%3D%20m).

Например, {1, 2, 3, 5, 7, 14} - минимальная цепочка для 14, т.е. ![equation](https://latex.codecogs.com/gif.latex?%5Cdpi%7B120%7D%20%5Clarge%20l%2814%29%20%3D%205).

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

## Источники

1. Гашков С. Б. Задача об аддитивных цепочках и ее обобщения // Математическое просвещение. 2011, вып. 15. С. 138-153.

2. Д. Кнут. Искусство программирования Т. 2. М.: Вильямс, 2001. 503-524 с.

3. Элементарное введение в эллиптическую криптографию: Алгебраические и алгоритмические основы / А. А. Болотов, С. Б. Гашков, А. Б. Фролов, А. А. Часовских. М.: КомКнига, 2006. 196-209 с.

4. Bergeron F., Berstel J., Brlek S. Efficient Computation of Addition Chains // Journal de Theorie des Nombres de Bordeaux. 1994, вып. 6. С. 21-38.

5. Brauer A. T. On addition chains // Bulletin of the American Mathematical Society. 1939, вып. 45. С. 736-739.

6. Yao A. C. On the evaluation of powers // SIAM Journal of Computing. 1993, вып. 5. С. 100-103.

7. Bernstein D. J. Pippenger’s exponentiation algorithm // URL: http://cr.yp.to/papers.html#pippenger. 2002. C. 21.