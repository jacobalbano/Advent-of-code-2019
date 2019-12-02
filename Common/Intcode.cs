using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Common
{
    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Methods called through reflection")]
    class Intcode
    {
        public Intcode()
        {
            instructions = GetType()
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Select(x => new { Method = x, Attribute = x.GetCustomAttribute<InstructionAttribute>() })
                .ToDictionary(x => x.Attribute.Opcode, x => WrapInstruction(x.Method));
        }

        public int[] Run(int[] input)
        {
            var memory = (int[])input.Clone();
            var ctrl = new Controller(memory);

            while (!ctrl.Halted)
            {
                var opcode = ctrl.Read();
                if (!instructions.TryGetValue(opcode, out var op))
                    throw new Exception($"Invalid operation {opcode}");

                op(ctrl);
            }

            return memory;
        }

        [Instruction(1)]
        private static int Add(int a, int b) => a + b;

        [Instruction(2)]
        private static int Multiply(int a, int b) => a * b;

        [Instruction(99)]
        private static void Halt(IController ctrl) =>  ctrl.Halt();

        private interface IController
        {
            void Halt();
        }

        private class Controller : IController
        {
            public int InstructionPointer { get; private set; }

            public bool Halted => halted || InstructionPointer >= Memory.Length;

            public int[] Memory { get; }

            public Controller(int[] memory)
            {
                Memory = memory;
            }

            public void Halt() => halted = true;

            public int Read()
            {
                return Memory[InstructionPointer++];
            }

            public int Read(int address)
            {
                return Memory[address];
            }

            public void Write(int address, int value)
            {
                Memory[address] = value;
            }

            private bool halted;
        }

        private sealed class InstructionAttribute : Attribute
        {
            public int Opcode { get; }
            public InstructionAttribute(int opcode) => Opcode = opcode;
        }

        private OpHandler WrapInstruction(MethodInfo method)
        {
            var pms = method.GetParameters();
            var ret = method.ReturnType;

            if (ret == typeof(int) && pms.All(x => x.ParameterType == typeof(int)))
                return BinaryInstruction;
            else if (pms.Length == 1 && pms[0].ParameterType == typeof(IController))
                return Passthrough;
            else
                throw new Exception($"Unknown signature for instruction method {method.Name}");

            void BinaryInstruction(Controller ctrl)
            {
                int a = ctrl.Read(), b = ctrl.Read(), dest = ctrl.Read();
                var args = new object[] { ctrl.Read(a), ctrl.Read(b) };
                ctrl.Write(dest, (int)method.Invoke(null, args));
            }

            void Passthrough(Controller ctrl)
            {
                method.Invoke(null, new object[] { ctrl });
            }
        }

        private delegate void OpHandler(Controller ctrl);
        private readonly Dictionary<int, OpHandler> instructions;
    }
}
