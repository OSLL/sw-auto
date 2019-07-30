using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using LangAnalyzer.Core;

namespace LangAnalyzer.Morphology
{
    /// <summary>
    /// Морфотип
    /// </summary>
    unsafe internal sealed class MorphoTypeNative
    {
        internal struct MorphoFormEndingUpperAndMorphoAttribute
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public MorphoFormEndingUpperAndMorphoAttribute(IntPtr endingUpper, MorphoAttributeEnum[] morphoAttributes)
            {
                EndingUpper = endingUpper;
                MorphoAttributes = morphoAttributes;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public MorphoFormEndingUpperAndMorphoAttribute(IntPtr endingUpper, LinkedList<MorphoAttributeEnum> morphoAttributes)
            {
                EndingUpper = endingUpper;
                MorphoAttributes = new MorphoAttributeEnum[morphoAttributes.Count];
                morphoAttributes.CopyTo(MorphoAttributes, 0);
            }

            public IntPtr EndingUpper;
            public MorphoAttributeEnum[] MorphoAttributes;

            public override string ToString()
            {
                return $"[{StringsHelper.ToString(EndingUpper)}, {{{string.Join(",", MorphoAttributes)}}}]";
            }
        }

        private sealed class Set<TValue> : IEnumerable<KeyValuePair<IntPtr, TValue>>
            where TValue : class
        {
            internal struct Slot
            {
                internal int hashCode;
                internal int next;
                internal IntPtr key;
                internal TValue value;
            }

            internal struct Enumerator : IEnumerator<KeyValuePair<IntPtr, TValue>>
            {
                private readonly Set<TValue> _set;
                private int _index;
                private KeyValuePair<IntPtr, TValue> _current;

                public IntPtr Current_IntPtr
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get { return _current.Key; }
                }
                public TValue Current_Value
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get { return _current.Value; }
                }
                public KeyValuePair<IntPtr, TValue> Current
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get { return _current; }
                }
                object IEnumerator.Current
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        if (_index == 0 || _index == _set._Count + 1)
                        {
                            throw (new InvalidOperationException("InvalidOperation_EnumOpCantHappen"));
                        }
                        return _current;
                    }
                }

                internal Enumerator(Set<TValue> set)
                {
                    _set = set;
                    _index = 0;
                    _current = default;
                }
                public void Dispose()
                {
                }

                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                public bool MoveNext()
                {
                    while (_index < _set._Count)
                    {
                        var slot = _set._Slots[_index];
                        _index++;
                        if (slot.value != null)
                        {
                            _current = new KeyValuePair<IntPtr, TValue>(slot.key, slot.value);
                            return true;
                        }
                    }
                    _index = _set._Count + 1;
                    _current = default;
                    return false;
                }

                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                void IEnumerator.Reset()
                {
                    _index = 0;
                    _current = default;
                }
            }

            private int[] _Buckets;
            private Slot[] _Slots;
            private int _Count;
            private int _FreeList;

            internal Slot[] Slots
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return _Slots; }
            }
            public int Count
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return _Count; }
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public Set(int capacity)
            {
                _Buckets = new int[capacity];
                _Slots = new Slot[capacity];
                _FreeList = -1;
            }

            /// <summary>
            /// try add not-exists-item & return-(true), else get exists-item to 'existsValue' & return-(false)
            /// </summary>
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public bool Add(IntPtr key, TValue value)
            {
                int hash = key.GetHashCode() & 0x7FFFFFFF;
                for (int i = _Buckets[hash % _Buckets.Length] - 1; 0 <= i;)
                {
                    var slot = _Slots[i];
                    if (slot.key == key)
                    {
                        return (false);
                    }
                    i = slot.next;
                }

                int n1;
                if (0 <= _FreeList)
                {
                    n1 = _FreeList;
                    _FreeList = _Slots[n1].next;
                }
                else
                {
                    if (_Count == _Slots.Length)
                    {
                        Resize();
                    }
                    n1 = _Count;
                    _Count++;
                }
                int n2 = hash % _Buckets.Length;
                _Slots[n1] = new Slot()
                {
                    hashCode = hash,
                    key = key,
                    value = value,
                    next = _Buckets[n2] - 1,
                };
                _Buckets[n2] = n1 + 1;

                return true;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public bool TryGetValue(IntPtr key, ref TValue existsValue)
            {
                int hash = key.GetHashCode() & 0x7fffffff;
                for (int i = _Buckets[hash % _Buckets.Length] - 1; 0 <= i;)
                {
                    var slot = _Slots[i];
                    if (slot.key == key)
                    {
                        existsValue = slot.value;
                        return (true);
                    }
                    i = slot.next;
                }

                return false;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public void Clear()
            {
                if (0 < _Count)
                {
                    Array.Clear(_Slots, 0, _Count);
                    Array.Clear(_Buckets, 0, _Buckets.Length);
                    _Count = 0;
                    _FreeList = -1;
                }
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            private void Resize()
            {
                int n1 = checked(_Count * 2 + 1);
                int[] buckets = new int[n1];
                Slot[] slots = new Slot[n1];
                Array.Copy(_Slots, 0, slots, 0, _Count);
                for (int i = 0; i < _Count; i++)
                {
                    int n2 = slots[i].hashCode % n1;
                    slots[i].next = buckets[n2] - 1;
                    buckets[n2] = i + 1;
                }
                _Buckets = buckets;
                _Slots = slots;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public Enumerator GetEnumerator()
            {
                return (new Enumerator(this));
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator<KeyValuePair<IntPtr, TValue>> IEnumerable<KeyValuePair<IntPtr, TValue>>.GetEnumerator()
            {
                return (new Enumerator(this));
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (new Enumerator(this));
            }
        }

        private static readonly char*[] EMPTY_ENDINGS = new char*[0];
        private static readonly MorphoFormEndingUpperAndMorphoAttribute[] EMPTY_MFUEMA = new MorphoFormEndingUpperAndMorphoAttribute[0];

        #region tempBuffers
        private static IntPtrSet tempBufferHS;
        private static Set<LinkedList<MorphoAttributeEnum>> tempBufferDict;
        private static Stack<LinkedList<MorphoAttributeEnum>> tempBufferLinkedLists;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static LinkedList<MorphoAttributeEnum> PopLinkedList()
        {
            if (tempBufferLinkedLists.Count != 0)
            {
                return tempBufferLinkedLists.Pop();
            }
            else
            {
                var pairs = new LinkedList<MorphoAttributeEnum>();
                tempBufferLinkedLists.Push(pairs);
                return pairs;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static void PushLinkedList(LinkedList<MorphoAttributeEnum> pairs)
        {
            pairs.Clear();
            tempBufferLinkedLists.Push(pairs);
        }

        internal static void BeginLoad()
        {
            const int DEFAULT_CAPACITY = 107;

            tempBufferHS = new IntPtrSet(DEFAULT_CAPACITY);
            tempBufferDict = new Set<LinkedList<MorphoAttributeEnum>>(DEFAULT_CAPACITY);
            tempBufferLinkedLists = new Stack<LinkedList<MorphoAttributeEnum>>(DEFAULT_CAPACITY);
            for (var i = 0; i < DEFAULT_CAPACITY; i++)
            {
                tempBufferLinkedLists.Push(new LinkedList<MorphoAttributeEnum>());
            }
        }
        internal static void EndLoad()
        {
            tempBufferHS = null;
            tempBufferDict = null;
            tempBufferLinkedLists = null;
        }
        #endregion

        /// группа морфо-атрибутов
        private readonly MorphoAttributeGroupEnum _morphoAttributeGroup;
        /// часть речи
        private readonly PartOfSpeechEnum _partOfSpeech;

        /// окончания морфо-форм
        private char*[] _morphoFormEndings;
        /// кортежи uppercase-окончаний морфо-форма и морфо-атрибутов
        private MorphoFormEndingUpperAndMorphoAttribute[] _morphoFormEndingUpperAndMorphoAttributes;

        internal MorphoTypeNative(PartOfSpeechBase partOfSpeechBase)
        {
            _morphoAttributeGroup = partOfSpeechBase.MorphoAttributeGroup;
            _partOfSpeech = partOfSpeechBase.PartOfSpeech;
            _morphoFormEndings = EMPTY_ENDINGS;
            _morphoFormEndingUpperAndMorphoAttributes = EMPTY_MFUEMA;
        }

        internal void SetMorphoForms(List<MorphoFormNative> morphoForms)
        {
            if (morphoForms.Count != 0)
            {
                LinkedList<MorphoAttributeEnum> morphoAttributes = null;
                for (int i = 0, len = morphoForms.Count; i < len; i++)
                {
                    var morphoForm = morphoForms[i];

                    tempBufferHS.Add((IntPtr)morphoForm.Ending);

                    var endingUpperPtr = (IntPtr)morphoForm.EndingUpper;
                    if (!tempBufferDict.TryGetValue(endingUpperPtr, ref morphoAttributes))
                    {
                        morphoAttributes = PopLinkedList();
                        tempBufferDict.Add(endingUpperPtr, morphoAttributes);
                    }
                    var morphoAttribute = MorphoAttributePair.GetMorphoAttribute(this, morphoForm);
                    morphoAttributes.AddLast(morphoAttribute);
                }

                _morphoFormEndings = new char*[tempBufferHS.Count];
                fixed (char** morphoFormEndingsBase = _morphoFormEndings)
                {
                    var it = tempBufferHS.GetEnumerator();
                    for (var i = 0; it.MoveNext(); i++)
                    {
                        *(morphoFormEndingsBase + i) = (char*)it.Current;
                    }
                }
                tempBufferHS.Clear();

                _morphoFormEndingUpperAndMorphoAttributes = new MorphoFormEndingUpperAndMorphoAttribute[tempBufferDict.Count];

                var it2 = tempBufferDict.GetEnumerator();
                for (var i = 0; it2.MoveNext(); i++)
                {
                    _morphoFormEndingUpperAndMorphoAttributes[i] =
                        new MorphoFormEndingUpperAndMorphoAttribute(it2.Current_IntPtr, it2.Current_Value);
                    PushLinkedList(it2.Current_Value);
                }

                tempBufferDict.Clear();
            }
            else
            {
                _morphoFormEndings = EMPTY_ENDINGS;
                _morphoFormEndingUpperAndMorphoAttributes = EMPTY_MFUEMA;
            }
        }

        /// группа морфо-атрибутов
        public MorphoAttributeGroupEnum MorphoAttributeGroup
        {
            get { return _morphoAttributeGroup; }
        }
        /// часть речи
        public PartOfSpeechEnum PartOfSpeech
        {
            get { return _partOfSpeech; }
        }

        public bool HasMorphoForms
        {
            get { return MorphoFormEndings.Length != 0; }
        }
        public char* FirstEnding
        {
            get { return MorphoFormEndings[0]; }
        }

        /// кортежи uppercase-окончаний морфо-форма и морфо-атрибутов
        public MorphoFormEndingUpperAndMorphoAttribute[] MorphoFormEndingUpperAndMorphoAttributes
        {
            get { return _morphoFormEndingUpperAndMorphoAttributes; }
        }
        /// окончания морфо-форм
        public char*[] MorphoFormEndings
        {
            get { return _morphoFormEndings; }
        }

        public override string ToString()
        {
            return $"[{PartOfSpeech}, {MorphoAttributeGroup}, {{{string.Join(",", MorphoFormEndingUpperAndMorphoAttributes)}}}]";
        }
    }
}