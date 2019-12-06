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
            var vm = new Intcode();
            var asm = vm.FromSource(input);

            asm[1] = 12;
            asm[2] = 2;

            return string.Join("\r\n", vm.Run(asm));
        }

        public override void Part1Test()
        {
            //  covered by IntcodeTests
        }

        public override string Part2(string input)
        {
            var cartProd =
                from noun in Enumerable.Range(0, 99)
                from verb in Enumerable.Range(0, 99)
                select new { Noun = noun, Verb = verb };
            
            var vm = new Intcode();
            var asm = vm.FromSource(input);

            foreach (var p in cartProd)
            {
                asm[1] = p.Noun;
                asm[2] = p.Verb;
                if (vm.Run(asm).First() == 19690720)
                    return (100 * p.Noun + p.Verb).ToString();
            }

            throw new Exception("No solution found!");
        }

        public override void Part2Test()
        {
            //  covered by IntcodeTests
        }
    }
}
