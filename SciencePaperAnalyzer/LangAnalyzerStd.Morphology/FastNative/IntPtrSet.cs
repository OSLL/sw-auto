using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;

namespace LangAnalyzerStd.Morphology
{
    internal sealed class IntPtrSet : IEnumerable<IntPtr>
    {
        internal struct Slot
        {
            internal int hashCode;
            internal int next;
            internal IntPtr value;
        }

        internal struct Enumerator : IEnumerator<IntPtr>
        {
            private readonly IntPtrSet _set;
            private int _index;

            public IntPtr Current
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get;
                private set;
            }

            object IEnumerator.Current
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    if (_index == 0 || _index == _set._count + 1)
                    {
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return Current;
                }
            }

            internal Enumerator(IntPtrSet set)
            {
                _set = set;
                _index = 0;
                Current = default;
            }

            public void Dispose()
            {
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public bool MoveNext()
            {
                while (_index < _set._count)
                {
                    Current = _set.Slots[_index].value;
                    _index++;
                    if (Current != IntPtr.Zero)
                    {
                        return (true);
                    }
                }
                _index = _set._count + 1;
                Current = default;
                return false;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            void IEnumerator.Reset()
            {
                _index = 0;
                Current = default;
            }
        }

        private int[] _buckets;
        private int _count;
        private int _freeList;

        internal Slot[] Slots
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get;
            private set;
        }
        public int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _count; }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IntPtrSet(int capacity)
        {
            _buckets = new int[capacity];
            Slots = new Slot[capacity];
            _freeList = -1;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public bool Add(IntPtr value)
        {
            int hash = value.GetHashCode() & 0x7FFFFFFF;
            for (int i = _buckets[hash % _buckets.Length] - 1; 0 <= i;)
            {
                var slot = Slots[i];
                if (slot.value == value)
                {
                    return false;
                }
                i = slot.next;
            }

            int n1;
            if (0 <= _freeList)
            {
                n1 = _freeList;
                _freeList = Slots[n1].next;
            }
            else
            {
                if (_count == Slots.Length)
                {
                    Resize();
                }
                n1 = _count;
                _count++;
            }
            int n2 = hash % _buckets.Length;
            Slots[n1] = new Slot()
            {
                hashCode = hash,
                value = value,
                next = _buckets[n2] - 1,
            };
            _buckets[n2] = n1 + 1;

            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Clear()
        {
            if (0 < _count)
            {
                Array.Clear(Slots, 0, _count);
                Array.Clear(_buckets, 0, _buckets.Length);
                _count = 0;
                _freeList = -1;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void Resize()
        {
            int n1 = checked(_count * 2 + 1);
            int[] buckets = new int[n1];
            Slot[] slots = new Slot[n1];
            Array.Copy(Slots, 0, slots, 0, _count);
            for (int i = 0; i < _count; i++)
            {
                int n2 = slots[i].hashCode % n1;
                slots[i].next = buckets[n2] - 1;
                buckets[n2] = i + 1;
            }
            _buckets = buckets;
            Slots = slots;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Enumerator GetEnumerator()
        {
            return (new Enumerator(this));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
        {
            return (new Enumerator(this));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (new Enumerator(this));
        }
    }
}
