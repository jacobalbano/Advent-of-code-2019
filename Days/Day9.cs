using AdventOfCode2019.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day9 : DayBase
    {
        public override string Part1(string input)
        {
            var result = new List<long>();
            var vm = new Intcode();
            vm.ConnectInput(() => 1);
            vm.ConnectOutput(result.Add);
            var bytecode = vm.FromSource(input);
            vm.Run(bytecode);

            return string.Join("\r\n", result);
        }

        public override void Part1Test()
        {
            {
                var vm = new Intcode();
                var input = new long[] { 109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99 };
                var output = vm.Run(input);
                Assert.ArraysMatch(input, output);
            }

            {
                long result = 0;
                var vm = new Intcode();
                vm.ConnectOutput(x => result = x);
                vm.Run(new long[] { 1102, 34915192, 34915192, 7, 4, 7, 99, 0 });
                Assert.AreEqual(16, result.CountDigits());
            }

            {

                long result = 0;
                var vm = new Intcode();
                vm.ConnectOutput(x => result = x);
                vm.Run(new[] { 104, 1125899906842624, 99 });
                Assert.AreEqual(1125899906842624, result);
            }

        }

        public override string Part2(string input)
        {
            var result = new List<long>();
            var vm = new Intcode();
            vm.ConnectInput(() => 2);
            vm.ConnectOutput(result.Add);
            var bytecode = vm.FromSource(input);
            vm.Run(bytecode);

            return string.Join("\r\n", result);
        }

        public override void Part2Test()
        {
        }
    }
}
