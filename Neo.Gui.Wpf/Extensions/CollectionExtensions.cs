using System.Collections.Specialized;

namespace Neo.Gui.Wpf.Extensions
{
    public static class CollectionExtensions
    {
        public static string[] ToArray(this StringCollection collection)
        {
            var array = new string[collection.Count];
            for (int i = 0; i < collection.Count; i++)
            {
                array[i] = collection[i];
            }
            
            return array;
        }
    }
}
