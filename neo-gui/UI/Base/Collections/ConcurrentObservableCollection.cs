using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Neo.UI.Base.Collections
{
    public class ConcurrentObservableCollection<T> : ObservableCollection<T>
    {
        // Used to prevent collection being modified on another thread while enumerating
        private readonly object lockObject = new object();

        public new void Add(T item)
        {
            lock (lockObject)
            {
                base.Add(item);
            }
        }

        public new void Insert(int index, T item)
        {
            lock (lockObject)
            {
                base.Insert(index, item);
            }
        }

        public void Replace(int index, T newItem)
        {
            lock (lockObject)
            {
                base[index] = newItem;
            }
        }

        public new void Remove(T item)
        {
            lock (lockObject)
            {
                base.Remove(item);
            }
        }

        public new void RemoveAt(int index)
        {
            lock (lockObject)
            {
                base.RemoveAt(index);
            }
        }

        public new void Clear()
        {
            lock (lockObject)
            {
                base.Clear();
            }
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            lock (lockObject)
            {
                return Enumerable.FirstOrDefault(this, predicate);
            }
        }

        public List<T> ConvertToList()
        {
            lock (lockObject)
            {
                return this.ToList();
            }
        }
    }
}