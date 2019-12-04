using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day3 : DayBase
    {
        public override string Part1(string input)
        {
            var scripts = ParseScript(input);
            return DistanceToClosestOverlap(scripts[0], scripts[1])
                .ToString();
        }

        public override void Part1Test()
        {
            var testValues = new[]
            {
                new { Input = "R8,U5,L5,D3\nU7,R6,D4,L4", Output = 6},
                new { Input = "R75,D30,R83,U83,L12,D49,R71,U7,L72\nU62,R66,U55,R34,D71,R55,D58,R83", Output = 159 },
                new { Input = "R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51\nU98,R91,D20,R16,D67,R40,U7,R15,U6,R7", Output = 135 }
            };

            foreach (var test in testValues)
            {
                var scripts = ParseScript(test.Input);
                var best = DistanceToClosestOverlap(scripts[0], scripts[1]);
                Debug.Assert(best == test.Output);
            }
        }

        public override string Part2(string input)
        {
            throw new NotImplementedException();
        }

        public override void Part2Test()
        {
            throw new NotImplementedException();
        }

        private static Move[][] ParseScript(string input)
        {
            return input
                .Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(',').Select(Move.Parse).ToArray())
                .ToArray();
        }

        private static int DistanceToClosestOverlap(Move[] redWire, Move[] blueWire)
        {
            var cursor = new Position { X = 0, Y = 0 };
            var visitedPositions = new HashSet<Position>(
                WalkWire(redWire).Select(x => cursor = cursor.Add(x))
            );

            cursor = new Position { X = 0, Y = 0 };
            var intersections = WalkWire(blueWire)
                .Select(x => cursor = cursor.Add(x))
                .Where(x => !visitedPositions.Add(x))
                .ToList();

            return ManhattanFromZero(intersections
                .OrderBy(ManhattanFromZero)
                .Where(x => !(x.X == 0 && x.Y == 0))
                .First());
        }

        private static IEnumerable<Move> WalkWire(Move[] wire)
        {
            return wire.SelectMany(x => Enumerable.Range(0, Math.Abs(x.X + x.Y)).Select(y => x));
        }

        private static int ManhattanFromZero(Position position)
        {
            return Math.Abs(position.X) + Math.Abs(position.Y);
        }

        private struct Position : IEquatable<Position>
        {
            public int X, Y;

            public Position Add(Move move) => new Position { X = X + Math.Sign(move.X), Y = Y + Math.Sign(move.Y) };

            #region Impl
            public override int GetHashCode()
            {
                var hashCode = 1861411795;
                hashCode = hashCode * -1521134295 + X.GetHashCode();
                hashCode = hashCode * -1521134295 + Y.GetHashCode();
                return hashCode;
            }

            public override bool Equals(object obj) => obj is Position position && Equals(position);
            public bool Equals(Position other) => X == other.X && Y == other.Y;
            public static bool operator ==(Position left, Position right) => left.Equals(right);
            public static bool operator !=(Position left, Position right) => !(left == right);
            public override string ToString() => $"{X}, {Y}";
            #endregion
        }

        private class Move
        {
            public int X { get; private set; }
            public int Y { get; private set; }

            public static Move Parse(string instr)
            {
                var dist = int.Parse(instr.Substring(1));
                switch (instr[0])
                {
                    case 'U': return new Move { Y = -dist };
                    case 'D': return new Move { Y = dist };
                    case 'L': return new Move { X = -dist };
                    case 'R': return new Move { X = dist };
                    default: throw new Exception($"Invalid direction {instr[0]}");
                }
            }
        }

    }
}
