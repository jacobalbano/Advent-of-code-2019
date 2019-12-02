using AdventOfCode2019.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day2 : DayBase
    {
        public override string Part1(string input)
        {
            var intsIn = input.Split(',')
                .Select(int.Parse)
                .ToArray();

            intsIn[1] = 12;
            intsIn[2] = 2;

            var vm = new Intcode();
            return string.Join("\r\n", vm.Run(intsIn));
        }

        public override void Part1Test()
        {
            var testValues = new[]
            {
                new { Input = new[] { 1, 0, 0, 0, 99 }, Output = new[] { 2, 0, 0, 0, 99 } },
                new { Input = new[] { 2, 3, 0, 3, 99 }, Output = new[] { 2, 3, 0, 6, 99 } },
                new { Input = new[] { 2, 4, 4, 5, 99, 0 }, Output = new[] { 2, 4, 4, 5, 99, 9801 } },
                new { Input = new[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 }, Output = new[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 } }
            };

            foreach (var test in testValues)
            {
                var vm = new Intcode();
                var result = vm.Run(test.Input);
                AssertArraysMatch(result, test.Output);
            }
        }

        private void AssertArraysMatch(int[] input, int[] output)
        {
            if (input.Length != output.Length)
                Debug.Assert(false, "input and output had different lengths");

            for (int i = 0; i < input.Length; i++)
                Debug.Assert(input[i] == output[i], $"input and output differ at position {i} ({input[i]}) vs {output[i]})");
        }

        public override string Part2(string input)
        {
            var cartProd =
                from noun in Enumerable.Range(0, 99)
                from verb in Enumerable.Range(0, 99)
                select new { Noun = noun, Verb = verb };

            var vm = new Intcode();
            var intsIn = input.Split(',')
                .Select(int.Parse)
                .ToArray();

            foreach (var p in cartProd)
            {
                intsIn[1] = p.Noun;
                intsIn[2] = p.Verb;
                if (vm.Run(intsIn).First() == 19690720)
                    return (100 * p.Noun + p.Verb).ToString();
            }

            throw new Exception("No solution found!");
        }

        public override void Part2Test()
        {
        }
    }
}
