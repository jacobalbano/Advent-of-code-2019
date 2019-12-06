using AdventOfCode2019.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day5 : DayBase
    {
        public override string Part1(string input)
        {
            var sb = new StringBuilder();
            var vm = new Intcode();
            var asm = vm.FromSource(input);

            vm.ConnectInput(() => 1);
            vm.ConnectOutput(x => sb.AppendLine(x.ToString()));

            vm.Run(asm);
            return sb.ToString();
        }

        public override void Part1Test()
        {
            //  covered by IntcodeTests
        }

        public override string Part2(string input)
        {
            var sb = new StringBuilder();
            var vm = new Intcode();
            var asm = vm.FromSource(input);

            vm.ConnectInput(() => 5);
            vm.ConnectOutput(x => sb.AppendLine(x.ToString()));

            vm.Run(asm);
            return sb.ToString();
        }

        public override void Part2Test()
        {
            //  covered by IntcodeTests
        }
    }
}
