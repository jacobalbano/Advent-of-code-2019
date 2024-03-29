﻿using AdventOfCode2019.Common;
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
                new { Input = new long[] { 1, 0, 0, 0, 99 }, Output = new long[] { 2, 0, 0, 0, 99 } },
                new { Input = new long[] { 2, 3, 0, 3, 99 }, Output = new long[] { 2, 3, 0, 6, 99 } },
                new { Input = new long[] { 2, 4, 4, 5, 99, 0 }, Output = new long[] { 2, 4, 4, 5, 99, 9801 } },
                new { Input = new long[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 }, Output = new long[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 } }
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
            var bytecode = new long[] { 1002, 4, 3, 4, 33 };
            var expected = new long[] { 1002, 4, 3, 4, 99 };

            var result = vm.Run(bytecode);
            Assert.ArraysMatch(result, expected);
        }

        [Test]
        private static void InstructionSet3_ValidateImmediateMode1()
        {
            long result = 0;
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
                vm.Run(vm.FromSource(test.Script));
                Assert.AreEqual(result, test.Expected);
            }
        }

        [Test]
        private static void InstructionSet3_ValidateImmediateMode2()
        {
            long result = 0;
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
                vm.Run(vm.FromSource(test.Script));
                Assert.AreEqual(result, test.Expected);
            }
        }

        [Test]
        private static void JustForFun()
        {
            var stack = new Stack<long>();
            var outputBuffer = new List<long>();

            var generator = new Intcode();
            generator.ConnectInput(() => 10);
            generator.ConnectOutput(x => stack.Push(x));

            var squarer = new Intcode();
            squarer.ConnectInput(() => stack.Pop());
            squarer.ConnectOutput(x => outputBuffer.Add(x));

            generator.Run(new long[] {
                1005, 0, 6,     // jump to loop_start
                0,              // (pos 3) var a = 0
                0,              // (pos 4) var b = 0,
                0,              // (pos 5) var c = 0,
                3, 3,           // [loop_start] a = input
                4, 4,           // output b # start with zero
                8, 3, 4, 5,     // c = (a == b)
                1001, 4, 1, 4,  // b = b + 1
                1006, 5, 6,     // jump to loop_start if c
                99
            });

            squarer.Run(new long[] {
                1005, 0, 5,     // jump to loop_start
                0,              // (pos 3) var a = 0
                0,              // (pos 4) var b = 0
                3, 3,           // [loop_start] a = input
                2, 3, 3, 4,     // b = a * a
                4, 4,           // output b
                1001, 3, -1, 3, // a = a - 1
                1005, 3, 5,     // jump back to loop_start if a != 0
                99              //  halt
            });

            Assert.ArraysMatch(new long[] { 100, 81, 64, 49, 36, 25, 16, 9, 4, 1 }, outputBuffer.ToArray());
        }

        [Test]
        private static void TestAssembler()
        {
            var src = @"
                # output all numbers from 0 to (input)
                var a
                var b
                var c
                loop_start:
	                in -> a
	                out b
	                eq a b -> c
	                add b 1 -> b
	                fjmp c @loop_start

                end";

            var results = new List<long>();
            var vm = new Intcode();
            vm.ConnectInput(() => 5);
            vm.ConnectOutput(results.Add);

            var bytecode = vm.FromAssembly(src.ToLines());
            var memory = vm.Run(bytecode);
            Assert.ArraysMatch(results.ToArray(), new long[] { 0, 1, 2, 3, 4, 5 });
        }

        [Test]
        private static void TestFactorial()
        {
            Assert.AreEqual(1, Factorial(0));
            Assert.AreEqual(1, Factorial(1));
            Assert.AreEqual(2, Factorial(2));
            Assert.AreEqual(6, Factorial(3));
            Assert.AreEqual(24, Factorial(4));
            Assert.AreEqual(120, Factorial(5));
            Assert.AreEqual(720, Factorial(6));
        }
        
        private static long Factorial(int input)
        {
            var src = @"
                # factorial of input
                var max
                var it = 1
                var fact = 1
                var done
                in -> max
                fjmp max @done # early out if we get zero

                loop:
	                mul fact it -> fact
	                eq max it -> done
	                add it 1 -> it
	                fjmp done @loop
	
                done:
                    out fact
                end";

            long result = 0;
            var vm = new Intcode();
            vm.ConnectInput(() => input);
            vm.ConnectOutput(x => result = x);
            var bytecode = vm.FromAssembly(src.ToLines());
            vm.Run(bytecode);
            return result;
        }
    }
}
