using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Common
{
    public partial class Intcode
    {
        public class ParallelPipe
        {
            public ParallelPipe(Intcode source, Intcode dest)
            {
                Source = source;
                Dest = dest;
            }

            public int Read() => queue.Dequeue();
            public void Write(int value) => queue.Enqueue(value);

            private Queue<int> queue = new Queue<int>();

            private Intcode Source { get; }
            private Intcode Dest { get; }
        }
    }
}
