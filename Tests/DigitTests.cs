using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Tests
{
    class DigitTests : TestsBase<DigitTests>
    {
        [Test]
        private static void TestCountDigits()
        {
            Assert.AreEqual(1, 1.CountDigits());
            Assert.AreEqual(2, 12.CountDigits());
            Assert.AreEqual(5, 12345.CountDigits());
        }

        [Test]
        private static void TestGetDigit()
        {
            Assert.AreEqual(1, 100.GetDigit(2));
            Assert.AreEqual(9, 9.GetDigit(0));
            Assert.AreEqual(1, 919.GetDigit(1));
        }

        [Test]
        private static void TestReadDigits()
        {
            Assert.AreEqual(100, 1100.ReadDigits(3));
            Assert.AreEqual(1, 101.ReadDigits(2)); // 01 == 1
        }

        [Test]
        private static void TestSplitDigits()
        {
            Assert.ArraysMatch(
                new[] { 1, 2, 3, 4, 5 },
                54321.SplitDigits().ToArray()
            );
        }
    }
}
