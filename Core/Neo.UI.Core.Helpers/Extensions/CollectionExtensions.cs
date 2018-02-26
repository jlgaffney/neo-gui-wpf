using System.Collections.Generic;

namespace Neo.UI.Core.Helpers.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            Guard.ArgumentIsNotNull(source, () => source);
            Guard.ArgumentIsNotNull(collection, () => collection);

            foreach (var item in collection)
            {
                source.Add(item);
            }
        }
    }
}
