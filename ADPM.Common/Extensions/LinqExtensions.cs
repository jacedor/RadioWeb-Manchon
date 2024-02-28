using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ADPM.Common.Extensions
{

    public static class LinqExtensions
    {

        public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> sequence, Func<T, IEnumerable<T>> childFetcher)
        {
            var itemsToYield = new Queue<T>(sequence);
            while (itemsToYield.Count > 0)
            {
                var item = itemsToYield.Dequeue();
                yield return item;

                var children = childFetcher(item);
                if (children != null)
                {
                    foreach (var child in children) itemsToYield.Enqueue(child);
                }
            }
        }

        public static bool IsGrouped(this IEnumerable collection)
        {
            // TODO: Why does the CheckGrouping method for Dictionary not work 
            if (collection is IDictionary)
            {
                return true;
            }
            return CheckGrouping(collection as dynamic);
        }

        #region .Helper methods 

        private static bool CheckGrouping(object collection)
        {
            return false;
        }
        private static bool CheckGrouping<TKey, TValue>(IEnumerable<IGrouping<TKey, TValue>> collection)
        {
            return true;
        }
        private static bool CheckGrouping<TKey, TValue>(IDictionary<TKey, TValue> collection)
        {
            return true;
        }

        #endregion

    }

}
