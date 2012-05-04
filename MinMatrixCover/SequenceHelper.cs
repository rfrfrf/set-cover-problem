using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinMatrixCover
{
    static class SequenceHelper
    {

        public static IEnumerable<T> OrderByMy<T>(this IEnumerable<T> data, Func<T, float> key1, Func<T, float> key2)
        {
            return data.GroupBy(key1).OrderBy(g => g.Key).SelectMany(gr => gr.OrderBy(key2));
        }
    }
}
