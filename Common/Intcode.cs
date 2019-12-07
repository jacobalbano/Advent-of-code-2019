using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode2019.Common
{
    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Methods called through reflection")]
    class Intcode
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

                Execute(op.Method, ctrl, context);
            }

            return memory;
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

        private static class Assembler
        {
            public static int[] Assemble(string[] lines)
            {
                return YieldIntcode(lines)
                    .ToArray();
            }

            private static IEnumerable<int> YieldIntcode(string[] lines)
            {
                var symbols = new List<Symbol>();
                var labels = new Dictionary<string, int>();
                var variables = new Dictionary<string, int>();
                int offset = 0;

                foreach (var sym in Symbolize(lines))
                {
                    if (sym is LabelDeclaration l)
                        labels[l.Name] = offset; // don't increase offset for labels; they don't actually exist
                    else if (sym is VariableDeclaration d)
                        variables[d.Name] = offset++;
                    else offset++;

                    symbols.Add(sym);
                }

                int jumpOffset = 0;
                if (variables.Count > 0)
                {
                    jumpOffset = 3;
                    symbols.InsertRange(0, new Symbol[] {
                        new Instruction { Opcode = 1005 },              // tjmp
                        new Literal { Value = 0 },                      // looking at program[0] (which will be 1005 and therefore 'true')
                        new Literal { Value = 3 + variables.Count }     // skip over these three instructions plus as many variables as will live before the main program
                    });
                }

                //  second pass now that we have positional data
                foreach (var sym in symbols)
                {
                    switch (sym)
                    {
                        case Literal l:
                            yield return l.Value;
                            break;
                        case LabelDeclaration l:
                            break; // already handled
                        case LabelReference l:
                            if (!labels.TryGetValue(l.Name, out var lPos))
                                throw new Exception($"Invalid label {l.Name}");
                            yield return lPos + jumpOffset;
                            break;
                        case VariableDeclaration v:
                            yield return v.InitialValue;
                            break;
                        case VariableReference v:
                            if (!variables.TryGetValue(v.Name, out var vPos))
                                throw new Exception($"Undeclared variable {v.Name}");
                            yield return vPos + jumpOffset; // if there are variables, they live after the header jump
                            break;
                        case Instruction i:
                            yield return i.Opcode;
                            break;
                        default: throw new Exception($"Unhandled symbol type {sym}");
                    }
                }
            }

            private static IEnumerable<Symbol> Symbolize(string[] lines)
            {
                foreach (var line in lines.Select(SanitizeLine).Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (labelDeclaration.IsMatch(line))
                        yield return new LabelDeclaration(line);
                    else if (variableDeclaration.IsMatch(line))
                        yield return new VariableDeclaration(line);
                    else foreach (var sym in YieldInstructionSymbols(line))
                        yield return sym;
                }
            }

            private static IEnumerable<Symbol> YieldInstructionSymbols(string line)
            {
                var e = whitespace.Split(line).Cast<string>().GetEnumerator();
                if (!e.MoveNext())
                    throw new Exception("Empty line somehow passed to YieldInstructionSymbols");

                if (!instructionsByName.TryGetValue(e.Current, out var instr))
                    throw new Exception($"Invalid instruction {e.Current}");

                var currentInstruction = new Instruction { Opcode = instr.Opcode };
                yield return currentInstruction;

                var paramDigit = 1;
                var args = instr.Method.GetParameters();
                foreach (var arg in args)
                {
                    if (arg.ParameterType == typeof(int))
                    {
                        ++paramDigit;
                        if (!e.MoveNext())
                            throw new Exception($"Incomplete statement: {line}");

                        if (int.TryParse(e.Current, out var iValue))
                        {
                            //  parameter mode
                            currentInstruction.Opcode += (int) Math.Pow(10, paramDigit);
                            yield return new Literal { Value = iValue };
                        }
                        else if (labelReference.IsMatch(e.Current))
                        {
                            //  parameter mode
                            currentInstruction.Opcode += (int)Math.Pow(10, paramDigit);
                            yield return new LabelReference(e.Current);
                        }
                        else if (variableReference.IsMatch(e.Current))
                            yield return new VariableReference { Name = e.Current };
                        else
                            throw new Exception($"Invalid statement: {line}");
                    }
                    else if (arg.ParameterType == typeof(int).MakeByRefType())
                    {
                        if (!e.MoveNext()) throw new Exception($"Incomplete statement: {line}");
                        if (e.Current != "->") throw new Exception($"Invalid statement: {line}");
                        if (!e.MoveNext()) throw new Exception($"Incomplete statement: {line}");

                        if (variableReference.IsMatch(e.Current))
                            yield return new VariableReference { Name = e.Current };
                        else
                            throw new Exception($"Invalid statement: {line}");
                    }
                }
            }

            private static string SanitizeLine(string line)
            {
                var comment = line.IndexOf('#');
                if (comment >= 0)
                    line = line.Substring(0, comment);

                return line.Trim();
            }

            private abstract class Symbol { }

            private class Literal : Symbol
            {
                public int Value { get; set; }
            }

            private class LabelDeclaration : Symbol
            {
                public string Name { get; set; }

                public LabelDeclaration(string line)
                {
                    var match = labelDeclaration.Match(line);
                    Name = match.Groups[1].Value;
                }
            }

            private class LabelReference : Symbol
            {
                public string Name { get; set; }

                public LabelReference(string line)
                {
                    var match = labelReference.Match(line);
                    Name = match.Groups[1].Value;
                }
            }

            private class VariableDeclaration : Symbol
            {
                public string Name { get; set; }
                public int InitialValue { get; set; }

                public VariableDeclaration(string line)
                {
                    var match = variableDeclaration.Match(line);
                    Name = match.Groups[1].Value;
                    if (int.TryParse(match.Groups[2].Value, out var iValue))
                        InitialValue = iValue;
                }
            }

            private class VariableReference : Symbol
            {
                public string Name { get; set; }
            }

            private class Instruction : Symbol
            {
                public int Opcode { get; set; }
            }

            private static readonly Regex labelDeclaration = new Regex(@"(\w+):", RegexOptions.Compiled);
            private static readonly Regex labelReference = new Regex(@"@(\w+)", RegexOptions.Compiled);
            private static readonly Regex variableDeclaration = new Regex(@"var\s*(\w+)(?:(?:\s*=\s*)(\w+|-?\d+))?", RegexOptions.Compiled);
            private static readonly Regex variableReference = new Regex(@"(\w+)", RegexOptions.Compiled);
            private static readonly Regex whitespace = new Regex(@"\s+", RegexOptions.Compiled);
        }
    }
}
