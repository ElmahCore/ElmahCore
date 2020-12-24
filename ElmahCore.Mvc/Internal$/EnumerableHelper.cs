using System;
using System.Collections.Generic;

namespace ElmahCore.Mvc
{
    static class EnumerableHelper
    {

        public static SerializableDictionary<TKey, TElement> ToSerializableDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
            var d = new SerializableDictionary<TKey, TElement>();
            foreach (TSource element in source) d.Add(keySelector(element), elementSelector(element));
            return d;
        }

    }
}