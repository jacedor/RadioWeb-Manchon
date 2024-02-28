using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADPM.Common.Extensions
{

    public static class DictionaryExtensions
    {

        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, 
            Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            foreach (var item in dictionary.Where(predicate).ToList())
            {
                dictionary.Remove(item.Key);
            }
        }

    }

}
