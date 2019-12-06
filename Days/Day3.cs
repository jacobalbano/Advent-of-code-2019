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
            var scripts = ParseScript(input);
            return DistanceToCheapestOverlap(scripts[0], scripts[1])
                .ToString();
        }

        public override void Part2Test()
        {
            var testValues = new[]
            {
                new { Input = "R8,U5,L5,D3\nU7,R6,D4,L4", Output = 30},
                new { Input = "R75,D30,R83,U83,L12,D49,R71,U7,L72\nU62,R66,U55,R34,D71,R55,D58,R83", Output = 610 },
                new { Input = "R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51\nU98,R91,D20,R16,D67,R40,U7,R15,U6,R7", Output = 410 }
            };

            foreach (var test in testValues)
            {
                var scripts = ParseScript(test.Input);
                var best = DistanceToCheapestOverlap(scripts[0], scripts[1]);
                Debug.Assert(best == test.Output);
            }
        }

        private static int DistanceToClosestOverlap(Move[] redWire, Move[] blueWire)
        {
            return (
                from red in WalkWire(redWire)
                from blue in WalkWire(blueWire)
                let point = red.Intersection(blue)
                where point != null
                let distance = ManhattanFromZero(point.Value)
                orderby distance ascending
                select distance
            ).First();
        }

        private int DistanceToCheapestOverlap(Move[] redWire, Move[] blueWire)
        {
            return (
                from red in WalkWire(redWire)
                from blue in WalkWire(blueWire)
                where red.Intersection(blue) != null
                let h = red.Start.X == red.End.X ? red : blue
                let v = red.Start.Y == red.End.Y ? red : blue
                let steps = red.TotalLength + blue.TotalLength - Math.Abs(h.End.X - v.End.X) - Math.Abs(v.End.Y - h.End.Y)
                orderby steps ascending
                select steps
            ).First();
        }

        private static Move[][] ParseScript(string input)
        {
            return input
                .ToLines()
                .Select(x => x.Split(',').Select(Move.Parse).ToArray())
                .ToArray();
        }

        private static IEnumerable<Line> WalkWire(Move[] wire)
        {
            var start = new Position();
            int length = 0;
            for (int i = 0; i < wire.Length; ++i)
            {
                var move = wire[i];
                var segLength = Math.Abs(move.X) + Math.Abs(move.Y);
                length += segLength;
                var end = new Position { X = start.X + move.X, Y = start.Y + move.Y };
                yield return new Line { Start = start, End = end, TotalLength = length };
                start = end;
            }
        }

        private static int ManhattanFromZero(Position position)
        {
            return Math.Abs(position.X) + Math.Abs(position.Y);
        }

        private struct Line
        {
            public int TotalLength;
            public Position Start, End;

            public Position? Intersection(Line other)
            {
                bool otherVertical = other.Start.X == other.End.X,
                    thisVertical = Start.X == End.X;

                if (otherVertical == thisVertical) // if both lines are vertical, no intersection
                    return null;

                Line h = thisVertical ? other : this,
                    v = otherVertical ? other : this;

                var top = Math.Min(v.End.Y, v.Start.Y);
                var bottom = Math.Max(v.End.Y, v.Start.Y);
                var left = Math.Min(h.Start.X, h.End.X);
                var right = Math.Max(h.Start.X, h.End.X);

                if (h.End.Y > top && h.End.Y < bottom && v.End.X > left && v.End.X < right)
                    return new Position
                    {
                        X = v.End.X,
                        Y = h.End.Y
                    }; // STUB

                return null;
            }

            public override string ToString() => $"({Start}) -> ({End})";
        }

        private struct Position : IEquatable<Position>
        {
            public int X, Y;
            
            #region Impl
            public static readonly Position Zero = new Position { X = 0, Y = 0 };
            
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
            public override string ToString() => $"{Y}, {X}";
            #endregion
        }

        private struct Move
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
