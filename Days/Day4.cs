using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day4 : DayBase
    {
        public override string Part1(string input)
        {
            return Solve(input, IsCandidate1).ToString();
        }

        public override void Part1Test()
        {
            Debug.Assert(IsCandidate1(0, int.MaxValue, 111111));
            Debug.Assert(!IsCandidate1(0, int.MaxValue, 223450));
            Debug.Assert(!IsCandidate1(0, int.MaxValue, 123789));
        }

        public override string Part2(string input)
        {
            return Solve(input, IsCandidate2).ToString();
        }

        public override void Part2Test()
        {
            Debug.Assert(IsCandidate2(0, int.MaxValue, 112233));
            Debug.Assert(!IsCandidate2(0, int.MaxValue, 123444));
            Debug.Assert(IsCandidate2(0, int.MaxValue, 111122));
        }

        private static int Solve(string input, Func<int, int, int, bool> isCandidate)
        {
            var parts = input.Split('-');
            int min = int.Parse(parts[0]), max = int.Parse(parts[1]);

            return (
                from n in Enumerable.Range(min, max)
                where isCandidate(min, max, n)
                select n
            ).Count();
        }

        private static bool IsCandidate1(int min, int max, int password)
        {
            if (password < min || password > max)
                return false;

            bool neighbors = false;
            int maxDigit = 0, lastDigit = 0;
            for (int i = 6; i-- > 0;)
            {
                var digit = password / pow10[i] % 10;
                if (digit < maxDigit)
                    return false;
                else if (digit == lastDigit)
                    neighbors = true;
                else
                    maxDigit = digit;
                lastDigit = digit;
            }

            return neighbors;
        }

        private static bool IsCandidate2(int min, int max, int password)
        {
            if (password < min || password > max)
                return false;

            var neighbors = new List<int>();
            int maxDigit = 0, lastDigit = 0;
            for (int i = 6; i-- > 0;)
            {
                var digit = password / pow10[i] % 10;
                if (digit < maxDigit)
                    return false;
                else if (digit == lastDigit)
                    neighbors[0]++;
                else
                {
                    neighbors.Insert(0, 1);
                    maxDigit = digit;
                }
                lastDigit = digit;
            }

            return neighbors.Any(x => x == 2);
        }

        static readonly int[] pow10 = Enumerable.Range(0, 6)
            .Select(x => (int) Math.Pow(10, x))
            .ToArray();
    }
}
