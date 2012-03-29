using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinMatrixCover
{
    public class MatrixAnt : MatrixPav
    {
        protected float[] _feramonsValues;
        Random _rnd = new Random();



        public MatrixAnt(int width, int height)
            : base(width, height)
        { 
        
        }
        protected override int GetBestRow(out int column)
        {
            float kof = 0.98f;

            int row = _rowIndexes
                .Select(x => new { j = x, prior = 0.0 + 1 * _rnd.NextDouble() })
                .AsParallel().OrderByDescending(x =>  
                    (float)(
                        //GetRowValue(x.j)* kof + ( _feramonsValues[x.j]*( 1 - kof)) * x.prior)
                        GetRowValue(x.j)* (kof + (( 1 - kof)) * x.prior))

                        )
                .First().j;
            //float kof = 0.5f;
            ////float sum = _rowIndexes.Sum(j => (float)(Math.Pow(GetRowValue(j),kof)*Math.Pow(_feramonsValues[j], 1-kof)));
            //float sum = _rowIndexes.Sum(j => (float)((GetRowValue(j)* kof) * (_feramonsValues[j]*( 1 - kof))));
            
            //float rndV = (float)_rnd.NextDouble() * sum;
            //int row = _rowIndexes.First();
            //foreach (int j in _rowIndexes)
            //{
            //    //rndV -= (float)(Math.Pow(GetRowValue(j), kof) * Math.Pow(_feramonsValues[j], 1 - kof));
            //    rndV -= (float)((GetRowValue(j) * kof) * (_feramonsValues[j] * (1 - kof)));
            //    if (rndV <= 0)
            //    {
            //        row = j;
            //        break;
            //    }
            //}
            _feramonsValues[row] -= GetRowValue(row);
            column = 0;
            return row;
        }

        public override List<KeyValuePair<int, int>> Solve()
        {
            List<KeyValuePair<int, int>> result = null;
            List<KeyValuePair<int, int>> solution;
            _feramonsValues = new float[_height];
            GenColumnValues();
            GenRowValues();
            _rowIndexes.ForEach(j => _feramonsValues[j] = 0*GetRowValue(j));

            for (int i = 0; i < 30; i++)
            {
                ClearIndexes();
                solution = base.Solve();
                if (result == null || result.Count > solution.Count)
                {
                    result = solution;
                }
               // Console.Write("{0} ", solution.Count);

                //_rowIndexes.ForEach(j => _feramonsValues[j] *= 0.95f);

            }
            return result;
        }
    }
}
