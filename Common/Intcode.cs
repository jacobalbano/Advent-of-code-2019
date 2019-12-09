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
        
        public long[] FromSource(string input)
        {
            return input.Split(',')
                .Select(long.Parse)
                .ToArray();
        }

        public long[] FromAssembly(string[] lines)
        {
            return Assembler.Assemble(lines);
        }

        public void ConnectInput(Func<long> getInput) => GetInput = getInput;
        public void ConnectOutput(Action<long> postOutput) => PostOutput = postOutput;

        [Instruction("add", 1)]
        private static void Add(long a, long b, out long writeTo) => writeTo = a + b;

        [Instruction("mul", 2)]
        private static void Multiply(long a, long b, out long writeTo) => writeTo = a * b;

        [Instruction("in", 3)]
        private static void Input(IController ctrl, out long writeTo) => writeTo = ctrl.Input();

        [Instruction("out", 4)]
        private static void Output(IController ctrl, long value) => ctrl.Output(value);

        [Instruction("tjmp", 5)]
        private static void JumpIfTrue(IController ctrl, long bit, long address) { if (bit != 0) ctrl.Jump(address); }

        [Instruction("fjmp", 6)]
        private static void JumpIfFalse(IController ctrl, long bit, long address) { if (bit == 0) ctrl.Jump(address); }

        [Instruction("lt", 7)]
        private static void LessThan(long a, long b, out long writeTo) => writeTo = a < b ? 1 : 0;

        [Instruction("eq", 8)]
        private static void Equals(long a, long b, out long writeTo) => writeTo = a == b ? 1 : 0;

        [Instruction("rbs", 9)]
        private static void RelBase(IController ctrl, long a) => ctrl.AdjustRelativeBase(a);

        [Instruction("end", 99)]
        private static void Halt(IController ctrl) => ctrl.Halt();

        public long[] Run(long[] input)
        {
            return RunUntilYield(input)
                .Last().Memory;
        }
        
        public ParallelController RunParallel(long[] input)
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
            public long[] Memory { get; set; }
            public YieldReason State { get; set; }
        }

        public IEnumerable<ExecutionState> RunUntilYield(long[] input)
        {
            var memory = (long[])input.Clone();
            var ctrl = new Controller(memory, GetInput, PostOutput);

            while (!ctrl.Halted)
            {
                var instruction = (int) ctrl.Read();

                var opcode = instruction.ReadDigits(2);
                var modes = instruction.SplitDigits().Skip(2).Select(x => (int) x).ToArray();

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
            var modes = new int[args.Length];
            var outs = new Dictionary<int, long>(); //  methodParam to dest
            var rawMemory = new List<long> { context.Opcode };
            
            for (int i = 0, p = 0; i < pms.Length; i++)
            {
                var param = pms[i];
                var pType = param.ParameterType;
                if (pType == typeof(IController))
                    args[i] = ctrl;
                else if (pType == typeof(long))
                {
                    var nextInt = ctrl.Read();
                    rawMemory.Add(nextInt);
                    switch (context.GetParamMode(p++))
                    {
                        case 0: nextInt = ctrl.ReadFrom(nextInt); break; // position
                        case 1: break; // immediate
                        case 2: nextInt = ctrl.ReadFromRelativeBase(nextInt); break; // relative
                        default: throw new Exception($"Invalid parameter mode {context.GetParamMode(p)}");
                    }

                    args[i] = nextInt;
                }
                else if (pType == typeof(long).MakeByRefType())
                {
                    outs.Add(i, ctrl.Read());
                    modes[i] = (int) context.GetParamMode(p++);
                }
                else throw new Exception($"Unhandled parameter type {pType.Name}");
            }

            method.Invoke(null, args);
            foreach (var p in outs)
            {
                var paramMode = modes[p.Key];
                if (paramMode == 2)
                    ctrl.WriteToRelativeBase(p.Value, (long)args[p.Key]);
                else
                    ctrl.WriteTo(p.Value, (long)args[p.Key]);
            }
        }

        private interface IController
        {
            void Halt();
            long Input();
            void Output(long value);
            void Jump(long address);
            void AdjustRelativeBase(long a);
        }

        private class OperationContext
        {
            public long Opcode { get; }
            public int[] ParamModes { get; }

            public OperationContext(long opcode, int[] modes)
            {
                Opcode = opcode;
                ParamModes = modes;
            }

            public long GetParamMode(int index)
            {
                return index >= ParamModes.Length ? 0 : ParamModes[index];
            }
        }

        private class Controller : IController
        {
            public long InstructionPointer { get; private set; }

            public bool Halted => halted || InstructionPointer >= InitialProgram.Length;

            public long[] InitialProgram { get; }

            private Dictionary<long, long> ExpandedMemory { get; }

            public Controller(long[] memory, Func<long> getInput, Action<long> postOutput)
            {
                InitialProgram = memory;
                GetInput = getInput;
                PostOutput = postOutput;
                ExpandedMemory = new Dictionary<long, long>();
            }
            
            public long Read() => MemGet(InstructionPointer++);
            public long ReadFrom(long address) => MemGet(address);
            public long ReadFromRelativeBase(long address) => MemGet(relBase + address);
            public void WriteTo(long address, long value) => MemSet(address, value);
            public void WriteToRelativeBase(long address, long value) => MemSet(relBase + address, value);

            private long MemGet(long address)
            {
                return address < InitialProgram.Length ?
                    InitialProgram[address] :
                    ExpandedMemory.GetOrCreate(address, key => 0);
            }

            private void MemSet(long address, long value)
            {
                if (address < InitialProgram.Length)
                    InitialProgram[address] = value;
                else
                    ExpandedMemory[address] = value;
            }

            void IController.Halt()
            {
                halted = true;
            }

            long IController.Input()
            {
                return GetInput?.Invoke() ?? throw new Exception("Input is not connected");
            }

            void IController.Output(long value)
            {
                PostOutput?.Invoke(value);
            }

            void IController.Jump(long address)
            {
                InstructionPointer = address;
            }

            void IController.AdjustRelativeBase(long a)
            {
                relBase += a;
            }

            private bool halted;
            private long relBase;
            private Func<long> GetInput { get; }
            private Action<long> PostOutput { get; }
        }

        private sealed class InstructionAttribute : Attribute
        {
            public string AsmName { get; }
            public long Opcode { get; }
            public InstructionAttribute(string asmName, long opcode)
            {
                AsmName = asmName;
                Opcode = opcode;
            }
        }

        private class InstructionInfo
        {
            public string Name { get; }
            public long Opcode { get; }
            public MethodInfo Method { get; }

            public InstructionInfo(string name, long opcode, MethodInfo method)
            {
                Name = name;
                Opcode = opcode;
                Method = method;
            }
        }

        private delegate void OpHandler(Controller ctrl);
        private static readonly Dictionary<long, InstructionInfo> instructionsByOpcode;
        private static readonly Dictionary<string, InstructionInfo> instructionsByName;
        private Func<long> GetInput;
        private Action<long> PostOutput;
    }
}
