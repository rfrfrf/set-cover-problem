using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MinMatrixCover
{
    public class Matrix
    {
        //private int[,] _data;
        private List<int[]> _data;
        private bool[] _rowRemoved;
        private bool[] _collumnRemoved;
        private int _rowExistsCount;
        private int _columnExistsCount;
        protected List<int> _rowIndexes;
        protected List<int> _columnIndexes;
        //private IEnumerable<int> _rowIndexes0;
        //private IEnumerable<int> _columnIndexes0;

        protected float[] _columnValues;
        protected float[] _rowValues;


        protected int _width;
        protected int _height;

        protected List<int> rowsWeight = new List<int>();

        public Matrix(int width, int height)
        {
            Init(width, height);
        }

        private void Init(int width, int height)
        {
            _width = width;
            _height = height;
            // _data = new int[_width, _height];
            _data = new List<int[]>(_width);
            for (int i = 0; i < _width; i++)
            {
                _data.Add(new int[_height]);
            }
            rowsWeight = Enumerable.Range(0,height).Select(x=>1).ToList();
            
            ClearIndexes();
        }

        protected void ClearIndexes()
        {
            _rowRemoved = new bool[_height];
            _collumnRemoved = new bool[_width];
            _rowExistsCount = _height;
            _columnExistsCount = _width;

           // _rowIndexes0 = Enumerable.Range(0, _height);
            //_columnIndexes0 = Enumerable.Range(0, _width);
            //_rowIndexes = _rowIndexes0.Where(IsRowExists);
            //_columnIndexes = _columnIndexes0.Where(IsColumnExists);
            _rowIndexes = Enumerable.Range(0, _height).ToList();
            _columnIndexes = Enumerable.Range(0, _width).ToList();
        }

        public int GetItem(int i, int j)
        {
            return _data[i][j];
        }
        public bool SetItem(int i, int j, int value)
        {
            _data[i][j] = value;
            return true;
        }

        public bool IsRowExists(int i)
        {
            return !_rowRemoved[i];
        }
        public bool IsColumnExists(int j)
        {
            return !_collumnRemoved[j];
        }

        public void GenerateMatrix(float density, int seed = 15)
        {
            int count = (int)Math.Round(_width * _height * density);
            Random random = new Random(seed);
            float probability1 = (_height * density - 1) / (_height - 1);//_width*_height*density/(_width*_height + _width - _height);//density - 1.0f / _height;

            _columnIndexes.ToList().ForEach(i =>
                {
                    int k = random.Next(_height);
                    SetItem(i, k, 1);

                    _rowIndexes.Where(j0 => j0 != k).ToList().ForEach(j =>
                        {
                            int val = random.NextDouble() < probability1 ? 1 : 0;
                            SetItem(i, j, val);
                        });

                });
            //int count2 = _columnIndexes.ToList().Aggregate(0, (s0, i) => s0 + _rowIndexes.ToList().Aggregate(0, (s2, j) => s2 + GetItem(i, j)));
            //Console.WriteLine("count1 {0}", count);
            //Console.WriteLine("count2 {0}", count2);
            //Console.WriteLine("diff {0} %", (count - count2) * 100.0f / count);
        }

        private int GetColumnWithMinValue()
        {
            //_columnIndexes0 = _columnIndexes.AsParallel().OrderBy(i => GetColumnValue(i)).ToList();
            //int column = _columnIndexes.First();
            //int column = _columnIndexes.AsParallel()
            //    .OrderBy(i => GetColumnValue(i))
            //    .First();
            int column = _columnIndexes.First();
            return column;
        }

        protected int GetRowWithMaxValue(int i)
        {
            //_rowIndexes0 = _rowIndexes.AsParallel().OrderByDescending(j => GetRowValue(j)).ToList();
            //int row = _rowIndexes.First(j => (GetItem(i, j) == 1));
            var tmpDebug = _rowIndexes
                .Where(j => (GetItem(i, j) == 1)).ToList();
            int row = _rowIndexes.AsParallel()
                .Where(j => (GetItem(i, j) == 1))
                .OrderByDescending(j => GetRowValue(j))
                .First();
   
            return row;
        }

        protected virtual int GetBestRow(out int column)
        {
            column = GetColumnWithMinValue();
            int row = GetRowWithMaxValue(column);
            return row;
        }

        protected float GetColumnValue(int i)
        {
            return _columnValues[i];             
        }
        protected float GetRowValue(int j)
        {
            //return _columnIndexes.ToList().Aggregate(0, (s2, i) => s2 + GetItem(i, j));
            return _rowValues[j]/rowsWeight[j];             
        }
        //number 1 in row
        protected virtual float ComputeRowValue(int j)
        {
            return _columnIndexes.ToList().Aggregate<int, float>(0.0f, (s2, i) => s2 + GetValue(i, j));
        }
        protected virtual float ComputeColumnValue(int i)
        {
            return _rowIndexes.ToList().Aggregate(0, (s2, j) => s2 + GetItem(i, j));
        }
        protected virtual void GenColumnValues()
        { 
            _columnValues = new float[_width];

            _columnIndexes.AsParallel().ForAll(i => _columnValues[i] = ComputeColumnValue(i));
        }
        protected virtual void GenRowValues()
        {
            _rowValues = new float[_height];

            _rowIndexes.AsParallel().ForAll(j => _rowValues[j] = ComputeRowValue(j));
        }
        protected virtual float GetValue(int i, int j)
        {
            return GetItem(i, j);
        }

        protected void RemoveAllByRow(int j)
        {
            //_columnIndexes.ToList().ForEach(i =>
            //{
            //    if (GetItem(i, j) == 1)
            //    {
            //        RemoveColumn(i);
            //    }
            //});
            var needRemove = _columnIndexes.Where(i => GetItem(i, j) == 1).ToList();
            needRemove.ForEach(i => RemoveColumn(i));

            RemoveRow(j);
        }

        protected void RemoveAllByColumn(int i)
        {
            //_rowIndexes.ToList().ForEach(j =>
            //{
            //    if (GetItem(i, j) == 1)
            //    {
            //        RemoveRow(j);
            //    }
            //});

            var needRemove = _rowIndexes.Where(j => GetItem(i, j) == 1).ToList();
            needRemove.ForEach(j => RemoveRow(j));

        }

        private void RemoveRow(int j)
        {
            _rowRemoved[j] = true;
            _rowExistsCount--;
            _rowIndexes.Remove(j);
        }

        private void RemoveColumn(int i)
        {
            _collumnRemoved[i] = true;
            _columnExistsCount--;
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
            //i = GetColumnWithMinValue();
            //int row = GetRowWithMaxValue(i);
            int row = GetBestRow(out i);
            RemoveAllByRow(row);
           // RemoveAllByColumn(i);//
            RemoveEmptyRows();
            return row;
        }

        protected bool Empty()
        {
            return _rowExistsCount == 0 || _columnExistsCount == 0;
        }

        protected void RemoveEmptyRows()
        {
            _rowIndexes.ToList().ForEach(j =>
            {
                //if (GetRowValue(j) == 0)

                if (!_columnIndexes.Any(i => GetItem(i, j) == 1))
                {
                    RemoveRow(j);
                }
            });
        }

        public virtual List<KeyValuePair<int, int>> Solve()
        {
            List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();
            GenColumnValues();
            GenRowValues();

            RemoveEmptyRows();
            _columnIndexes = _columnIndexes.AsParallel()
                .OrderBy(i => GetColumnValue(i))
                .ToList();

            while (!Empty())
            {
                int column;
                int row = GetSyndromRow(out column);
                //RemoveEmptyRows();

                KeyValuePair<int, int> syndromElement = new KeyValuePair<int, int>(row, column);
                result.Add(syndromElement);
            }
            return result;
        }

        public float GetOFV(List<KeyValuePair<int, int>> solution)
        {
            return solution.Sum(x => rowsWeight[x.Key]);
        }


        public bool Check(List<KeyValuePair<int, int>> syndromElements)
        {
            bool[] check = new bool[_width];
            //syndromElements.RemoveAt(new Random().Next(syndromElements.Count));
            foreach (var element in syndromElements)
            {
                for (int i = 0; i < _width; i++)
                {
                    check[i] = check[i] || (GetItem(i, element.Key) == 1);
                }
            }
            bool result = check.All(x => x);
            return result;
        }

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

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.Append("  0 1 2 3 4 5 6 7 8 9").AppendLine();
            for (int j = 0; j < _height; j++)
            {
                result.AppendFormat("{0} ",j);
                for (int i = 0; i < _width; i++)
                {
                    result.AppendFormat("{0} ", GetItem(i, j));
                }
                result.AppendLine();
            }
            return result.ToString();
        }

        public List<int> GetResolvent(List<KeyValuePair<int, int>> syndromElements, int r)
        {
            List<int> result = new List<int>(_height);

            int count = Math.Min(Math.Min(_height / 2 +1,r), syndromElements.Count);
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

        public virtual List<KeyValuePair<int, int>> Solve2(int count)
        {
            List<KeyValuePair<int, int>> result = null;
            List<KeyValuePair<int, int>> solution = new List<KeyValuePair<int, int>>();

            int widthBase = _width;

            List<int> resolvent;
            for (int i = 0; i < count; i++)
           
          //  do
            {
                ClearIndexes();
                solution = Solve();
                //Console.WriteLine("solution {0}: {1} ", i,
                //    String.Concat(
                //        solution.Select(x => string.Format("{0}:{1}, ", x.Key, x.Value))));

                if (result == null || GetOFV(result) > GetOFV(solution))
                //if (result == null || result.Count > solution.Count)

                {
                    result = solution;
                }
                resolvent = GetResolvent(solution, result.Count);

                var freeIndexes= Enumerable.Range(widthBase, _width - widthBase).Where(x => !solution.Any(y => y.Value == x)).ToList();
                if (freeIndexes.Count > 0 && (_width - widthBase) > result.Count)
                {
                    _data[freeIndexes.OrderByDescending(x=>GetColumnValue(x)).First()] = resolvent.ToArray();
                }
                else
                {
                    _data.Add(resolvent.ToArray());
                    _width++;
                }
               // Console.WriteLine("S:{0} R:{1}", solution.Count, resolvent.Sum());

                if (!resolvent.Contains(1))
                {
                    break;
                }
                
            } //while (resolvent.Contains(1));

            var check = result.Select(j => _data.Select(l => l[j.Key]).ToList()).ToList();
            return result;
        }

        public void ReadFromFile(string path)
        {
            string[] lines=File.ReadAllLines(path);
            var tmp = lines.SelectMany(l => l.Split(' ')).Where(x=>!string.IsNullOrWhiteSpace(x)).ToList();
            int[] elements = tmp.Select(x=>int.Parse(x)).ToArray();
            int pos = 0;
            int w = (elements[pos++]);
            int h = (elements[pos++]);

            Init(w, h);
            rowsWeight = new List<int>();

            while (rowsWeight.Count < h)
            {
                rowsWeight.Add(elements[pos++]);
                //rowsWeight.Add(1); 
                //pos++;

            }

            for (int i = 0; i < w; i++)
            {
                int n = elements[pos++];
                for (int k = 0; k < n; k++)
                {
                    int j = elements[pos++]-1;
                    SetItem(i, j, 1);
                }
            }



        }

    }
}

