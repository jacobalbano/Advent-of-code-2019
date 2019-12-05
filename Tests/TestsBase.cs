using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class Test : Attribute { }

    class TestsBase<T> where T : TestsBase<T>
    {
        public static void RunAll()
        {
            var tests = typeof(T)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(x => x.IsDefined(typeof(Test)));

            foreach (var method in tests)
                method.Invoke(null, null);
        }
    }
}
