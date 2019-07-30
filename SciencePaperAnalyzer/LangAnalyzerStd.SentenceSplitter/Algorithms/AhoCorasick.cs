using System.Collections.Generic;
using System.Linq;

namespace LangAnalyzerStd.SentenceSplitter
{
    internal struct NGram<TValue>
    {
        public NGram(string[] _words, TValue _value)
        {
            Words = _words;
            Value = _value;
        }

        public string[] Words { get; private set; }
        public TValue Value { get; private set; }

        public override string ToString()
        {
            return $"{'\''}{string.Join("' '", Words)}' ({Words.Length}), '{Value}'";
        }
    }

    internal struct SearchResult<TValue>
    {
        public SearchResult(int startIndex, int length, TValue value)
        {
            StartIndex = startIndex;
            Length = length;
            Value = value;
        }

        public int StartIndex { get; private set; }
        public int Length { get; private set; }
        public TValue Value { get; private set; }

        public override string ToString()
        {
            var s = Value.ToString();
            if (string.IsNullOrEmpty(s))
            {
                return ($"[{StartIndex}:{Length}]");
            }
            return ($"[{StartIndex}:{Length}], value: '{s}'");
        }
    }

    internal struct SearchResultFromHead2Left<TValue>
    {
        public SearchResultFromHead2Left(SsWord lastWord, int length, TValue value)
        {
            LastWord = lastWord;
            Length = length;
            Value = value;
        }

        public SsWord LastWord { get; private set; }
        public int Length { get; private set; }
        public TValue Value { get; private set; }

        public override string ToString()
        {
            var s = Value.ToString();
            if (string.IsNullOrEmpty(s))
            {
                return $"[0:{Length}]";
            }
            return $"[0:{Length}], value: '{s}'";
        }
    }

    /// <summary>
    /// Class for searching string for one or multiple keywords using efficient Aho-Corasick search algorithm
    /// </summary>
    internal sealed class AhoCorasick<TValue>
    {
        /// <summary>
        /// Tree node representing character and its transition and failure function
        /// </summary>
        private sealed class TreeNode
        {
            private sealed class NGramEqualityComparer : IEqualityComparer<NGram<TValue>>
            {
                public static readonly NGramEqualityComparer Instance = new NGramEqualityComparer();
                private NGramEqualityComparer() { }

                public bool Equals(NGram<TValue> x, NGram<TValue> y)
                {
                    var len = x.Words.Length;
                    if (len != y.Words.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < len; i++)
                    {
                        if (!string.Equals(x.Words[i], y.Words[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                public int GetHashCode(NGram<TValue> obj)
                {
                    return obj.Words.Length.GetHashCode();
                }
            }

            /// <summary>
            /// Initialize tree node with specified character
            /// </summary>
            /// <param name="parent">Parent node</param>
            /// <param name="word">word</param>
            public TreeNode(TreeNode parent, string word)
            {
                Word = word;
                Parent = parent;
            }

            /// <summary>
            /// Adds pattern ending in this node
            /// </summary>
            /// <param name="ngram">Pattern</param>
            public void AddNgram(NGram<TValue> ngram)
            {
                if (_nGrams == null)
                {
                    _nGrams = new HashSet<NGram<TValue>>(NGramEqualityComparer.Instance);
                }
                _nGrams.Add(ngram);
            }

            /// <summary>
            /// Adds trabsition node
            /// </summary>
            /// <param name="node">Node</param>
            public void AddTransition(TreeNode node)
            {
                if (_transDict == null)
                {
                    _transDict = new Dictionary<string, TreeNode>();
                }
                _transDict.Add(node.Word, node);
            }

            /// <summary>
            /// Returns transition to specified character (if exists)
            /// </summary>
            /// <param name="word">word</param>
            /// <returns>Returns TreeNode or null</returns>
            public TreeNode GetTransition(string word)
            {
                if ((_transDict != null) && _transDict.TryGetValue(word, out TreeNode node))
                    return node;
                return null;
            }

            /// <summary>
            /// Returns true if node contains transition to specified character
            /// </summary>
            /// <param name="c">Character</param>
            /// <returns>True if transition exists</returns>
            public bool ContainsTransition(string word)
            {
                return (_transDict != null) && _transDict.ContainsKey(word);
            }

            private Dictionary<string, TreeNode> _transDict;
            private HashSet<NGram<TValue>> _nGrams;

            /// <summary>
            /// Character
            /// </summary>
            public string Word { get; private set; }

            /// <summary>
            /// Parent tree node
            /// </summary>
            public TreeNode Parent { get; private set; }

            /// <summary>
            /// Failure function - descendant node
            /// </summary>
            public TreeNode Failure { get; internal set; }

            /// <summary>
            /// Transition function - list of descendant nodes
            /// </summary>
            public IEnumerable<TreeNode> Transitions { get { return (_transDict != null) ? _transDict.Values : Enumerable.Empty<TreeNode>(); } }

            /// <summary>
            /// Returns list of patterns ending by this letter
            /// </summary>
            public IEnumerable<NGram<TValue>> Ngrams { get { return _nGrams ?? Enumerable.Empty<NGram<TValue>>(); } }
            public bool HasNgrams { get { return _nGrams != null; } }

            public override string ToString()
            {
                return $"{((Word != null) ? ('\'' + Word + '\'') : "ROOT")}, transitions(descendants): {((_transDict != null) ? _transDict.Count : 0)}, ngrams: {((_nGrams != null) ? _nGrams.Count : 0)}";
            }
        }

        private sealed class SearchResultComparer : IComparer<SearchResult<TValue>>
        {
            public static readonly SearchResultComparer Instance = new SearchResultComparer();
            private SearchResultComparer() { }

            public int Compare(SearchResult<TValue> x, SearchResult<TValue> y)
            {
                var d = y.Length - x.Length;
                if (d != 0)
                    return d;

                return x.StartIndex - y.StartIndex;
            }
        }

        private sealed class SearchResultOfHead2LeftComparer : IComparer<SearchResultFromHead2Left<TValue>>
        {
            public static readonly SearchResultOfHead2LeftComparer Instance = new SearchResultOfHead2LeftComparer();
            private SearchResultOfHead2LeftComparer() { }

            public int Compare(SearchResultFromHead2Left<TValue> x, SearchResultFromHead2Left<TValue> y)
            {
                return y.Length - x.Length;
            }
        }

        private struct Finder
        {
            private TreeNode _root;
            private TreeNode _node;
            public static Finder Create(TreeNode root) => new Finder() { _root = root, _node = root };

            public TreeNode Find(string word)
            {
                TreeNode transNode;
                do
                {
                    transNode = _node.GetTransition(word);
                    if (_node == _root)
                    {
                        break;
                    }
                    if (transNode == null)
                    {
                        _node = _node.Failure;
                    }
                }
                while (transNode == null);
                if (transNode != null)
                {
                    _node = transNode;
                }
                return _node;
            }
        }

        private readonly SearchResult<TValue>[] EMPTY_RESULT_1 = new SearchResult<TValue>[0];
        private readonly SearchResultFromHead2Left<TValue>[] EMPTY_RESULT_2 = new SearchResultFromHead2Left<TValue>[0];
        /// <summary>
        /// Root of keyword tree
        /// </summary>
        private readonly TreeNode _root;

        /// <summary>
        /// Initialize search algorithm (Build keyword tree)
        /// </summary>
        /// <param name="keywords">Keywords to search for</param>
        internal AhoCorasick(IList<NGram<TValue>> ngrams)
        {
            _root = new TreeNode(null, null);
            Count = ngrams.Count;
            if (0 < Count)
            {
                NgramMaxLength = ngrams.Max(ngram => ngram.Words.Length);
            }
            else
            {
                NgramMaxLength = 0;
            }
            BuildTree(ngrams);
        }

        /// <summary>
        /// Build tree from specified keywords
        /// </summary>
        private void BuildTree(IEnumerable<NGram<TValue>> ngrams)
        {
            // Build keyword tree and transition function
            //---_Root = new TreeNode( null, null );
            foreach (NGram<TValue> ngram in ngrams)
            {
                // add pattern to tree
                TreeNode node = _root;
                foreach (string word in ngram.Words)
                {
                    TreeNode nodeNew = null;
                    foreach (TreeNode trans in node.Transitions)
                    {
                        if (trans.Word == word)
                        {
                            nodeNew = trans;
                            break;
                        }
                    }

                    if (nodeNew == null)
                    {
                        nodeNew = new TreeNode(node, word);
                        node.AddTransition(nodeNew);
                    }
                    node = nodeNew;
                }
                node.AddNgram(ngram);
            }

            // Find failure functions
            var nodes = new List<TreeNode>();
            // level 1 nodes - fail to root node
            foreach (TreeNode node in _root.Transitions)
            {
                node.Failure = _root;
                foreach (TreeNode trans in node.Transitions)
                {
                    nodes.Add(trans);
                }
            }
            // other nodes - using BFS
            while (nodes.Count != 0)
            {
                var newNodes = new List<TreeNode>();
                foreach (TreeNode node in nodes)
                {
                    TreeNode r = node.Parent.Failure;
                    string word = node.Word;

                    while (r != null && !r.ContainsTransition(word))
                    {
                        r = r.Failure;
                    }
                    if (r == null)
                    {
                        node.Failure = _root;
                    }
                    else
                    {
                        node.Failure = r.GetTransition(word);
                        foreach (NGram<TValue> result in node.Failure.Ngrams)
                        {
                            node.AddNgram(result);
                        }
                    }

                    // add child nodes to BFS list 
                    foreach (TreeNode child in node.Transitions)
                    {
                        newNodes.Add(child);
                    }
                }
                nodes = newNodes;
            }
            _root.Failure = _root;
        }

        internal int Count { get; private set; }
        internal int NgramMaxLength { get; private set; }

        internal ICollection<SearchResult<TValue>> FindAll(DirectAccessList<SsWord> words)
        {
            var ss = default(SortedSet<SearchResult<TValue>>);
            var finder = Finder.Create(_root);

            for (int index = 0, len = words.Count; index < len; index++)
            {
                var node = finder.Find(words._Items[index].valueOriginal);

                if (node.HasNgrams)
                {
                    if (ss == null) ss = new SortedSet<SearchResult<TValue>>(SearchResultComparer.Instance);

                    foreach (var ngram in node.Ngrams)
                    {
                        var r = ss.Add(new SearchResult<TValue>(index - ngram.Words.Length + 1, ngram.Words.Length, ngram.Value));
                        System.Diagnostics.Debug.Assert(r);
                    }
                }
            }
            if (ss != null)
            {
                return ss;
            }
            return EMPTY_RESULT_1;
        }

        internal ICollection<SearchResultFromHead2Left<TValue>> FindFromHead2Left(SsWord headWord)
        {
            var ss = default(SortedSet<SearchResultFromHead2Left<TValue>>);
            var finder = Finder.Create(_root);
            int index = 0;

            for (var word = headWord; word != null; word = word.next)
            {
                var node = finder.Find(word.valueOriginal);

                if (node.HasNgrams)
                {
                    foreach (var ngram in node.Ngrams)
                    {
                        var wordIndex = index - ngram.Words.Length + 1;
                        if (wordIndex == 0)
                        {
                            if (ss == null) ss = new SortedSet<SearchResultFromHead2Left<TValue>>(SearchResultOfHead2LeftComparer.Instance);

                            var r = ss.Add(new SearchResultFromHead2Left<TValue>(word, ngram.Words.Length, ngram.Value));
                            System.Diagnostics.Debug.Assert(r);
                        }
                    }
                }
                index++;
            }
            if (ss != null)
            {
                return ss;
            }
            return EMPTY_RESULT_2;
        }

        public override string ToString()
        {
            return $"[{_root}], count: {Count}";
        }
    }
}
