using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinMatrixCover
{
    public class MatrixPav2 : MatrixPav
    {

        protected class State
        {
            private List<int> _rows;
            private float _value;

            public float Value { get { return _value; } protected set {_value=value;} }
            public List<int> Rows { get { return _rows; } }

            public State()
            {
                _rows = new List<int>();
                _value = 0;
            }

            public void AddRow(int row, float value)
            {
                _rows.Add(row);
                _value += value;
            }

            public State Clone()
            {
                State result = new State();
                result.Value = this.Value;
                result.Rows.AddRange(this.Rows);
                return result;
            }
        }

        private List<State> states;

         public MatrixPav2(int width, int height)
            : base(width, height)
        { 
        
        }

        public override List<KeyValuePair<int, int>> Solve()
        {
            List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();
            GenColumnValues();
            GenRowValues();

            RemoveEmptyRows();
            _columnIndexes = _columnIndexes.AsParallel()
                .OrderBy(i => GetColumnValue(i))
                .ToList();

            states = new List<State>();
            State state = new State();
            states.Add(state);
            Random rnd = new Random(15);
            while (!Empty())
            {
                State state2 = state.Clone();
                
                int column;
                int row = GetBestRow(out column);
                
                state.AddRow(row, GetRowValue(row));
                states.Add(state);

                row = GetBest2Row(out column);

                state2.AddRow(row, GetRowValue(row));
                states.Add(state2);

                state = states.OrderByDescending(x => x.Value *(float)(0.75 + 0.25 * rnd.NextDouble())).First();
                states.Remove(state);
                LoadState(state);
            }

            result = state.Rows.Select(x => new KeyValuePair<int, int>(x, 0)).ToList();
            //while (!Empty())
            //{
            //    int column;
            //    int row = GetSyndromRow(out column);

            //    KeyValuePair<int, int> syndromElement = new KeyValuePair<int, int>(row, column);
            //    result.Add(syndromElement);
            //}
            return result;
        }

        protected int GetBest2Row(out int column)
        {
            int row = _rowIndexes.AsParallel()
                .OrderByDescending(j => GetRowValue(j))
                .Skip(1)
                .First();
            column = 0;
            return row;
        }


        protected void LoadState(State state)
        {
            ClearIndexes();

            GenColumnValues();
            GenRowValues();
            foreach (int row in state.Rows)
            {
                RemoveAllByRow(row);
            }
        }

    }
}
