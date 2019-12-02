using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day1 : DayBase
    {
        public override string Part1(string input)
        {
            return input.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .Select(FuelForMass)
                .Sum()
                .ToString();
        }

        public List<int> Read(string f_name)
        {
            return File.ReadAllLines(f_name)
                .Select(line => new { Success = int.TryParse(line, out var value), Value = value })
                .Where(x => x.Success)
                .Select(x => x.Value)
                .ToList();
        }

        public override string Part2(string input)
        {
            return input.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .Select(FuelForModule)
                .Sum()
                .ToString();
        }

        public override void Part1Test()
        {
            var testValues = new[]
            {
                new { Mass = 12, Fuel = 2 },
                new { Mass = 14, Fuel = 2 },
                new { Mass = 1969, Fuel = 654 },
                new { Mass = 100756, Fuel = 33583 },
            };

            foreach (var pair in testValues)
                Debug.Assert(pair.Fuel == FuelForMass(pair.Mass));
        }

        public override void Part2Test()
        {
            var testValues = new[]
            {
                new { Mass = 14, Fuel = 2 },
                new { Mass = 1969, Fuel = 966 },
                new { Mass = 100756, Fuel = 50346 },
            };

            foreach (var pair in testValues)
                Debug.Assert(pair.Fuel == FuelForModule(pair.Mass));
        }

        private static int FuelForMass(int mass) => mass / 3 - 2;

        private static int FuelForModule(int moduleMass)
        {
            var fuel = FuelForMass(moduleMass);
            if (FuelForMass(fuel) < 0)
                return fuel;

            return fuel + FuelForModule(fuel);
        }
    }
}
