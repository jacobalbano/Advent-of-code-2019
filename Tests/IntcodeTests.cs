using AdventOfCode2019.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Tests
{
    class IntcodeTests : TestsBase<IntcodeTests>
    {
        [Test]
        private static void InstructionSet1_ValidateMemory()
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
                Assert.ArraysMatch(result, test.Output);
            }
        }

        [Test]
        private static void InstructionSet2_ValidateImmediateMode()
        {
            var vm = new Intcode();
            var bytecode = new[] { 1002, 4, 3, 4, 33 };
            var expected = new[] { 1002, 4, 3, 4, 99 };

            var result = vm.Run(bytecode);
            Assert.ArraysMatch(result, expected);
        }

        [Test]
        private static void InstructionSet3_ValidateImmediateMode1()
        {
            int result = 0;
            var vm = new Intcode();
            vm.ConnectOutput(x => result = x);
            vm.ConnectInput(() => 8);

            var testValues = new[]
            {
                new { Script = "3,9,8,9,10,9,4,9,99,-1,8", Expected = 1 }, //  input == 8
                new { Script = "3,9,7,9,10,9,4,9,99,-1,8", Expected = 0 }, //  input < 8
                new { Script = "3,3,1108,-1,8,3,4,3,99", Expected = 1 }, //  input == 8
                new { Script = "3,3,1107,-1,8,3,4,3,99", Expected = 0 }, //  input < 8
            };

            foreach (var test in testValues)
            {
                vm.Run(vm.Compile(test.Script));
                Assert.AreEqual(result, test.Expected);
            }
        }

        [Test]
        private static void InstructionSet3_ValidateImmediateMode2()
        {
            int result = 0;
            var vm = new Intcode();
            vm.ConnectOutput(x => result = x);
            vm.ConnectInput(() => 3);

            var testValues = new[]
            {
                new { Script = "3,9,8,9,10,9,4,9,99,-1,8", Expected = 0 }, //  input == 8
                new { Script = "3,9,7,9,10,9,4,9,99,-1,8", Expected = 1 }, //  input < 8
                new { Script = "3,3,1108,-1,8,3,4,3,99", Expected = 0 }, //  input == 8
                new { Script = "3,3,1107,-1,8,3,4,3,99", Expected = 1 }, //  input < 8
            };

            foreach (var test in testValues)
            {
                vm.Run(vm.Compile(test.Script));
                Assert.AreEqual(result, test.Expected);
            }
        }
    }
}
