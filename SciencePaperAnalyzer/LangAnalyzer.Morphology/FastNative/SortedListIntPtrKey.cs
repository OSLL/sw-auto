using System;
using System.Collections.Generic;
using System.Runtime;

namespace LangAnalyzer.Morphology
{
    internal struct SortedListIntPtrKey<TValue>
        where TValue : class
    {
        private const int MAX_CAPACITY_THRESHOLD = 0x7FFFFFFF - 0x400 * 0x400;

        internal struct Tuple
        {
            public TValue Value;
            public IntPtr Key;
        }

        private static readonly Tuple[] EMPTY_ARRAY = new Tuple[0];
        private ushort _size;

        /// <summary>Gets or sets the number of elements that the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" /> can contain.</summary>
        /// <returns>The number of elements that the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" /> can contain.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <see cref="P:System.Collections.Generic.SortedListIntPtrKey`2.Capacity" /> is set to a value that is less than <see cref="P:System.Collections.Generic.SortedListIntPtrKey`2.Count" />.</exception>
        /// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
        public int Capacity
        {
            get
            {
                return Array.Length;
            }
            private set
            {
                if (value != Array.Length)
                {
                    if (0 < value)
                    {
                        var destinationArray = new Tuple[value];
                        if (0 < _size)
                        {
                            System.Array.Copy(Array, 0, destinationArray, 0, _size);
                        }
                        Array = destinationArray;
                    }
                    else
                    {
                        Array = EMPTY_ARRAY;
                    }
                }
            }
        }

        /// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</summary>
        /// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</returns>
        public int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _size; }
        }

        /// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IList`1" /> containing the keys in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</returns>
        public Tuple[] Array { [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get; private set;
        }

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" /> and a set operation creates a new element using the specified key.</returns>
        /// <param name="key">The key whose value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
        public TValue this[IntPtr key]
        {
            get
            {
                int n = IndexOfKey(key);
                if (n >= 0)
                {
                    return Array[n].Value;
                }
                throw new KeyNotFoundException();
            }
            set
            {
                int n = InternalBinarySearch(key);
                if (n >= 0)
                {
                    Array[n].Value = value;
                    return;
                }
                Insert(~n, key, value);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" /> class that is empty, has the specified initial capacity, and uses the default <see cref="T:System.Collections.Generic.IComparer`1" />.</summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" /> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="capacity" /> is less than zero.</exception>
        public SortedListIntPtrKey(int capacity)
        {
            Array = new Tuple[capacity];
            _size = 0;
        }
        /// <summary>Adds an element with the specified key and value into the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</exception>
        public void Add(IntPtr key, TValue value)
        {
            int n = InternalBinarySearch(key);
            if (n >= 0)
            {
                throw (new ArgumentException(n.ToString(), "n"));
            }
            Insert(~n, key, value);
        }

        /// <summary>Removes all elements from the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</summary>
        public void Clear()
        {
            System.Array.Clear(Array, 0, _size);
            _size = 0;
        }
        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" /> contains a specific key.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" /> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        public bool ContainsKey(IntPtr key)
        {
            return IndexOfKey(key) >= 0;
        }

        private void EnsureCapacity(int min)
        {
            int n = Array.Length * 3;
            if (MAX_CAPACITY_THRESHOLD < n)
            {
                n = MAX_CAPACITY_THRESHOLD;
            }
            if (n < min)
            {
                n = min;
            }
            Capacity = n;
        }

        /// <summary>Searches for the specified key and returns the zero-based index within the entire <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</summary>
        /// <returns>The zero-based index of <paramref name="key" /> within the entire <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />, if found; otherwise, -1.</returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        public int IndexOfKey(IntPtr key)
        {
            int n = InternalBinarySearch(key);
            if (n < 0)
            {
                return -1;
            }
            return n;
        }
        public int IndexOfKeyCore(IntPtr key)
        {
            int n = InternalBinarySearch(key);
            return n;
        }
        public void Insert(int index, IntPtr key, TValue value)
        {
            if (_size == Array.Length)
            {
                EnsureCapacity(_size + 1);
            }
            if (index < _size)
            {
                System.Array.Copy(Array, index, Array, index + 1, _size - index);
            }
            Array[index] = new Tuple() { Key = key, Value = value };
            _size++;
        }
        public TValue GetValue(int index)
        {
            return Array[index].Value;
        }
        public void SetValue(int index, TValue value)
        {
            Array[index].Value = value;
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" /> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        public bool TryGetValue(IntPtr key, out TValue value)
        {
            int n = IndexOfKey(key);
            if (n >= 0)
            {
                value = Array[n].Value;
                return true;
            }
            value = default;
            return false;
        }
        /// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.SortedListIntPtrKey`2.Count" />.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || _size <= index)
            {
                throw (new ArgumentOutOfRangeException("index"));
            }
            _size--;
            if (index < _size)
            {
                System.Array.Copy(Array, index + 1, Array, index, _size - index);
            }
            Array[_size] = default;
        }

        /// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</summary>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />.</returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        public bool Remove(IntPtr key)
        {
            int n = IndexOfKey(key);
            if (n >= 0)
            {
                RemoveAt(n);
            }
            return n >= 0;
        }

        /// <summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.SortedListIntPtrKey`2" />, if that number is less than 90 percent of current capacity.</summary>
        public void TrimExcess()
        {
            int n = (int)(Array.Length * 0.9);
            if (_size < n)
            {
                Capacity = _size;
            }
        }
        public void Trim()
        {
            Capacity = _size;
        }

        public void SetArray(Tuple[] array)
        {
            Array = array;
            _size = (ushort)array.Length;
        }

        private int InternalBinarySearch(IntPtr value)
        {
            int i = 0;
            int n1 = _size - 1;
            while (i <= n1)
            {
                int n2 = i + ((n1 - i) >> 1);
                int n3 = CompareRoutine(Array[n2].Key, value);
                if (n3 == 0)
                {
                    return n2;
                }
                if (n3 < 0)
                {
                    i = n2 + 1;
                }
                else
                {
                    n1 = n2 - 1;
                }
            }
            return ~i;
        }
        unsafe internal static int CompareRoutine(IntPtr x, IntPtr y)
        {
            if (x == y)
                return 0;

            for (char* x_ptr = (char*)x, y_ptr = (char*)y; ; x_ptr++, y_ptr++)
            {
                int d = *x_ptr - *y_ptr;
                if (d != 0)
                    return d;
                if (*x_ptr == '\0')
                    return 0;
            }
        }
    }

}
