using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neo.Gui.Cross.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }   
    }
}