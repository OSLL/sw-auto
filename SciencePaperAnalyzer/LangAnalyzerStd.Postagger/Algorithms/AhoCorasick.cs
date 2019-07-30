using System.Collections.Generic;
using System.Linq;

using LangAnalyzerStd.Tokenizing;

namespace LangAnalyzerStd.Postagger
{
    internal struct SearchResult
    {
        public SearchResult(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        public int StartIndex { get; private set; }
        public int Length { get; private set; }

        public override string ToString()
        {
            return $"[{StartIndex}:{Length}]";
        }
    }

    /// <summary>
    /// Class for searching string for one or multiple keywords using efficient Aho-Corasick search algorithm
    /// </summary>
    internal sealed class AhoCorasick
    {
        /// <summary>
        /// Tree node representing character and its transition and failure function
        /// </summary>
        private sealed class TreeNode
        {
            private sealed class StringsIEqualityComparer : IEqualityComparer<string[]>
            {
                public static readonly StringsIEqualityComparer Instance = new StringsIEqualityComparer();
                private StringsIEqualityComparer() { }

                public bool Equals(string[] x, string[] y)
                {
                    var len = x.Length;
                    if (len != y.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < len; i++)
                    {
                        if (!string.Equals(x[i], y[i]))
                        {
                            return false;
                        }
                    }
                    return (true);
                }

                public int GetHashCode(string[] obj)
                {
                    return obj.Length.GetHashCode();
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
            public void AddNgram(string[] ngram)
            {
                if (_ngrams == null)
                {
                    _ngrams = new HashSet<string[]>(StringsIEqualityComparer.Instance);
                }
                _ngrams.Add(ngram);
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
            /// <param name="c">Character</param>
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
            private HashSet<string[]> _ngrams;

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
            public IEnumerable<TreeNode> Transitions
            {
                get
                {
                    return (_transDict != null)
                        ? _transDict.Values
                        : Enumerable.Empty<TreeNode>();
                }
            }

            /// <summary>
            /// Returns list of patterns ending by this letter
            /// </summary>
            public IEnumerable<string[]> Ngrams { get { return _ngrams ?? Enumerable.Empty<string[]>(); } }
            public bool HasNgrams { get { return _ngrams != null; } }

            public override string ToString()
            {
                return $"{((Word != null) ? ('\'' + Word + '\'') : "ROOT")}, transitions(descendants): {((_transDict != null) ? _transDict.Count : 0)}, ngrams: {((_ngrams != null) ? _ngrams.Count : 0)}";
            }
        }

        private sealed class SearchResultIComparer : IComparer<SearchResult>
        {
            public static readonly SearchResultIComparer Instance = new SearchResultIComparer();
            private SearchResultIComparer() { }

            public int Compare(SearchResult x, SearchResult y)
            {
                var d = y.Length - x.Length;
                if (d != 0)
                    return d;

                return y.StartIndex - x.StartIndex;
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

        /// <summary>
        /// Root of keyword tree
        /// </summary>
        private readonly TreeNode _root;

        /// <summary>
        /// Initialize search algorithm (Build keyword tree)
        /// </summary>
        /// <param name="keywords">Keywords to search for</param>
        public AhoCorasick(IList<string[]> ngrams)
        {
            _root = new TreeNode(null, null);
            Count = ngrams.Count;
            BuildTree(ngrams);
        }

        /// <summary>
        /// Build tree from specified keywords
        /// </summary>
        private void BuildTree(IList<string[]> ngrams)
        {
            // Build keyword tree and transition function
            //---_Root = new TreeNode( null, null );
            foreach (var ngram in ngrams)
            {
                // add pattern to tree
                TreeNode node = _root;
                foreach (string word in ngram)
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
                        foreach (var result in node.Failure.Ngrams)
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

        public int Count { get; private set; }

        public SearchResult? FindFirstIngnoreCase(List<Word> words)
        {
            var ss = FindAllIngnoreCaseInternal(words);
            if (ss != null)
            {
                return ss.Min;
            }
            return null;
        }
        public SearchResult? FindFirstSensitiveCase(List<Word> words)
        {
            var ss = FindAllSensitiveCaseInternal(words);
            if (ss != null)
            {
                return ss.Min;
            }
            return null;
        }
        public ICollection<SearchResult> FindAllIngnoreCase(List<Word> words)
        {
            return FindAllIngnoreCaseInternal(words);
        }
        public ICollection<SearchResult> FindAllSensitiveCase(List<Word> words)
        {
            return FindAllSensitiveCaseInternal(words);
        }

        private SortedSet<SearchResult> FindAllIngnoreCaseInternal(List<Word> words)
        {
            var ss = default(SortedSet<SearchResult>);
            var finder = Finder.Create(_root);

            for (int index = 0, len = words.Count; index < len; index++)
            {
                var valueUpper = words[index].valueUpper;
                if (valueUpper == null)
                    continue;

                var node = finder.Find(valueUpper);

                if (node.HasNgrams)
                {
                    if (ss == null) ss = new SortedSet<SearchResult>(SearchResultIComparer.Instance);

                    foreach (var ngram in node.Ngrams)
                    {
                        ss.Add(new SearchResult(index - ngram.Length + 1, ngram.Length));
                    }
                }
            }
            return ss;
        }
        private SortedSet<SearchResult> FindAllSensitiveCaseInternal(List<Word> words)
        {
            var ss = default(SortedSet<SearchResult>);
            var finder = Finder.Create(_root);

            for (int index = 0, len = words.Count; index < len; index++)
            {
                var valueOriginal = words[index].valueOriginal;
                if (valueOriginal == null)
                    continue;

                var node = finder.Find(valueOriginal);

                if (node.HasNgrams)
                {
                    if (ss == null) ss = new SortedSet<SearchResult>(SearchResultIComparer.Instance);

                    foreach (var ngram in node.Ngrams)
                    {
                        ss.Add(new SearchResult(index - ngram.Length + 1, ngram.Length));
                    }
                }
            }
            return ss;
        }

        public override string ToString()
        {
            return $"[{_root}], count: {Count}";
        }
    }
}
