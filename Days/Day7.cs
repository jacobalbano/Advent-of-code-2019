using AdventOfCode2019.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day7 : DayBase
    {
        public override string Part1(string input)
        {
            var max = GetPermutations(new[] { 0, 1, 2, 3, 4 }, 5)
                .Select(x => RunAmplifierSequence(x, input))
                .Max();

            return max.ToString();
        }

        public override void Part1Test()
        {
            Assert.AreEqual(43210, RunAmplifierSequence(new[] { 4, 3, 2, 1, 0 }, "3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0"));
            Assert.AreEqual(54321, RunAmplifierSequence(new[] { 0, 1, 2, 3, 4 }, "3,23,3,24,1002,24,10,24,1002,23,-1,23,101,5,23,23,1,24,23,23,4,23,99,0,0"));
            Assert.AreEqual(65210, RunAmplifierSequence(new[] { 1, 0, 4, 3, 2 }, "3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0"));
        }

        public override string Part2(string input)
        {
            var max = GetPermutations(new[] { 5, 6, 7, 8, 9 }, 5)
                .Select(x => RunAmplifierLoop(x, input))
                .Max();

            return max.ToString();
        }

        public override void Part2Test()
        {
            Assert.AreEqual(139629729, RunAmplifierLoop(new[] { 9, 8, 7, 6, 5 }, "3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5"));
            Assert.AreEqual(18216, RunAmplifierLoop(new[] { 9, 7, 8, 5, 6 }, "3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54,-5,54,1105,1,12,1,53,54,53,1008,54,0,55,1001,55,1,55,2,53,55,53,4,53,1001,56,-1,56,1005,56,6,99,0,0,0,0,10"));
        }

        private static IEnumerable<T[]> GetPermutations<T>(T[] list, int length)
        {
            if (length == 1) return list.Select(t => new[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)), (t1, t2) => t1.Concat(new[] { t2 }).ToArray());
        }

        private static int RunAmplifierSequence(int[] sequence, string program)
        {
            int output = 0;
            for (int i = 0; i < sequence.Length; ++i)
            {
                var input = sequence[i];
                var queue = new Queue<int>();
                queue.Enqueue(input);
                queue.Enqueue(output);

                var vm = new Intcode();
                vm.ConnectInput(queue.Dequeue);
                vm.ConnectOutput(x => output = x);
                var bytecode = vm.FromSource(program);
                vm.Run(bytecode);
            }

            return output;
        }

        private static int RunAmplifierLoop(int[] sequence, string program)
        {
            var amps = new List<IEnumerator<Intcode.ExecutionState>>();
            var queues = sequence
                .Select(x => { var q = new Queue<int>(); q.Enqueue(x); return q; })
                .ToList();

            //var wire = new IntcodeAsyncPipe(); wire.Write(0);



            for (int i = 0; i < sequence.Length; ++i)
            {
                var vm = new Intcode();
                vm.ConnectInput(queues[i].Dequeue);
                vm.ConnectOutput(queues[(i + 1) % sequence.Length].Enqueue);
                var bytecode = vm.FromSource(program);

                amps.Add(vm.RunUntilYield(bytecode).GetEnumerator());
            }

            //  bootstrap amp A
            queues[0].Enqueue(0);
            while (true)
            {
                for (int i = 0; i < amps.Count; ++i)
                {
                    var amp = amps[i];
                    var state = Wrap(amp)
                        .TakeWhile(x => x.State != Intcode.YieldReason.AboutToInput || queues[i].Count != 0)
                        .Select(x => x.State)
                        .LastOrDefault();
                    
                    if (state == Intcode.YieldReason.Halted && i == amps.Count - 1)
                        return queues[0].Dequeue();
                }
            }
        }

        private static IEnumerable<T> Wrap<T>(IEnumerator<T> e)
        {
            while (e.MoveNext())
                yield return e.Current;
        }
    }
}
