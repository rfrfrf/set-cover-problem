using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MinMatrixCover
{
    [Flags]
    public enum OutputType
    {
        None = 0,
        ShowTmpSolution = 1,
        ShowBetterSolution = 1<<1,
        ShowResolvent = 1 << 2,
        ShowMatrixIteration = 1 << 3,
        ShowDetailedSolve = 1 << 4
    }

    public class Matrix
    {
        //матрица хранится, как список столбцов
        private List<int[]> _data;

        //индексы невычеркнутых строк
        protected List<int> _rowIndexes;
        //индексы невычеркнутых столбцов в порядке увеличения количества единиц
        protected List<int> _columnIndexes;

        //количество единиц в столбце
        protected float[] _columnValues;
        //количество единиц в строке
        protected float[] _rowValues;

        //решение, найденное на предыдущей итерации
        //prevSolution(i) - синдромный элемент: KeyValuePair<int, int> - key - строка, value - столбец
        private List<KeyValuePair<int, int>> prevSolution = null;

        //ширина матрицы, учитывая добавленные резольвенты
        protected int _width;
        protected int _height;
        //начальная ширина матрицы
        private int _widthBase;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public OutputType Output;

        //список улучшений решения: ResultDynamic(i) - пара key-номер итерации, value - количество строк в решении
        //для вывода итераций, на которых решение улучшается
        public List<KeyValuePair<int, float>> ResultDynamic { get; set; }

        //список весов строк (в данной задаче для всех строк вес равен единице)
        protected List<int> rowsWeight = new List<int>();

        public Matrix(int width, int height)
        {
            Init(width, height);
        }

        private void Init(int width, int height)
        {
            _width = width;
            _height = height;
            _data = new List<int[]>(_width);
            for (int i = 0; i < _width; i++)
            {
                _data.Add(new int[_height]);
            }
            //список весов строк (в данной задаче для всех строк вес равен единице)
            rowsWeight = Enumerable.Range(0, height).Select(x => 1).ToList();

            ClearIndexes();
        }

        /// <summary>
        /// Приводит матрицу к начальному виду (когда нет вычеркнутых строк и столбцов)
        /// </summary>
        protected void ClearIndexes()
        {
            _rowIndexes = Enumerable.Range(0, _height).ToList();
            _columnIndexes = Enumerable.Range(0, _width).ToList();
        }

        /// <summary>
        /// Возвращает элемент матрицы
        /// </summary>
        /// <param name="i">Номер столбца</param>
        /// <param name="j">Номер строки</param>
        /// <returns>Значение элемент матрицы</returns>
        public int GetItem(int i, int j)
        {
            return _data[i][j];
        }
        /// <summary>
        /// Устанавливает значение элемента матрицы
        /// </summary>
        /// <param name="i">Номер столбца</param>
        /// <param name="j">Номер строки</param>
        /// <param name="value"></param>
        /// <returns>истина</returns>
        public bool SetItem(int i, int j, int value)
        {
            _data[i][j] = value;
            return true;
        }

        /// <summary>
        /// Генерация матрицы заданной плотности
        /// </summary>
        /// <param name="density">Плотность</param>
        /// <param name="seed">Seed для генератора случайных чисел</param>
        public void GenerateMatrix(float density, int seed = 15)
        {
            int count = (int)Math.Round(_width * _height * density);
            Random random = new Random(seed);
            //пересчет вероятности с учетом того, что каждый столбец имеет как минимум одну единицу
            float probability1 = (_height * density - 1) / (_height - 1);

            _columnIndexes.ToList().ForEach(i =>
                {
                    int k = random.Next(_height);
                    //Чтобы гарантировать, что в каждом столбце есть как минимум одна единица
                    SetItem(i, k, 1);

                    _rowIndexes.Where(j0 => j0 != k).ToList().ForEach(j =>
                        {
                            int val = random.NextDouble() < probability1 ? 1 : 0;
                            SetItem(i, j, val);
                        });

                });
        }

        private int GetColumnWithMinValue()
        {
            int column = _columnIndexes.First();
            return column;
        }

        protected int GetRowWithMaxValue(int i)
        {
            int row = _rowIndexes
                //.AsParallel()
                .Where(j => (GetItem(i, j) == 1))
                .OrderByDescending(j => GetRowValue(j)).ThenBy(j => j)
                .First();

            return row;
        }

        /// <summary>
        /// Возвращает оптимальную строку  
        /// </summary>
        /// <param name="column">Возвращает номер синдромного столбца</param>
        /// <returns>Возвращает номер строки</returns>
        protected virtual int GetBestRow(out int column)
        {
            column = GetColumnWithMinValue();
            //взять строку с макимальным значением(количество единиц) из не вычеркнутых, покрывающую столбец 
            int row = GetRowWithMaxValue(column);

            return row;
        }

        protected float GetColumnValue(int i)
        {
            return _columnValues[i];
        }

        protected float GetRowValue(int j)
        {
            //в данном случае вес равено единице
            return _rowValues[j] / rowsWeight[j];
        }
        protected virtual float ComputeRowValue(int j)
        {
            //сумма единиц в строке j
            return _columnIndexes.ToList().Aggregate<int, float>(0.0f, (s2, i) => s2 + GetValue(i, j));
        }
        protected virtual float ComputeColumnValue(int i)
        {
            //сумма единиц в столбце
            return _rowIndexes.ToList().Aggregate(0, (s2, j) => s2 + GetItem(i, j));
        }
        protected virtual void GenColumnValues()
        {
            _columnValues = new float[_width];
            //считает значения для каждого столбца (в данном случае количество единиц в столбце)
            _columnIndexes.AsParallel().ForAll(i => _columnValues[i] = ComputeColumnValue(i));
        }
        protected virtual void GenRowValues()
        {
            _rowValues = new float[_height];
            //считает значения для каждой строки (в данном случае количество единиц в строке)
            _rowIndexes.AsParallel().ForAll(j => _rowValues[j] = ComputeRowValue(j));
        }
        protected virtual float GetValue(int i, int j)
        {
            return GetItem(i, j);
        }

        /// <summary>
        /// Удалить все столбцы, которые покрываются строкой j
        /// </summary>
        /// <param name="j">Номер строки</param>
        protected void RemoveAllByRow(int j)
        {
            var needRemove = _columnIndexes.Where(i => GetItem(i, j) == 1).ToList();
            needRemove.ForEach(i => RemoveColumn(i));

            RemoveRow(j);
        }

        /// <summary>
        /// Удалить все строки, которые покрывают столбец i
        /// </summary>
        /// <param name="j">Номер строки</param>
        protected void RemoveAllByColumn(int i)
        {
            var needRemove = _rowIndexes.Where(j => GetItem(i, j) == 1).ToList();
            needRemove.ForEach(j => RemoveRow(j));

        }

        /// <summary>
        /// Вычеркнуть строку
        /// </summary>
        /// <param name="j">Номер строки</param>
        private void RemoveRow(int j)
        {
            _rowIndexes.Remove(j);

            bool needOrder = false;
            _columnIndexes.ToList().ForEach(i =>
            {
                if (GetItem(i, j) == 1)
                {
                    _columnValues[i] -= GetValue(i, j);
                    needOrder = true;
                }
            });
            if (needOrder)
            {
                _columnIndexes = _columnIndexes
                    //.AsParallel()
                    .OrderByMy(i => GetColumnValue(i), i => i)
                    .ToList();
            }
        }

        /// <summary>
        /// Вычеркнуть столбец
        /// </summary>
        /// <param name="i">Номер столбца</param>
        private void RemoveColumn(int i)
        {
            _columnIndexes.Remove(i);

            _rowIndexes.ToList().ForEach(j =>
            {
                if (GetItem(i, j) == 1)
                {
                    _rowValues[j] -= GetValue(i, j);
                }
            });
        }

        protected virtual int GetSyndromRow(out int i)
        {
            //взять отпимальную строку row и столбец j
            int row = GetBestRow(out i);
            //проверить, что решение на текущей итерации идет по пути, не хуже предыдушего
            if (prevSolution != null && prevSolution.Count > 0 && GetColumnValue(i) >= GetColumnValue(prevSolution.First().Value))
            {
                row = prevSolution.First().Key;
                i = prevSolution.First().Value;
                prevSolution.RemoveAt(0);
            }
            else
            {
                prevSolution = null;
            }
            //удаляем все столбцы, покрываемые синдромной строкой!
            RemoveAllByRow(row);
            //удаляем все строки, которые покрывают синдромный столбец!
            RemoveAllByColumn(i);
            //удаляем пустые строки
            RemoveEmptyRows();
            return row;
        }

        protected bool Empty()
        {
            return _rowIndexes.Count == 0 || _columnIndexes.Count == 0;
        }

        protected void RemoveEmptyRows()
        {
            _rowIndexes.ToList().ForEach(j =>
            {
                if (!_columnIndexes.Any(i => GetItem(i, j) == 1))
                {
                    RemoveRow(j);
                }
            });
        }

        /// <summary>
        /// Решить матрицу
        /// </summary>
        /// <returns>Решение: список синдромных элементов - key - синдромная строка, value - синдромный столбец</returns>
        public virtual List<KeyValuePair<int, int>> Solve()
        {
            List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();
            GenColumnValues();
            GenRowValues();

            RemoveEmptyRows();

            //сортируем столбцы в порядке приоритета (увеличения единиц)
            _columnIndexes = _columnIndexes
                //.AsParallel()
                .OrderBy(i => GetColumnValue(i)).ThenBy(i => i)
                .ToList();

            //пока матрица не разрушена, продолжаем итерации
            while (!Empty())
            {
                if (Output.HasFlag(OutputType.ShowDetailedSolve))
                {
                    Console.WriteLine(this);
                }
                int column;
                int row = GetSyndromRow(out column);
                if (Output.HasFlag(OutputType.ShowDetailedSolve))
                {
                    Console.WriteLine("Syndrom item: row: {0}, column: {1})",row, column);
                }
                KeyValuePair<int, int> syndromElement = new KeyValuePair<int, int>(row, column);
                result.Add(syndromElement);
                
            }
            //Console.WriteLine("C:{0}", GetOFV(result));

            return result;
        }

        /// <summary>
        /// Значение целевой функции для данного решения(сумма весов строк решения)
        /// В нашем примере возвращает количество строк в решении
        /// </summary>
        /// <param name="solution"></param>
        /// <returns>Значение целевой функции</returns>
        public float GetOFV(List<KeyValuePair<int, int>> solution)
        {
            return solution.Sum(x => rowsWeight[x.Key]);
        }

        /// <summary>
        /// Проверка, что решение покрывает все столбцы
        /// </summary>
        /// <param name="syndromElements">Решение - синдромные элементы</param>
        /// <returns>True, если строки покрывают все столбцы</returns>
        public bool Check(List<KeyValuePair<int, int>> syndromElements)
        {
            bool[] check = new bool[_widthBase];
            foreach (var element in syndromElements)
            {
                for (int i = 0; i < _widthBase; i++)
                {
                    check[i] = check[i] || (GetItem(i, element.Key) == 1);
                }
            }
            bool result = check.All(x => x);
            return result;
        }

        /// <summary>
        /// Вывести строки решения
        /// </summary>
        /// <param name="syndromElements">синдромные элементы</param>
        /// <returns>Решение</returns>
        public string Print(List<KeyValuePair<int, int>> syndromElements)
        {
            StringBuilder result = new StringBuilder();
            foreach (var element in syndromElements)
            {
                result.AppendFormat("{0} ", element.Key);
                for (int i = 0; i < _width; i++)
                {
                    result.AppendFormat("{0} ", GetItem(i, element.Key));
                }
                result.AppendFormat(" ({0}) ", element.Value);
                result.AppendLine();
            }
            return result.ToString();
        }

        /// <summary>
        /// Вывести матрицу
        /// </summary>
        /// <returns>текстовое представление матрицы</returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.Append("   ");

            
            for (int i = 0; i < _width; i++)
            {
                if (!_columnIndexes.Contains(i))
                    continue;
                result.AppendFormat("|{0,3}", i);
            }
            result.AppendLine();

            
            for (int j = 0; j < _height; j++)
            {
                if (!_rowIndexes.Contains(j))
                    continue;

                result.AppendFormat("{0,3}|", j);
                for (int i = 0; i < _width; i++)
                {
                    if (!_columnIndexes.Contains(i))
                        continue;
                    result.AppendFormat("{0,3} ", GetItem(i, j));
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        /// <summary>
        /// Генерирует столбец-резольвенту
        /// </summary>
        /// <param name="syndromElements">Текущее решение</param>
        /// <param name="r">Количество строк в лучшем решении</param>
        /// <returns></returns>
        public List<int> GetResolvent(List<KeyValuePair<int, int>> syndromElements, int r)
        {
            List<int> result = new List<int>(_height);

            int count = Math.Min(Math.Min(_height / 2 + 1, r), syndromElements.Count);
            for (int j = 0; j < _height; j++)
            {
                int sum = 0;
                for (int k = 0; k < count; k++)
                {
                    int i = syndromElements[k].Value;
                    sum += GetItem(i, j);
                }
                if (sum >= 2)
                {
                    result.Add(1);
                }
                else
                {
                    result.Add(0);
                }
            }
            return result;
        }

        public List<KeyValuePair<int, int>> Solve2(int count = 10000)
        { 
            int state;
            int iter;
            return Solve2(count, out state, out iter);
        }


        /// <summary>
        /// Усиленный метод групповый резолюций (с заменой резольвент)
        /// </summary>
        /// <param name="count">Максимальное количество итераций</param>
        /// <param name="state">Состояние, в котором завершилось выполнение алгоритма
        /// 0 - превышено максимальное количество итераций
        /// 1 - Резольвента повторилась, алгоритм не останавливается
        /// 2 - Резольвента не содержит единиц - алгоритма сошелся
        /// </param>
        /// <param name="iterationCount">Возвращает номер итерации, на которой алгоритм сошелся</param>
        /// <returns>Решение - пары синдромных  строки и столбца</returns>
        public virtual List<KeyValuePair<int, int>> Solve2(int count, out int state, out int iterationCount)
        {
            List<KeyValuePair<int, int>> result = null;
            List<KeyValuePair<int, int>> solution = new List<KeyValuePair<int, int>>();
            ResultDynamic = new List<KeyValuePair<int, float>>();
            state = 0;
            int widthBase = _width;
            _widthBase = _width;

            List<int> resolvent;
            List<List<int>> resolventsList = new List<List<int>>();
            int i;
            for (i = 0; i < count; i++)
            {
                ClearIndexes();
                prevSolution = new List<KeyValuePair<int, int>>();
                prevSolution.AddRange(solution);

                if (Output.HasFlag(OutputType.ShowMatrixIteration))
                {
                    Console.WriteLine("Iteration: {0}", i);
                    Console.WriteLine(this);
                }

                //решение на текущей итерации
                solution = Solve();
                var ofv = GetOFV(solution);
                if (Output.HasFlag(OutputType.ShowTmpSolution))
                {
                    Console.WriteLine("sol{0}: OFV:{1}: {2} ", i, ofv,
                        String.Concat(
                            solution.Select(x => string.Format("{0}:{1}, ", x.Key, x.Value))));
                }
                //запоминаем лучшее решение
                if (result == null || GetOFV(result) > GetOFV(solution))
                {
                    result = solution;
                    ResultDynamic.Add(new KeyValuePair<int, float>(i, GetOFV(solution)));
                    if (Output.HasFlag(OutputType.ShowBetterSolution) && !Output.HasFlag(OutputType.ShowTmpSolution))
                    {
                        Console.WriteLine("sol{0}: OFV:{1}: {2} ", i, ofv,
                            String.Concat(
                                solution.Select(x => string.Format("{0}:{1}, ", x.Key, x.Value))));
                    }
                    //if (solution.Count <= 15) { break; }
                }
                //генерируем резольвенту
                resolvent = GetResolvent(solution, result.Count);

                //добавляем резольвенту в resolventsList, если такой еще не было
                if (resolventsList.Any(x => x.SequenceEqual(resolvent)))
                {
                    //Console.WriteLine("Resolvent exists");
                    state = 1;
                }
                else
                {
                    resolventsList.Add(resolvent);
                }

                int index;
                //столбцы-резольвенты, которые можно исключить
                var freeIndexes = Enumerable.Range(widthBase, _width - widthBase).Where(x => !solution.Any(y => y.Value == x)).ToList();
                if (freeIndexes.Count > 0 && (_width - widthBase) > result.Count)
                {
                    index = freeIndexes.OrderByDescending(x => GetColumnValue(x)).First();
                    //заменяем одну из старых резольвент на новую
                    _data[index] = resolvent.ToArray();
                }
                else
                {
                    //добавляем новую резольвенту
                    _data.Add(resolvent.ToArray());
                    index = _width;
                    _width++;

                }
                // Console.WriteLine("S:{0} R:{1}", solution.Count, resolvent.Sum());
                if (Output.HasFlag(OutputType.ShowResolvent))
                {
                    Console.WriteLine("Resolvent[{1}] {0}", String.Concat(
                            resolvent.Select(x => x)), index);
                }
                if (!resolvent.Contains(1))
                {
                    //прерываем алгоритм, если получили нулевую резольвенту
                    Console.WriteLine("Don't contains 1");
                    state = 2;

                    break;
                }

            } //while (resolvent.Contains(1));
            ResultDynamic.Add(new KeyValuePair<int, float>(i, GetOFV(result)));
            iterationCount = i;

            var check = result.Select(j => _data.Select(l => l[j.Key]).ToList()).ToList();
            _width = _widthBase;
            _data.RemoveRange(_width, _data.Count - _width);
            _data.RemoveRange(_width, _data.Count - _width);
            //Console.WriteLine("Number of iterations {0}", i);
            return result;
        }

        /// <summary>
        /// Чтение матрицы (с весами) из файла вида
        /// http://people.brunel.ac.uk/~mastjjb/jeb/orlib/scpinfo.html
        /// </summary>
        /// <param name="path"></param>
        public void ReadFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);
            var tmp = lines.SelectMany(l => l.Split(' ')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            int[] elements = tmp.Select(x => int.Parse(x)).ToArray();
            int pos = 0;
            int w = (elements[pos++]);
            int h = (elements[pos++]);

            Init(w, h);
            rowsWeight = new List<int>();

            while (rowsWeight.Count < h)
            {
                rowsWeight.Add(elements[pos++]);
            }

            for (int i = 0; i < w; i++)
            {
                int n = elements[pos++];
                for (int k = 0; k < n; k++)
                {
                    int j = elements[pos++] - 1;
                    SetItem(i, j, 1);
                }
            }
        }

        /// <summary>
        /// Генерация матрицы с заданным минимальным покрытием
        /// </summary>
        /// <param name="density">Плотность единиц</param>
        /// <param name="minCover">Заданное минимальное покрытие</param>
        /// <param name="seed">Seed для генератора случайных чисел</param>
        public void GenerateMatrixForTest(float density, int minCover, int seed = 15)
        {
            int count = (int)Math.Round(_width * _height * density);
            Random random = new Random(seed);

            //fill left top square
            for (int i = 0; i < minCover; i++)
            {
                SetItem(i, i, 1);
            }
            count -= minCover;

            //fill left bottom rectangle
            float countPerMinCoverRow = minCover * density;

            for (int j = minCover; j < _height; j++)
            {
                if (countPerMinCoverRow > 1)
                {
                    SetItem(random.Next(minCover), j, 1);
                    count--;
                }
                else
                {
                    if (random.NextDouble() > countPerMinCoverRow)
                    {
                        SetItem(random.Next(minCover), j, 1);
                        count--;
                    }
                }
            }

            //fill right top rectangle
            for (int i = minCover; i < _width; i++)
            {
                SetItem(i, random.Next(minCover), 1);
                count--;
            }

            //fill other part of matrix
            float probabitilyPart = ((float)count) / ((_width - minCover) * (_height - minCover));
            for (int i = minCover; i < _width; i++)
            {
                for (int j = minCover; j < _height; j++)
                {
                    if (random.NextDouble() < probabitilyPart)
                    {
                        SetItem(i, j, 1);
                    }
                }
            }
        }

        //Задайся некоторым набором строк (в каждом примере он должен быть разным), например,
        //3,6,7,12,22, 33,35. 
        //Задайся некоторым набором строк (в каждом примере он должен быть разным), например,
        //3,6,7,12,22, 33,35. Считай, что это и есть
        //минимальное покрытие. Генерируй матрицу случайно, но так чтобы в каждом столбце генерируемой матрицы
        //как минимум одна из строк  3,6,7,12,22, 33,35 содержала в нем  единицу. Кроме того, генерируй матрицы с низкой плотностью единиц,
        //т.е. такие, где алгоритм не сходится за очень большое число итераций.

        /// <summary>
        /// Способ2: Генерация матрицы с заданным минимальным покрытием (покрытие может быть и меньше заданого)
        /// </summary>
        /// <param name="density">Плотность единиц</param>
        /// <param name="minCover">Заданное минимальное покрытие</param>
        /// <param name="seed">Seed для рандома</param>
        public void GenerateMatrixForTest2(float density, int minCover, int seed = 15)
        {
            int count = (int)Math.Round(_width * _height * density);
            Random random = new Random(seed);

            List<int> coverRows = new List<int>();
            for (int i = 0; i < minCover; i++)
            {
                int rnd = random.Next(1, _width);
                if (!coverRows.Contains(rnd))
                {
                    coverRows.Add(rnd);
                }
                else
                {
                    i--;
                }
            }

            this.GenerateMatrix(density, seed);
            for (int i = 0; i < _width; i++)
            {
                bool contains = false;
                foreach (var row in coverRows)
                {
                    if (GetItem(i, row) == 1)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    SetItem(i, coverRows[random.Next(coverRows.Count)], 1);
                }
            }

        }


    }
}

