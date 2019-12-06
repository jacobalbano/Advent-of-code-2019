using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> factory)
    {
        if (!self.TryGetValue(key, out var value))
            self[key] = value = factory(key);

        return value;
    }
}
