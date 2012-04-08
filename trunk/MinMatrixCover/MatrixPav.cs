using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinMatrixCover
{
    public class MatrixPav : Matrix
    {
        public MatrixPav(int width, int height)
            : base(width, height)
        { 
        
        }

        protected override float GetValue(int i, int j)
        {
            return GetItem(i, j) * GetColumnValue(i);
        }

        //protected override float ComputeRowValue(int j)
        //{
        //    float result = _columnIndexes.ToList()
        //        .Aggregate<int, float>(0, 
        //            (s2, i) => s2 + GetItem(i, j)*(GetColumnValue(i))
        //        );
        //    return result;
        //}
        protected override int GetBestRow(out int column)
        {
            int row = _rowIndexes.AsParallel()                
                .OrderByDescending(j => GetRowValue(j))
                .First();
            column = _columnIndexes.First(i => GetItem(i, row) == 1);
            return row;
        }

        protected override int GetSyndromRow(out int i)
        {
            //i = GetColumnWithMinValue();
            //int row = GetRowWithMaxValue(i);
            int row = GetBestRow(out i);
            RemoveAllByRow(row);

            GenColumnValues();
            GenRowValues();

            RemoveEmptyRows();


            return row;
        }

        protected override float ComputeRowValue(int j)
        {

            return (base.ComputeRowValue(j) / rowsWeight[j] );// / _columnIndexes.Count(i0 => GetItem(i0, j) == 1);
        }

        protected override float ComputeColumnValue(int i)
        {
            var workers = _rowIndexes.Where(j => GetItem(i, j) == 1);
            float cost = 1.0f;// workers.Aggregate(0, (s, j) => s + rowsWeight[j] / _columnIndexes.Count(i0 => GetItem(i0, j) == 1));
            cost = cost / workers.Count();
            return cost;
        }


        //protected override void GenColumnValues()
        //{
        //    _columnValues = new float[_columnIndexes.Count];

        //    _columnIndexes.AsParallel().ForAll(i => _columnValues[i] = 1.0f/ComputeColumnValue(i));
        //}
    }
}
