using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

using LangAnalyzer.Core;

namespace LangAnalyzer.Morphology
{
    /// <summary>
    /// 
    /// </summary>
    unsafe internal sealed class EnumParser<T>
        where T : struct
    {
        private sealed class CharKeyedSet<X>
        {
            internal struct Slot
            {
                internal int hashCode;
                internal int next;
                internal X value;
                internal char key;
            }

            private const int DEFAULT_CAPACITY = 20;

            private int[] _Buckets;
            private Slot[] _Slots;
            private int _Count;
            private int _FreeList;

            internal Slot[] Slots
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return (_Slots); }
            }
            public int Count
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return (_Count); }
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public CharKeyedSet(int? capacity = null)
            {
                _Buckets = new int[capacity.GetValueOrDefault(DEFAULT_CAPACITY)];
                _Slots = new Slot[capacity.GetValueOrDefault(DEFAULT_CAPACITY)];
                _FreeList = -1;
            }

            public bool Add(char key, X value)
            {
                int hash = (key.GetHashCode() & 0x7fffffff);
                for (int i = _Buckets[hash % _Buckets.Length] - 1; 0 <= i;)
                {
                    var slot = _Slots[i];
                    if (slot.key == key)
                    {
                        return false;
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
            public bool Contains(char key)
            {
                int hash = (key.GetHashCode() & 0x7fffffff);
                for (int i = _Buckets[hash % _Buckets.Length] - 1; 0 <= i;)
                {
                    var slot = _Slots[i];
                    if ((slot.key == key))
                    {
                        return true;
                    }
                    i = slot.next;
                }

                return false;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public bool TryGetValue(char key, ref X value)
            {
                int hash = (key.GetHashCode() & 0x7fffffff);
                for (int i = _Buckets[hash % _Buckets.Length] - 1; 0 <= i;)
                {
                    var slot = _Slots[i];
                    if (slot.key == key)
                    {
                        value = slot.value;
                        return true;
                    }
                    i = slot.next;
                }

                return false;
            }

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
        }

        private sealed class WordTreeNode
        {
            private static readonly char* _UPPER_INVARIANT_MAP;

            static WordTreeNode()
            {
                _UPPER_INVARIANT_MAP = XlatUnsafe.Inst._UPPER_INVARIANT_MAP;
            }

            private CharKeyedSet<WordTreeNode> _CurrentLevel;
            private T? _TValue;
            private WordTreeNode _this_next;

            private WordTreeNode()
            {
                _CurrentLevel = new CharKeyedSet<WordTreeNode>(20);
            }

            public void Add(string s, T t)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    var ch = _UPPER_INVARIANT_MAP[s[0]];
                    if (!_CurrentLevel.TryGetValue(ch, ref _this_next))
                    {
                        _this_next = new WordTreeNode();
                        _CurrentLevel.Add(ch, _this_next);
                    }
                    _this_next.Add(s.Substring(1), t);
                }
                else
                {
                    _TValue = t;
                }
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public bool TryGetValueUnwrapRecursion(char* ptr, char* endPtr, ref T value)
            {
                for (var _this = this; ;)
                {
                    if (ptr == endPtr)
                    {
                        value = _this._TValue.Value;
                        return true;
                    }

                    if (!_this._CurrentLevel.TryGetValue(_UPPER_INVARIANT_MAP[*ptr], ref _this_next))
                    {
                        return false;
                    }
                    _this = _this_next;
                    ptr++;
                }
            }

            private IEnumerable<Tuple<string, T?>> GetAllInternal(Tuple<string, T?> t)
            {
                if (_TValue.HasValue)
                {
                    yield return Tuple.Create(t.Item1, _TValue);
                }

                for (var i = 0; i < _CurrentLevel.Count; i++)
                {
                    var slot = _CurrentLevel.Slots[i];
                    var nt = new Tuple<string, T?>(t.Item1 + slot.key, null);

                    foreach (var nnt in slot.value.GetAllInternal(nt))
                    {
                        yield return nnt;
                    }
                }
            }
            public IEnumerable<Tuple<string, T>> GetAll()
            {
                foreach (var t in GetAllInternal(new Tuple<string, T?>(string.Empty, null)))
                {
                    if (t.Item2.HasValue)
                    {
                        yield return Tuple.Create(t.Item1, t.Item2.Value);
                    }
                }
            }

            public int MaxCharsOnLevel()
            {
                return Math.Max(_CurrentLevel.Count, (0 < _CurrentLevel.Count)
                    ? _CurrentLevel.Slots.Where(sl => sl.value != null).Max(sl => sl.value.MaxCharsOnLevel())
                    : 0);
            }
#if DEBUG
            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                foreach ( var t in GetAll() )
                {
                    sb.Append( '\'' ).Append( t.Item1 ).Append( "' => " ).Append( t.Item2 ).Append( "\r\n" );
                }
                return (sb.ToString());
            }
#endif
            public static WordTreeNode Create()
            {
                var root = new WordTreeNode();

                var seq = Enum.GetValues(typeof(T))
                              .Cast<T>()
                              .Select(v => new { EnumValue = v, TextValue = v.ToString() });
                foreach (var a in seq)
                {
                    root.Add(a.TextValue, a.EnumValue);
                }
                return root;
            }
        }


        private WordTreeNode _FirstWordTrieNode;

        public EnumParser()
        {
            _FirstWordTrieNode = WordTreeNode.Create();
        }

        public bool TryParse(char* ptr, int len, ref T t)
        {
            return _FirstWordTrieNode.TryGetValueUnwrapRecursion(ptr, ptr + len, ref t);
        }

        public IEnumerable<Tuple<string, T>> GetAll()
        {
            foreach (var t in _FirstWordTrieNode.GetAll())
            {
                yield return t;
            }
        }
        public override string ToString()
        {
            return _FirstWordTrieNode.ToString();
        }
    }
}