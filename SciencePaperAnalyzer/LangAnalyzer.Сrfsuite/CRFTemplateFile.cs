using System;
using System.Collections.Generic;
using System.Linq;

namespace LangAnalyzer.Сrfsuite
{
    /// <summary>
    /// Внутреннее представление шаблона для построения входных данных SRFSuitNER
    /// </summary>
    public sealed class CRFTemplateFile
    {
        private struct IndexCount : IEqualityComparer<IndexCount>
        {
            public static readonly IndexCount Instance = new IndexCount();

            public IndexCount(int index, int count)
            {
                Index = index;
                Count = count;
            }

            public int Index;
            public int Count;

            public bool Equals(IndexCount x, IndexCount y)
            {
                return (x.Index == y.Index) && (x.Count == y.Count);
            }
            public int GetHashCode(IndexCount obj)
            {
                return Index.GetHashCode() ^ Count.GetHashCode();
            }
        }

        private readonly int _minCrfAttributePosition;
        private readonly int _maxCrfAttributePosition;
        private readonly Dictionary<IndexCount, CRFNgram[]> _dictionary;

        /// <summary>
        /// Конструктор шаблона для построения входных данных SRFSuitNER
        /// </summary>
        /// <param name="columnNames">Наименования столбцов преобразованного входного файла</param>
        /// <param name="crfNgrams">шаблоны N-грамм</param>
		public CRFTemplateFile(char[] columnNames, CRFNgram[] crfNgrams)
        {
            CheckTemplate(columnNames, crfNgrams);

            ColumnNames = columnNames;
            CRFNgrams = crfNgrams;

            var positions = from crfNgram in CRFNgrams
                            from crfAttribute in crfNgram.CRFAttributes
                            select crfAttribute.Position;
            _minCrfAttributePosition = positions.Min();
            _maxCrfAttributePosition = positions.Max();

            _dictionary = new Dictionary<IndexCount, CRFNgram[]>(IndexCount.Instance);
        }

        /// <summary>
        /// Наименования столбцов преобразованного входного файла
        /// </summary>
        public char[] ColumnNames
        {
            get;
            private set;
        }

        /// <summary>
        /// шаблоны N-грамм
        /// </summary>
        public CRFNgram[] CRFNgrams
        {
            get;
            private set;
        }

        public CRFNgram[] GetCRFNgramsWhichCanTemplateBeApplied(int wordIndex, int wordsCount)
        {
            var i1 = wordIndex + _minCrfAttributePosition; if (0 < i1) i1 = 0;
            var i2 = wordsCount - (wordIndex + _maxCrfAttributePosition) - 1; if (0 < i2) i2 = 0;
            var wordIndexAndCountTuple = new IndexCount(i1, i2);

            if (!_dictionary.TryGetValue(wordIndexAndCountTuple, out CRFNgram[] ngrams))
            {
                var lst = new List<CRFNgram>();
                foreach (var crfNgram in CRFNgrams)
                {
                    if (crfNgram.CanTemplateBeApplied(wordIndex, wordsCount))
                    {
                        lst.Add(crfNgram);
                    }
                }
                ngrams = lst.ToArray();

                _dictionary.Add(wordIndexAndCountTuple, ngrams);
            }
            return (ngrams);
        }


        private static void CheckTemplate(char[] _columnNames, CRFNgram[] _crfNgrams)
        {
            var columnNames = new HashSet<char>(_columnNames);

            foreach (CRFNgram ngram in _crfNgrams)
            {
                foreach (CRFAttribute crfAttribute in ngram.CRFAttributes)
                {
                    if (!columnNames.Contains(crfAttribute.AttributeName))
                    {
                        throw new Exception("Аттрибут '" + crfAttribute.AttributeName + "' не содержащится в названиях столбцов CRF-файла-шаблона: '" +
                            string.Join("', '", _columnNames.Select(c => c.ToString())) + '\'');
                    }
                }
            }
        }
    };
}
