using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day8 : DayBase
    {
        public override string Part1(string input)
        {
            var pixels = GetLayers(input)
                .OrderBy(x => x.Count(y => y == 0))
                .First()
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            return (pixels[1] * pixels[2]).ToString();
        }

        public override void Part1Test()
        {
        }

        public override string Part2(string input)
        {
            return string.Join("\r\n", GetLayers(input)
                .Reverse()
                .Aggregate((canvas, layer) => layer.Select((p, i) => p != 2 ? p : canvas[i]).ToArray())
                .Select(x => (char)('▓' - x * 2))
                .Partition(25)
                .Select(x => new string(x)));
        }

        public override void Part2Test()
        {
        }

        private static IEnumerable<int[]> GetLayers(string input)
        {
            return input
                .Select(char.GetNumericValue)
                .Select(x => (int) x)
                .Partition(25 * 6);
        }
    }
}
