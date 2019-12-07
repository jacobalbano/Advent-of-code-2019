using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode2019.Common
{
    partial class Intcode
    {
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
