using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AdventOfCode2019.Days
{
    public abstract class DayBase
    {
        public abstract string Part1(string input);
        public abstract void Part1Test();

        public abstract string Part2(string input);
        public abstract void Part2Test();

        protected Stream GetFile(string localFileName)
        {
            var t = GetType();
            var asm = t.Assembly;
            return asm.GetManifestResourceStream($"{asm.GetName().Name}.Input.{t.Name}.{localFileName}");
        }

        protected string GetString(string txtFileName)
        {
            using (var sr = new StreamReader(GetFile(txtFileName)))
                return sr.ReadToEnd();
        }

        protected IEnumerable<string> GetLines(string txtFileName)
        {
            using (var sr = new StreamReader(GetFile(txtFileName)))
                while (sr.Peek() >= 0) yield return sr.ReadLine();
        }

        public string Name => GetType().Name;
    }
}