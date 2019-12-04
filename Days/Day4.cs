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
            var parts = input.Split('-');
            int min = int.Parse(parts[0]), max = int.Parse(parts[1]);

            return (
                from n in Enumerable.Range(min, max)
                where IsCandidate(min, max, n)
                select n
            ).Count().ToString();
        }

        public override void Part1Test()
        {
            Debug.Assert(IsCandidate(0, int.MaxValue, 111111));
            Debug.Assert(!IsCandidate(0, int.MaxValue, 223450));
            Debug.Assert(!IsCandidate(0, int.MaxValue, 123789));
        }

        public override string Part2(string input)
        {
            throw new NotImplementedException();
        }

        public override void Part2Test()
        {
            throw new NotImplementedException();
        }

        private bool IsCandidate(int min, int max, int password)
        {
            if (password < min || password > max)
                return false;

            bool neighbors = false;
            int maxDigit = 0, lastDigit = 0;
            for (int i = 6; i--> 0;)
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

        static readonly int[] pow10 = Enumerable.Range(0, 6)
            .Select(x => (int) Math.Pow(10, x))
            .ToArray();
    }
}
