using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
#pragma warning disable CS8601
#pragma warning disable CS8618

namespace PnFDesktop.Classes
{
    /// <summary>
    /// An implementation of observable collection that contains a duplicate internal
    /// list that is retained momentarily after the list is cleared.
    /// This is so that observers can undo events, etc on the list after it has been cleared and
    /// raised a CollectionChanged event with a Reset action.
    /// </summary>
    public class ImpObservableCollection<T> : ObservableCollection<T>, ICloneable
    {
        /// <summary>
        /// Inner list.
        /// </summary>
        private readonly List<T> _inner = new List<T>();

        public ImpObservableCollection()
        {
        }

        public ImpObservableCollection(IEnumerable<T> range) :
            base(range)
        {
        }

        public ImpObservableCollection(IList<T> list) :
            base(list)
        {
            _inner.AddRange(list);
        }

        public void AddRange(T[] range)
        {
            foreach (T item in range)
            {
                Add(item);
            }
        }

        public void AddRange(IEnumerable range)
        {
            foreach (T item in range)
            {
                Add(item);
            }
        }

        public void AddRange(ICollection<T> range)
        {
            foreach (T item in range)
            {
                Add(item);
            }
        }

        public void RemoveRange(T[] range)
        {
            foreach (T item in range)
            {
                Remove(item);
            }
        }

        public void RemoveRange(IEnumerable range)
        {
            foreach (T item in range)
            {
                Remove(item);
            }
        }

        public void RemoveRange(ImpObservableCollection<T> range)
        {
            foreach (T item in range)
            {
                Remove(item);
            }
        }

        public void RemoveRangeAt(int index, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                RemoveAt(index);
            }
        }

        public void RemoveRange(ICollection<T> range)
        {
            foreach (T item in range)
            {
                Remove(item);
            }
        }

        public void RemoveRange(ICollection range)
        {
            foreach (T item in range)
            {
                Remove(item);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (_inner.Count > 0)
                {
                    OnItemsRemoved(_inner);
                }

                _inner.Clear();
            }

            if (e.OldItems != null)
            {
                foreach (T item in e.OldItems)
                {
                    _inner.Remove(item);
                }

                OnItemsRemoved(e.OldItems);
            }

            if (e.NewItems != null)
            {
                foreach (T item in e.NewItems)
                {
                    _inner.Add(item);
                }

                OnItemsAdded(e.NewItems);
            }
        }

        protected virtual void OnItemsAdded(ICollection items)
        {
            ItemsAdded?.Invoke(this, new CollectionItemsChangedEventArgs(items));
        }

        protected virtual void OnItemsRemoved(ICollection items)
        {
            ItemsRemoved?.Invoke(this, new CollectionItemsChangedEventArgs(items));
        }

        /// <summary>
        /// Event raised when items have been added.
        /// </summary>
        public event EventHandler<CollectionItemsChangedEventArgs> ItemsAdded;

        /// <summary>
        /// Event raised when items have been removed.
        /// </summary>
        public event EventHandler<CollectionItemsChangedEventArgs> ItemsRemoved;

        public T[] ToArray()
        {
            return _inner.ToArray();
        }

        public T2[] ToArray<T2>()
            where T2 : class
        {
            T2[] array = new T2[this.Count];
            int i = 0;
            foreach (T obj in this)
            {
                array[i] = obj as T2;
                ++i;
            }

            return array;
        }

        public object Clone()
        {
            ImpObservableCollection<T> clone = new ImpObservableCollection<T>();
            foreach (var unknown in this)
            {
                var obj = (ICloneable)unknown;
                clone.Add(((T)obj?.Clone())!);
            }

            return clone;
        }

        public List<T> ToList()
        {
            return _inner;
        }
    }
}
