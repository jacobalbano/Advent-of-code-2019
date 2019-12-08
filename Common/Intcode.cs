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
    partial class Intcode
    {
        static Intcode()
        {
            var methods = typeof(Intcode)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Select(x => new { Method = x, Attribute = x.GetCustomAttribute<InstructionAttribute>() })
                .Where(x => x.Attribute != null)
                .Select(x => new InstructionInfo(x.Attribute.AsmName, x.Attribute.Opcode, x.Method))
                .ToList();

            instructionsByOpcode = methods.ToDictionary(x => x.Opcode);
            instructionsByName = methods.ToDictionary(x => x.Name);
        }
        
        public int[] FromSource(string input)
        {
            return input.Split(',')
                .Select(int.Parse)
                .ToArray();
        }

        public int[] FromAssembly(string[] lines)
        {
            return Assembler.Assemble(lines);
        }

        public void ConnectInput(Func<int> getInput) => GetInput = getInput;
        public void ConnectOutput(Action<int> postOutput) => PostOutput = postOutput;

        [Instruction("add", 1)]
        private static void Add(int a, int b, out int writeTo) => writeTo = a + b;

        [Instruction("mul", 2)]
        private static void Multiply(int a, int b, out int writeTo) => writeTo = a * b;

        [Instruction("in", 3)]
        private static void Input(IController ctrl, out int writeTo) => writeTo = ctrl.Input();

        [Instruction("out", 4)]
        private static void Output(IController ctrl, int value) => ctrl.Output(value);

        [Instruction("tjmp", 5)]
        private static void JumpIfTrue(IController ctrl, int bit, int address) { if (bit != 0) ctrl.Jump(address); }

        [Instruction("fjmp", 6)]
        private static void JumpIfFalse(IController ctrl, int bit, int address) { if (bit == 0) ctrl.Jump(address); }

        [Instruction("lt", 7)]
        private static void LessThan(int a, int b, out int writeTo) => writeTo = a < b ? 1 : 0;

        [Instruction("eq", 8)]
        private static void Equals(int a, int b, out int writeTo) => writeTo = a == b ? 1 : 0;

        [Instruction("end", 99)]
        private static void Halt(IController ctrl) => ctrl.Halt();

        public int[] Run(int[] input)
        {
            return RunUntilYield(input)
                .Last().Memory;
        }
        
        public ParallelController RunParallel(int[] input)
        {
            throw new NotImplementedException();
        }

        public enum YieldReason
        {
            None,
            AboutToInput,
            AboutToOutput,
            AfterInput,
            AfterOutput,
            Halted,
        }

        public struct ExecutionState
        {
            public int[] Memory { get; set; }
            public YieldReason State { get; set; }
        }

        public IEnumerable<ExecutionState> RunUntilYield(int[] input)
        {
            var memory = (int[])input.Clone();
            var ctrl = new Controller(memory, GetInput, PostOutput);

            while (!ctrl.Halted)
            {
                var instruction = ctrl.Read();

                var opcode = instruction.ReadDigits(2);
                var modes = instruction.SplitDigits().Skip(2).ToArray();

                var context = new OperationContext(opcode, modes);

                if (!instructionsByOpcode.TryGetValue(opcode, out var op))
                    throw new Exception($"Invalid operation {opcode}");

                switch (opcode)
                {
                    case 3: // input
                        yield return new ExecutionState { Memory = memory, State = YieldReason.AboutToInput };
                        break;
                    case 4: // input
                        yield return new ExecutionState { Memory = memory, State = YieldReason.AboutToOutput };
                        break;
                }

                Execute(op.Method, ctrl, context);

                switch (opcode)
                {
                    case 3: // input
                        yield return new ExecutionState { Memory = memory, State = YieldReason.AfterInput };
                        break;
                    case 4: // input
                        yield return new ExecutionState { Memory = memory, State = YieldReason.AfterOutput};
                        break;
                }
            }

            yield return new ExecutionState { Memory = memory, State = YieldReason.Halted };
        }

        private void Execute(MethodInfo method, Controller ctrl, OperationContext context)
        {
            var pms = method.GetParameters();
            var args = new object[pms.Length];
            var outs = new Dictionary<int, int>(); //  methodParam to dest
            var rawMemory = new List<int> { context.Opcode };

            for (int i = 0, p = 0; i < pms.Length; i++)
            {
                var param = pms[i];
                var pType = param.ParameterType;
                if (pType == typeof(IController))
                    args[i] = ctrl;
                else if (pType == typeof(int))
                {
                    var nextInt = ctrl.Read();
                    rawMemory.Add(nextInt);
                    switch (context.GetParamMode(p++))
                    {
                        case 0: nextInt = ctrl.ReadFrom(nextInt); break; // position
                        case 1: break; // immediate
                        default: throw new Exception($"Invalid parameter mode {context.GetParamMode(p)}");
                    }

                    args[i] = nextInt;
                }
                else if (pType == typeof(int).MakeByRefType())
                {
                    outs.Add(i, ctrl.Read());
                    p++;
                }
                else throw new Exception($"Unhandled parameter type {pType.Name}");
            }

            method.Invoke(null, args);
            foreach (var p in outs)
                ctrl.WriteTo(p.Value, (int)args[p.Key]);
        }

        private interface IController
        {
            void Halt();
            int Input();
            void Output(int value);
            void Jump(int address);
        }

        private class OperationContext
        {
            public int Opcode { get; }
            public int[] ParamModes { get; }

            public OperationContext(int opcode, int[] modes)
            {
                Opcode = opcode;
                ParamModes = modes;
            }

            public int GetParamMode(int index)
            {
                return index >= ParamModes.Length ? 0 : ParamModes[index];
            }
        }

        private class Controller : IController
        {
            public int InstructionPointer { get; private set; }

            public bool Halted => halted || InstructionPointer >= Memory.Length;

            public int[] Memory { get; }

            public Controller(int[] memory, Func<int> getInput, Action<int> postOutput)
            {
                Memory = memory;
                GetInput = getInput;
                PostOutput = postOutput;
            }
            
            public int Read() => Memory[InstructionPointer++];
            public int ReadFrom(int address) => Memory[address];
            public void WriteTo(int address, int value) => Memory[address] = value;

            void IController.Halt()
            {
                halted = true;
            }

            int IController.Input()
            {
                return GetInput?.Invoke() ?? throw new Exception("Input is not connected");
            }

            void IController.Output(int value)
            {
                PostOutput?.Invoke(value);
            }

            void IController.Jump(int address)
            {
                InstructionPointer = address;
            }

            private bool halted;
            private Func<int> GetInput { get; }
            private Action<int> PostOutput { get; }
        }

        private sealed class InstructionAttribute : Attribute
        {
            public string AsmName { get; }
            public int Opcode { get; }
            public InstructionAttribute(string asmName, int opcode)
            {
                AsmName = asmName;
                Opcode = opcode;
            }
        }

        private class InstructionInfo
        {
            public string Name { get; }
            public int Opcode { get; }
            public MethodInfo Method { get; }

            public InstructionInfo(string name, int opcode, MethodInfo method)
            {
                Name = name;
                Opcode = opcode;
                Method = method;
            }
        }

        private delegate void OpHandler(Controller ctrl);
        private static readonly Dictionary<int, InstructionInfo> instructionsByOpcode;
        private static readonly Dictionary<string, InstructionInfo> instructionsByName;
        private Func<int> GetInput;
        private Action<int> PostOutput;
    }
}
