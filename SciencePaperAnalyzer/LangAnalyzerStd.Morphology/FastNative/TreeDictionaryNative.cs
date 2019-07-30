using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Morphology
{
    /// <summary>
    /// Словарь-дерево для слов
    /// </summary>
    unsafe internal sealed class TreeDictionaryNative
    {
        private struct Pair
        {
            public Pair(BaseMorphoFormNative baseMorphoForm, MorphoAttributeEnum morphoAttribute)
            {
                BaseMorphoForm = baseMorphoForm;
                MorphoAttribute = morphoAttribute;
            }

            public BaseMorphoFormNative BaseMorphoForm;
            public MorphoAttributeEnum MorphoAttribute;

            public override int GetHashCode()
            {
                return (int)MorphoAttribute ^
                    (int)BaseMorphoForm.Base ^
                    (int)BaseMorphoForm.PartOfSpeech ^
                    (int)BaseMorphoForm.MorphoFormEndings[0];
            }
#if DEBUG
            public override string ToString()
            {
                return (BaseMorphoForm + ", " + MorphoAttribute);
            }
#endif
        }

        private struct PairComparer : IEqualityComparer<Pair>
        {
            public bool Equals(Pair x, Pair y)
            {
                if (x.MorphoAttribute != y.MorphoAttribute)
                    return false;

                if (x.BaseMorphoForm.Base != y.BaseMorphoForm.Base)
                    return false;

                if (x.BaseMorphoForm.PartOfSpeech != y.BaseMorphoForm.PartOfSpeech)
                    return false;

                if (x.BaseMorphoForm.MorphoFormEndings[0] != y.BaseMorphoForm.MorphoFormEndings[0])
                    return false;

                return true;
            }
            public int GetHashCode(Pair obj)
            {
                return (obj.GetHashCode());
            }
        }

        private sealed class PairSet : IEnumerable<Pair>
        {
            internal struct Slot
            {
                internal int hashCode;
                internal int next;
                internal Pair value;
            }

            internal struct Enumerator : IEnumerator<Pair>
            {
                private readonly PairSet _set;
                private Pair _current;
                private int _index;

                public Pair Current
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get { return _current; }
                }
                object IEnumerator.Current
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        if (_index == 0 || _index == _set._count + 1)
                        {
                            throw (new InvalidOperationException("InvalidOperation_EnumOpCantHappen"));
                        }
                        return _current;
                    }
                }

                internal Enumerator(PairSet set)
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
                    while (_index < _set._count)
                    {
                        _current = _set._slots[_index].value;
                        _index++;
                        if (_current.BaseMorphoForm != null)
                        {
                            return true;
                        }
                    }
                    _index = _set._count + 1;
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

            private int[] _buckets;
            private Slot[] _slots;
            private int _count;
            private int _freeList;
            private readonly PairComparer _comparer;

            internal Slot[] Slots
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return _slots; }
            }
            public int Count
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return _count; }
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public PairSet(int capacity)
            {
                _buckets = new int[capacity];
                _slots = new Slot[capacity];
                _freeList = -1;
            }

            /// <summary>
            /// try add not-exists-item & return-(true), else get exists-item to 'existsValue' & return-(false)
            /// </summary>
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public bool TryAddOrGetExists(Pair keyValue, ref Pair existsValue)
            {
                int hash = keyValue.GetHashCode() & 0x7FFFFFFF;
                for (int i = _buckets[hash % _buckets.Length] - 1; 0 <= i;)
                {
                    var slot = _slots[i];
                    if ((slot.hashCode == hash) && _comparer.Equals(slot.value, keyValue))
                    {
                        existsValue = slot.value;
                        return false;
                    }
                    i = slot.next;
                }

                int n1;
                if (0 <= _freeList)
                {
                    n1 = _freeList;
                    _freeList = _slots[n1].next;
                }
                else
                {
                    if (_count == _slots.Length)
                    {
                        Resize();
                    }
                    n1 = _count;
                    _count++;
                }
                int n2 = hash % _buckets.Length;
                _slots[n1] = new Slot()
                {
                    hashCode = hash,
                    value = keyValue,
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
                    Array.Clear(_slots, 0, _count);
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
                Array.Copy(_slots, 0, slots, 0, _count);
                for (int i = 0; i < _count; i++)
                {
                    int n2 = slots[i].hashCode % n1;
                    slots[i].next = buckets[n2] - 1;
                    buckets[n2] = i + 1;
                }
                _buckets = buckets;
                _slots = slots;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public Enumerator GetEnumerator()
            {
                return (new Enumerator(this));
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator<Pair> IEnumerable<Pair>.GetEnumerator()
            {
                return (new Enumerator(this));
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (new Enumerator(this));
            }
        }

        #region tempBuffers
        private static PairSet tempBufferPairs;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static Pair[] TempBufferPairsToArrayAndClear()
        {
            var pairs_new = new Pair[tempBufferPairs.Count];
            var it = tempBufferPairs.GetEnumerator();
            for (var i = 0; it.MoveNext(); i++)
            {
                pairs_new[i] = it.Current;
            }
            tempBufferPairs.Clear();
            return pairs_new;
        }
        private static char* _UPPER_INVARIANT_MAP;

        static TreeDictionaryNative()
        {
            _UPPER_INVARIANT_MAP = XlatUnsafe.Inst._UPPER_INVARIANT_MAP;
        }

        internal static void BeginLoad()
        {
            const int DEFAULT_CAPACITY = 11;

            BaseMorphoFormNative.BeginLoad();
            MorphoTypeNative.BeginLoad();

            tempBufferPairs = new PairSet(DEFAULT_CAPACITY);
        }
        internal static void EndLoad()
        {
            BaseMorphoFormNative.EndLoad();
            MorphoTypeNative.EndLoad();
            tempBufferPairs = null;
        }
        #endregion

        /// слот для дочерних слов
        private SortedListCharKey<TreeDictionaryNative> _slots;
        /// коллекция информаций о формах слова
        private SortedListIntPtrKey<Pair[]> _endings;

        private bool HasEndings()
        {
            return (_endings.Array != null);
        }

        public TreeDictionaryNative()
        {
            _slots.InitArrayAsEmpty();
        }

        internal void Trim()
        {
            if (HasEndings())
            {
                _endings.Trim();
            }
            _slots.Trim();
            for (int i = 0, len = _slots.Count; i < len; i++)
            {
                _slots.Array[i].Value.Trim();
            }
        }

        #region Append word
        /// добавление слова и всех его форм в словарь
        /// wordBase   - marshaled-слово
        /// morphoType - морфотип
        /// nounType   - тип сущетсвительного
        public void AddWord(char* wordBase, MorphoTypeNative morphoType, ref MorphoAttributePair? nounType)
        {
            var baseMorphoForm = new BaseMorphoFormNative(wordBase, morphoType);

            for (TreeDictionaryNative _this = this, _this_next; ;)
            {
                var first_char = _UPPER_INVARIANT_MAP[*wordBase];

                #region [.сохранение характеристик if end-of-word.]
                if (first_char == '\0')
                {
                    var len = morphoType.MorphoFormEndingUpperAndMorphoAttributes.Length;
                    SortedListIntPtrKey<Pair[]>.Tuple[] tuples;
                    int tuplesOffset;
                    if (!_this.HasEndings())
                    {
                        tuplesOffset = 0;
                        tuples = new SortedListIntPtrKey<Pair[]>.Tuple[len];
                    }
                    else
                    {
                        tuplesOffset = _this._endings.Count;
                        tuples = new SortedListIntPtrKey<Pair[]>.Tuple[len + tuplesOffset];

                        for (int i = 0; i < tuplesOffset; i++)
                        {
                            tuples[i] = _this._endings.Array[i];
                        }
                    }

                    for (int i = 0; i < len; i++)
                    {
                        var p = morphoType.MorphoFormEndingUpperAndMorphoAttributes[i];
                        var pairs_current_len = p.MorphoAttributes.Length;
                        var pairs_current = new Pair[pairs_current_len];
                        for (int j = 0; j < pairs_current_len; j++)
                        {
                            var ma = MorphoAttributePair.GetMorphoAttribute(morphoType, p.MorphoAttributes[j], ref nounType);
                            pairs_current[j] = new Pair(baseMorphoForm, ma);
                        }
                        tuples[i + tuplesOffset] = new SortedListIntPtrKey<Pair[]>.Tuple() { Key = p.EndingUpper, Value = pairs_current };
                    }

                    ShellSortAscending(tuples);
                    MergeSorted(ref tuples);
                    _this._endings.SetArray(tuples);
                    return;
                }
                #endregion

                if (!_this._slots.TryGetValue(first_char, out _this_next))
                {
                    /// добавление новой буквы
                    _this_next = new TreeDictionaryNative();
                    _this._slots.Add(first_char, _this_next);
                }
                _this = _this_next;
                wordBase++;
            }
        }

        private static void ShellSortAscending(SortedListIntPtrKey<Pair[]>.Tuple[] array)
        {
            for (int arrayLength = array.Length, gap = (arrayLength >> 1); 0 < gap; gap = (gap >> 1))
            {
                for (int i = 0, len = arrayLength - gap; i < len; i++)
                {
                    int j = i + gap;
                    SortedListIntPtrKey<Pair[]>.Tuple tmp = array[j];
                    while (gap <= j)
                    {
                        var k = j - gap;
                        var t = array[k];
                        if (0 <= SortedListIntPtrKey<Pair[]>.CompareRoutine(tmp.Key, t.Key))
                            break;
                        array[j] = t;
                        j = k;
                    }
                    array[j] = tmp;
                }
            }
        }
        private static void MergeSorted(ref SortedListIntPtrKey<Pair[]>.Tuple[] array)
        {
            var emptyCount = 0;
            var i_prev = 0;
            var t_prev = array[0];
            var arrayLength = array.Length;
            var pair_exists = default(Pair);
            for (int i = 1; i < arrayLength; i++)
            {
                var t_curr = array[i];
                //comparing native-strings, given that duplicates have the same addresses
                if (t_prev.Key == t_curr.Key)
                /*full comparing native-strings (same addresses & char-to-char compare)*/
                /*if ( SortedListIntPtrKey< pair_t[] >.CompareRoutine( t_prev.Key, t_curr.Key ) == 0 )*/
                {
                    array[i].Key = IntPtr.Zero;
                    emptyCount++;

                    var pairs = t_curr.Value;
                    for (int j = 0, ln = pairs.Length; j < ln; j++)
                    {
                        var pair = pairs[j];
                        if (!tempBufferPairs.TryAddOrGetExists(pair, ref pair_exists))
                        {
                            pair_exists.BaseMorphoForm.AppendMorphoFormEndings(pair.BaseMorphoForm);
                        }
                    }

                    pairs = t_prev.Value;
                    for (int j = 0, ln = pairs.Length; j < ln; j++)
                    {
                        var pair = pairs[j];
                        if (!tempBufferPairs.TryAddOrGetExists(pair, ref pair_exists))
                        {
                            pair_exists.BaseMorphoForm.AppendMorphoFormEndings(pair.BaseMorphoForm);
                        }
                    }

                    var pairs_new = TempBufferPairsToArrayAndClear();
                    array[i_prev].Value = pairs_new;
                }
                else
                {
                    t_prev = t_curr;
                    i_prev = i;
                }
            }

            if (emptyCount != 0)
            {
                var array_new = new SortedListIntPtrKey<Pair[]>.Tuple[arrayLength - emptyCount];
                for (int i = 0, j = 0; i < arrayLength; i++)
                {
                    var t_curr = array[i];
                    if (t_curr.Key != IntPtr.Zero)
                    {
                        array_new[j++] = t_curr;
                    }
                }
                array = array_new;
            }
        }

        private static string ToString(SortedListIntPtrKey<Pair[]>.Tuple[] array)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var a in array)
            {
                sb.Append(StringsHelper.ToString(a.Key)).Append(Environment.NewLine);
            }
            return (sb.ToString());
        }
        #endregion

        #region GetWordFormMorphologies & GetWordForms
        /// получение морфологических свойств слова
        /// wordUpper - слово
	    /// result - коллекция информаций о формах слова	    
        public bool GetWordFormMorphologies(string wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            result.Clear();
            fixed (char* wordUpper_ptr = wordUpper)
            {
                switch (wordFormMorphologyMode)
                {
                    case WordFormMorphologyModeEnum.Default:
                        FillWordFormMorphologies(wordUpper_ptr, result);
                        break;

                    case WordFormMorphologyModeEnum.FirstStartsWithUpperAfterLowerLetter:
                        {
                            FillWordFormMorphologies_StartsWithUpperLetter(wordUpper_ptr, result);
                            if (result.Count == 0)
                            {
                                FillWordFormMorphologies_StartsWithLowerLetter(wordUpper_ptr, result);
                            }
                        }
                        break;

                    case WordFormMorphologyModeEnum.FirstStartsWithLowerAfterUpperLetter:
                        {
                            FillWordFormMorphologies_StartsWithLowerLetter(wordUpper_ptr, result);
                            if (result.Count == 0)
                            {
                                FillWordFormMorphologies_StartsWithUpperLetter(wordUpper_ptr, result);
                            }
                        }
                        break;

                    case WordFormMorphologyModeEnum.StartsWithLowerLetter:
                        FillWordFormMorphologies_StartsWithLowerLetter(wordUpper_ptr, result);
                        break;

                    case WordFormMorphologyModeEnum.StartsWithUpperLetter:
                        FillWordFormMorphologies_StartsWithUpperLetter(wordUpper_ptr, result);
                        break;
                }
            }
            return result.Count != 0;
        }
        public bool GetWordFormMorphologies(char* wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            result.Clear();
            {
                switch (wordFormMorphologyMode)
                {
                    case WordFormMorphologyModeEnum.Default:
                        FillWordFormMorphologies(wordUpper, result);
                        break;

                    case WordFormMorphologyModeEnum.FirstStartsWithUpperAfterLowerLetter:
                        FillWordFormMorphologies_FirstStartsWithUpperAfterLowerLetter(wordUpper, result);
                        break;

                    case WordFormMorphologyModeEnum.FirstStartsWithLowerAfterUpperLetter:
                        FillWordFormMorphologies_FirstStartsWithLowerAfterUpperLetter(wordUpper, result);
                        break;

                    case WordFormMorphologyModeEnum.StartsWithLowerLetter:
                        FillWordFormMorphologies_StartsWithLowerLetter(wordUpper, result);
                        break;

                    case WordFormMorphologyModeEnum.StartsWithUpperLetter:
                        FillWordFormMorphologies_StartsWithUpperLetter(wordUpper, result);
                        break;
                }
            }
            return result.Count != 0;
        }

        /// получение всех форм слова
        /// wordUpper - слово
        /// result - коллекция форм слова	    
        public bool GetWordForms(string wordUpper, List<WordForm> result)
        {
            result.Clear();
            fixed (char* word_ptr = wordUpper)
            {
                FillWordForms(word_ptr, result);
            }
            return result.Count != 0;
        }

        /// поиск слова в словаре
        /// word   - слово
        /// result - коллекция форм слова
        private void FillWordFormMorphologies(char* word, List<WordFormMorphology> result)
        {
            FillWordFormMorphologies_Core(word, result);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_slots.TryGetValue(first_char, out TreeDictionaryNative value))
                {
                    value.FillWordFormMorphologies(word + 1, result);
                }
            }
        }
        private void FillWordFormMorphologies_FirstStartsWithUpperAfterLowerLetter(char* word, List<WordFormMorphology> result)
        {
            FillWordFormMorphologies_FirstStartsWithUpperAfterLowerLetter_Core(word, result);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_slots.TryGetValue(first_char, out TreeDictionaryNative value))
                {
                    value.FillWordFormMorphologies_FirstStartsWithUpperAfterLowerLetter(word + 1, result);
                }
            }
        }
        private void FillWordFormMorphologies_FirstStartsWithLowerAfterUpperLetter(char* word, List<WordFormMorphology> result)
        {
            FillWordFormMorphologies_FirstStartsWithLowerAfterUpperLetter_Core(word, result);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_slots.TryGetValue(first_char, out TreeDictionaryNative value))
                {
                    value.FillWordFormMorphologies_FirstStartsWithLowerAfterUpperLetter(word + 1, result);
                }
            }
        }
        private void FillWordFormMorphologies_StartsWithLowerLetter(char* word, List<WordFormMorphology> result)
        {
            FillWordFormMorphologies_StartsWithLowerLetter_Core(word, result);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_slots.TryGetValue(first_char, out TreeDictionaryNative value))
                {
                    value.FillWordFormMorphologies_StartsWithLowerLetter(word + 1, result);
                }
            }
        }
        private void FillWordFormMorphologies_StartsWithUpperLetter(char* word, List<WordFormMorphology> result)
        {
            FillWordFormMorphologies_StartsWithUpperLetter_Core(word, result);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_slots.TryGetValue(first_char, out TreeDictionaryNative value))
                {
                    value.FillWordFormMorphologies_StartsWithUpperLetter(word + 1, result);
                }
            }
        }
        private void FillWordForms(char* word, List<WordForm> result)
        {
            FillWordForms_Core(word, result);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_slots.TryGetValue(first_char, out TreeDictionaryNative value))
                {
                    value.FillWordForms(word + 1, result);
                }
            }
        }

        /// поиск слова в слоте
        /// wordPart - оставшаяся часть слова
        /// result   - коллекция форм слова
        private void FillWordFormMorphologies_Core(char* wordPart, List<WordFormMorphology> result)
        {
            if (!HasEndings())
                return;

            if (_endings.TryGetValue((IntPtr)wordPart, out Pair[] pairs))
            {
                for (int i = 0, len = pairs.Length; i < len; i++)
                {
                    var p = pairs[i];
                    var wfmi = new WordFormMorphology(p.BaseMorphoForm, p.MorphoAttribute);
                    result.Add(wfmi);
                }
            }
        }
        private void FillWordFormMorphologies_FirstStartsWithUpperAfterLowerLetter_Core(char* wordPart, List<WordFormMorphology> result)
        {
            if (!HasEndings())
                return;

            if (_endings.TryGetValue((IntPtr)wordPart, out Pair[] pairs))
            {
                var findWithUpper = false;
                var len = pairs.Length;
                for (var i = 0; i < len; i++)
                {
                    var p = pairs[i];
                    var baseMorphoForm = p.BaseMorphoForm;
                    var first_char = *baseMorphoForm.Base;
                    if ((first_char != '\0') && _UPPER_INVARIANT_MAP[first_char] != first_char)
                    {
                        continue;
                    }

                    findWithUpper = true;
                    var wfmi = new WordFormMorphology(baseMorphoForm, p.MorphoAttribute);
                    result.Add(wfmi);
                }

                if (!findWithUpper)
                {
                    for (var i = 0; i < len; i++)
                    {
                        var p = pairs[i];
                        var wfmi = new WordFormMorphology(p.BaseMorphoForm, p.MorphoAttribute);
                        result.Add(wfmi);
                    }
                }
            }
        }
        private void FillWordFormMorphologies_FirstStartsWithLowerAfterUpperLetter_Core(char* wordPart, List<WordFormMorphology> result)
        {
            if (!HasEndings())
                return;

            if (_endings.TryGetValue((IntPtr)wordPart, out Pair[] pairs))
            {
                var findWithLower = false;
                var len = pairs.Length;
                for (var i = 0; i < len; i++)
                {
                    var p = pairs[i];
                    var baseMorphoForm = p.BaseMorphoForm;
                    var first_char = *baseMorphoForm.Base;
                    if ((first_char != '\0') && _UPPER_INVARIANT_MAP[first_char] == first_char)
                    {
                        continue;
                    }

                    findWithLower = true;
                    var wfmi = new WordFormMorphology(baseMorphoForm, p.MorphoAttribute);
                    result.Add(wfmi);
                }

                if (!findWithLower)
                {
                    for (var i = 0; i < len; i++)
                    {
                        var p = pairs[i];
                        var wfmi = new WordFormMorphology(p.BaseMorphoForm, p.MorphoAttribute);
                        result.Add(wfmi);
                    }
                }
            }
        }
        private void FillWordFormMorphologies_StartsWithLowerLetter_Core(char* wordPart, List<WordFormMorphology> result)
        {
            if (!HasEndings())
                return;

            if (_endings.TryGetValue((IntPtr)wordPart, out Pair[] pairs))
            {
                for (int i = 0, len = pairs.Length; i < len; i++)
                {
                    var p = pairs[i];
                    var baseMorphoForm = p.BaseMorphoForm;
                    var first_char = *baseMorphoForm.Base;
                    if ((first_char != '\0') && _UPPER_INVARIANT_MAP[first_char] == first_char)
                    {
                        continue;
                    }

                    var wfmi = new WordFormMorphology(baseMorphoForm, p.MorphoAttribute);
                    result.Add(wfmi);
                }
            }
        }
        private void FillWordFormMorphologies_StartsWithUpperLetter_Core(char* wordPart, List<WordFormMorphology> result)
        {
            if (!HasEndings())
                return;

            if (_endings.TryGetValue((IntPtr)wordPart, out Pair[] pairs))
            {
                for (int i = 0, len = pairs.Length; i < len; i++)
                {
                    var p = pairs[i];
                    var baseMorphoForm = p.BaseMorphoForm;
                    var first_char = *baseMorphoForm.Base;
                    if ((first_char != '\0') && _UPPER_INVARIANT_MAP[first_char] != first_char)
                    {
                        continue;
                    }

                    var wfmi = new WordFormMorphology(baseMorphoForm, p.MorphoAttribute);
                    result.Add(wfmi);
                }
            }
        }
        private void FillWordForms_Core(char* wordPart, List<WordForm> result)
        {
            if (!HasEndings())
                return;

            if (_endings.TryGetValue((IntPtr)wordPart, out Pair[] pairs))
            {
                for (int i = 0, len = pairs.Length; i < len; i++)
                {
                    var p = pairs[i];
                    var partOfSpeech = p.BaseMorphoForm.PartOfSpeech;
                    var _base = p.BaseMorphoForm.Base;
                    fixed (char** morphoFormsEnding = p.BaseMorphoForm.MorphoFormEndings)
                    {
                        for (int j = 0, mf_len = p.BaseMorphoForm.MorphoFormEndings.Length; j < mf_len; j++)
                        {
                            /// получение словоформы
                            var wordForm = StringsHelper.CreateWordForm(_base, *(morphoFormsEnding + j));

                            var wf = new WordForm(wordForm, partOfSpeech);
                            result.Add(wf);
                        }
                    }
                }
            }
        }
        #endregion
    }
}