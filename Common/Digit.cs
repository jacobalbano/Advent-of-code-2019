using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Digit
{
    public static int CountDigits(this int self) => self == 0 ? 0 : CountDigits(self / 10) + 1;
    public static long CountDigits(this long self) => self == 0 ? 0 : CountDigits(self / 10) + 1;

    public static int GetDigit(this int self, int digit) => self / pow10[digit] % 10;

    public static int ReadDigits(this int self, int count) => self.SplitDigits()
        .Take(count)
        .Select((x, i) => x * pow10[i])
        .Aggregate(0, (x, accum) => x + accum);

    public static IEnumerable<int> SplitDigits(this int self)
    {
        for (int i = 0, max = self.CountDigits(); i < max; i++)
            yield return self.GetDigit(i);
    }
    
    static readonly int[] pow10 = Enumerable.Range(0, 10)
        .Select(x => (int)Math.Pow(10, x))
        .ToArray();
}
