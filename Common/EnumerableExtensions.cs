using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class EnumerableExtensions
{
    public static IEnumerable<T[]> Partition<T>(this IEnumerable<T> self, int size)
    {
        int i = 0;
        var result = new T[size];
        foreach (var item in self)
        {
            result[i++] = item;
            if (i == size)
            {
                yield return result;
                result = new T[size];
                i = 0;
            }
        }

        if (i != 0)
            throw new Exception("Input enumerable was not a multiple of (size)");
    }
}