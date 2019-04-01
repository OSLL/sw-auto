using System;
using System.Collections;
using System.Collections.Generic;

namespace LangAnalyzer.morphology
{
    internal sealed class LinkedList<T> : IEnumerable<T>
        where T : struct
    {
        public LinkedList()
        {
        }
        public LinkedList(T item)
        {
            _Item = item;
        }

        private T? _Item;
        public T Item
        {
            get { return (_Item.Value); }
        }
        public LinkedList<T> Next
        {
            get;
            private set;
        }

        public LinkedList<T> Add(T item)
        {
            if (!_Item.HasValue)
            {
                _Item = item;
                return (this);
            }
            else
            {
                for (var ll = this; ; ll = ll.Next)
                {
                    if (ll.Next == null)
                    {
                        var next = new LinkedList<T>(item);
                        ll.Next = next;
                        return (next);
                    }
                }
            }
        }
        public void AddRange(LinkedList<T> list)
        {
            if (list.IsEmpty)
                return;

            if (!_Item.HasValue)
            {
                _Item = list._Item;
                Next = list.Next;
            }
            else
            {
                for (var ll = this; ; ll = ll.Next)
                {
                    if (ll.Next == null)
                    {
                        ll.Next = list;
                        return;
                    }
                }
            }
        }
        public bool IsEmpty
        {
            get { return (!_Item.HasValue); }
        }
        public int Count
        {
            get { return (!_Item.HasValue ? 0 : 1); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!_Item.HasValue)
            {
                yield break;
            }
            else
            {
                for (var ll = this; ll.Next != null; ll = ll.Next)
                {
                    yield return (ll.Item);
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (GetEnumerator());
        }
    }
}
