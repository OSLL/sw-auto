using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Urls
{
    public class Url
    {
        public string value;
        public int startIndex;
        public int length;

        public override string ToString()
        {
            if (value != null)
                return $"{'\''}{value}' [{startIndex}:{length}]";
            return $"[{startIndex}:{length}]";
        }

        internal Url CreateCopy()
        {
            var url = new Url()
            {
                startIndex = startIndex,
                length = length,
                value = value,
            };
            return url;
        }
        unsafe internal UrlStruct ToUrlStruct(char* _base)
        {
            var url = new UrlStruct()
            {
                startPtr = _base + startIndex,
                length = length,
            };
            return url;
        }

        unsafe static internal Url ToUrl(UrlStruct url, char* _base)
        {
            var _url = new Url()
            {
                startIndex = (int)(url.startPtr - _base),
                length = url.length,
            };
            return _url;
        }
    }

    unsafe public struct UrlStruct
    {
        public char* startPtr;
        public int length;
    }

    public class UrlDetectorModel
    {
        public UrlDetectorModel(string urlDetectorResourcesXmlFilename)
        {
            var xdoc = XDocument.Load(urlDetectorResourcesXmlFilename);

            var firstLevelDomains = from xe in xdoc.Root.Element("first-level-domains").Elements()
                                    select xe.Value;
            var uriSchemes = from xe in xdoc.Root.Element("uri-schemes").Elements()
                             select xe.Value;

            Initialize(firstLevelDomains, uriSchemes);

            xdoc = null;
        }

        public UrlDetectorModel(IEnumerable<string> firstLevelDomains, IEnumerable<string> uriSchemes)
        {
            Initialize(firstLevelDomains, uriSchemes);
        }

        private void Initialize(IEnumerable<string> firstLevelDomains, IEnumerable<string> uriSchemes)
        {
            FirstLevelDomains = firstLevelDomains.ToHashset_4Urls();
            FirstLevelDomainsMaxLength = FirstLevelDomains.GetItemMaxLength_4Urls();

            URIschemes = uriSchemes.ToHashsetWithReverseValues_4Urls();
            URIschemesMaxLength = URIschemes.GetItemMaxLength_4Urls();
        }

        public HashSet<string> FirstLevelDomains
        {
            get;
            private set;
        }
        public int FirstLevelDomainsMaxLength
        {
            get;
            private set;
        }
        public HashSet<string> URIschemes
        {
            get;
            private set;
        }
        public int URIschemesMaxLength
        {
            get;
            private set;
        }
    }

    public class UrlDetectorConfig
    {
        public UrlDetectorConfig()
        {
        }
        public UrlDetectorConfig(string urlDetectorResourcesXmlFilename)
        {
            Model = new UrlDetectorModel(urlDetectorResourcesXmlFilename);
        }

        public UrlDetectorModel Model
        {
            get;
            set;
        }
        public UrlDetector.UrlExtractModeEnum UrlExtractMode
        {
            get;
            set;
        }
    }

    unsafe public sealed class UrlDetector : IDisposable
    {
        public enum UrlExtractModeEnum
        {
            ValueAndPosition,
            Position,
        }

        private const int DEFAULT_LIST_CAPACITY = 100;
        private const int ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING = 10;
        private readonly HashSet<string> _firstLevelDomains;
        private readonly int _firstLevelDomainsMaxLength;
        private readonly HashSet<string> _URIschemes;
        private readonly int _URIschemesMaxLength;
        private readonly bool _extractValue;
        private readonly List<Url> _urls;
        private readonly List<UrlStruct> _urlStructs;
        private readonly StringBuilder _stringBuilder;
        private readonly char[] _firstLevelDomainBuffer; //buffer for first-level-domain (right) part of url
        private readonly GCHandle _firstLevelDomainBufferGCHandle;
        private char* _fldBufferPtrBase;
        private readonly char[] _URIschemesBuffer;       //buffer for URI-schemes (left) part of url
        private readonly GCHandle _URIschemesBufferGCHandle;
        private char* _uriSchBufferPtrBase;
        private readonly Url _url;
        private readonly CharType* _CTM;  //xlat.CHARTYPE_MAP
        private readonly char* _UIM;  //xlat.UPPER_INVARIANT_MAP        
        private char* _BASE; //start pointer into text
        private char* _ptr;  //current pointer into text

        #region ctor dtor dispose
        public UrlDetector(UrlDetectorConfig config)
        {
            _extractValue = (config.UrlExtractMode == UrlExtractModeEnum.ValueAndPosition);

            _firstLevelDomains = config.Model.FirstLevelDomains;
            _firstLevelDomainsMaxLength = config.Model.FirstLevelDomainsMaxLength;

            _URIschemes = config.Model.URIschemes;
            _URIschemesMaxLength = config.Model.URIschemesMaxLength;

            _urls = new List<Url>(DEFAULT_LIST_CAPACITY);
            _stringBuilder = new StringBuilder();
            _url = new Url();
            _urlStructs = new List<UrlStruct>(DEFAULT_LIST_CAPACITY);

            _CTM = XlatUnsafe.Inst._CHARTYPE_MAP;
            _UIM = XlatUnsafe.Inst._UPPER_INVARIANT_MAP;

            _firstLevelDomainBuffer = new char[_firstLevelDomainsMaxLength + 1];
            _firstLevelDomainBufferGCHandle = GCHandle.Alloc(_firstLevelDomainBuffer, GCHandleType.Pinned);
            _fldBufferPtrBase = (char*)_firstLevelDomainBufferGCHandle.AddrOfPinnedObject().ToPointer();

            _URIschemesBuffer = new char[_URIschemesMaxLength + 1];
            _URIschemesBufferGCHandle = GCHandle.Alloc(_URIschemesBuffer, GCHandleType.Pinned);
            _uriSchBufferPtrBase = (char*)_URIschemesBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~UrlDetector()
        {
            DisposeNativeResources();
        }

        public void Dispose()
        {
            DisposeNativeResources();

            GC.SuppressFinalize(this);
        }

        private void DisposeNativeResources()
        {
            if (_fldBufferPtrBase != null)
            {
                _firstLevelDomainBufferGCHandle.Free();
                _fldBufferPtrBase = null;
            }

            if (_uriSchBufferPtrBase != null)
            {
                _URIschemesBufferGCHandle.Free();
                _uriSchBufferPtrBase = null;
            }
        }
        #endregion

        unsafe public List<Url> AllocateUrls(string text)
        {
            _urls.Clear();

            fixed (char* _base = text)
            {
                _BASE = _base;

                for (_ptr = _BASE; *_ptr != '\0'; _ptr++)
                {
                    switch (*_ptr)
                    {
                        //-dot-
                        case '.':
                            {
                                if (TryAllocateUrlByWWW())
                                {
                                    _urls.Add(_url.CreateCopy());
                                }
                                else if (TryAllocateUrlByFirstLevelDomain(ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING))
                                {
                                    _urls.Add(_url.CreateCopy());
                                }
                            }
                            break;

                        //-colon-
                        case ':':
                            {
                                if (TryAllocateUrlByUriSchemes())
                                {
                                    _urls.Add(_url.CreateCopy());
                                }
                            }
                            break;
                    }
                }
            }

            return _urls;
        }

        unsafe public List<UrlStruct> AllocateUrls(char* _base)
        {
            _urlStructs.Clear();

            _BASE = _base;

            for (_ptr = _BASE; *_ptr != '\0'; _ptr++)
            {
                switch (*_ptr)
                {
                    //-dot-
                    case '.':
                        {
                            if (TryAllocateUrlByWWW())
                            {
                                _urlStructs.Add(_url.ToUrlStruct(_base));
                            }
                            else if (TryAllocateUrlByFirstLevelDomain(ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING))
                            {
                                _urlStructs.Add(_url.ToUrlStruct(_base));
                            }
                        }
                        break;

                    //-colon-
                    case ':':
                        {
                            if (TryAllocateUrlByUriSchemes())
                            {
                                _urlStructs.Add(_url.ToUrlStruct(_base));
                            }
                        }
                        break;
                }
            }

            return _urlStructs;
        }

        unsafe public void InitializeAllocate(char* _base)
        {
            _BASE = _base;
        }

        unsafe public Url AllocateSingleUrl(char* ptr)
        {
            switch (*ptr)
            {
                //-dot-
                case '.':
                    {
                        _ptr = ptr;

                        if (TryAllocateUrlByWWW())
                        {
                            return (_url);
                        }

                        if (TryAllocateUrlByFirstLevelDomain(ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING))
                        {
                            return (_url);
                        }

                        return (null);
                    }

                //-colon-
                case ':':
                    {
                        _ptr = ptr;
                        if (TryAllocateUrlByUriSchemes())
                        {
                            return (_url);
                        }
                        return (null);
                    }
            }

            return null;
        }

        unsafe public Url AllocateSingleUrlByColon(char* ptr)
        {
            {
                _ptr = ptr;
                if (TryAllocateUrlByUriSchemes())
                {
                    return _url.CreateCopy();
                }
                return null;
            }
        }

        unsafe public Url AllocateSingleUrlByDot(char* ptr)
        {
            {
                _ptr = ptr;

                if (TryAllocateUrlByWWW())
                {
                    return _url.CreateCopy();
                }

                if (TryAllocateUrlByFirstLevelDomain(ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING))
                {
                    return _url.CreateCopy();
                }

                return null;
            }
        }

        unsafe public UrlStruct? AllocateSingleUrlstruct(char* ptr)
        {
            switch (*ptr)
            {
                //-dot-
                case '.':
                    {
                        _ptr = ptr;

                        if (TryAllocateUrlByWWW())
                        {
                            return _url.ToUrlStruct(_BASE);
                        }

                        if (TryAllocateUrlByFirstLevelDomain(ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING))
                        {
                            return _url.ToUrlStruct(_BASE);
                        }

                        return null;
                    }

                //-colon-
                case ':':
                    {
                        _ptr = ptr;
                        if (TryAllocateUrlByUriSchemes())
                        {
                            return _url.ToUrlStruct(_BASE);
                        }
                        return null;
                    }
            }

            return null;
        }
        unsafe public UrlStruct? AllocateSingleUrlstructByColon(char* ptr)
        {
            {
                _ptr = ptr;
                if (TryAllocateUrlByUriSchemes())
                {
                    return _url.ToUrlStruct(_BASE);
                }
                return null;
            }
        }
        unsafe public UrlStruct? AllocateSingleUrlstructByDot(char* ptr)
        {
            {
                _ptr = ptr;

                if (TryAllocateUrlByWWW())
                {
                    return _url.ToUrlStruct(_BASE);
                }

                if (TryAllocateUrlByFirstLevelDomain(ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING))
                {
                    return _url.ToUrlStruct(_BASE);
                }

                return null;
            }
        }

        private bool TryAllocateUrlByWWW()
        {
            const int WWW_LENGTH = 3;

            if (_ptr - WWW_LENGTH < _BASE)
            {
                return false;
            }
            var isWWW = (*(_UIM + *(_ptr - 1)) == 'W') &&
                        (*(_UIM + *(_ptr - 2)) == 'W') &&
                        (*(_UIM + *(_ptr - 3)) == 'W');
            if (!isWWW)
            {
                return false;
            }

            var right_len = FindUrlEndOnTheRight(0);

            var left_ptr = _ptr - WWW_LENGTH;
#if DEBUG
var xxx = new string( left_ptr - 25, 0, 75 );
#endif
            var length = WWW_LENGTH + 1 + right_len;
            _url.startIndex = (int)(left_ptr - _BASE);
            _url.length = length;
            if (_extractValue)
            {
                _url.value = new string(left_ptr, 0, length);
            }
            _ptr += 1 + right_len;
            return true;
        }

        private bool TryAllocateUrlByFirstLevelDomain(int maxRecursionNesting)
        {
            if (maxRecursionNesting <= 0)
            {
                return false;
            }

            char ch;
            var right_len = 0;
            for (_ptr++; ; right_len++)
            {
                ch = _ptr[right_len];

                if ((_CTM[ch] & CharType.IsLetter) != CharType.IsLetter)
                {
                    break;
                }

                if (_firstLevelDomainsMaxLength < right_len)
                {
                    return (false);
                }

                //to upper
                _fldBufferPtrBase[right_len] = _UIM[ch];
            }

            if (right_len == 0)
            {
                return false;
            }

            _stringBuilder.Clear().Append(_firstLevelDomainBuffer, 0, right_len);
            if (!_firstLevelDomains.Contains(_stringBuilder.ToString()))
            {
                return false;
            }

            if (Xlat.IsDot(ch))
            {
                var safe_Ptr = _ptr;
                _ptr += right_len;
                var res = TryAllocateUrlByFirstLevelDomain(maxRecursionNesting--);
                if (res)
                {
                    return true;
                }
                _ptr = safe_Ptr;
            }

            _ptr--;
            if (Xlat.IsURIschemesPathSeparator(ch))
            {
                right_len = FindUrlEndOnTheRight(right_len);
            }

            var left_len = FindUrlEndOnTheLeft(1);
            if (left_len == 0)
            {
                return false;
            }

            var left_ptr = _ptr - left_len;
#if DEBUG
var xxx = new string( left_ptr - 25, 0, 75 );
#endif
            var length = left_len + 1 + right_len;
            _url.startIndex = (int)(left_ptr - _BASE);
            _url.length = length;
            if (_extractValue)
            {
                _url.value = new string(left_ptr, 0, length);
            }
            _ptr += 1 + right_len;
            return true;
        }

        private bool TryAllocateUrlByUriSchemes()
        {
            var left_len = 0;
            for (; ; left_len++)
            {
                var p = _ptr - left_len - 1;
                if (p < _BASE)
                {
                    break;
                }

                var ch = *p;
                if ((_CTM[ch] & CharType.IsURIschemesChar) != CharType.IsURIschemesChar)
                {
                    break;
                }

                if (_URIschemesMaxLength < left_len)
                {
                    return false;
                }

                //to upper
                _uriSchBufferPtrBase[left_len] = _UIM[ch];
            }

            if (left_len == 0)
            {
                return false;
            }

            _stringBuilder.Clear().Append(_URIschemesBuffer, 0, left_len);
            if (!_URIschemes.Contains(_stringBuilder.ToString()))
            {
                _ptr++;
                return false;
            }

            var right_len = FindUrlEndOnTheRight(0);

            var left_ptr = _ptr - left_len;
#if DEBUG
var xxx = new string( left_ptr - 25, 0, 75 );
#endif
            var length = left_len + 1 + right_len;
            _url.startIndex = (int)(left_ptr - _BASE);
            _url.length = length;
            if (_extractValue)
            {
                _url.value = new string(left_ptr, 0, length);
            }
            _ptr += 1 + right_len;
            return true;
        }

        private int FindUrlEndOnTheRight(int offsetToRight)
        {
            var right_len = offsetToRight;
            for (; ; right_len++)
            {
                var ch = _ptr[right_len];

                if ((_CTM[ch] & CharType.IsUrlBreak) == CharType.IsUrlBreak)
                {
                    for (right_len--; 0 <= right_len; right_len--)
                    {
                        ch = _ptr[right_len];
                        if (ch == '/')
                            break;
                        if ((_CTM[ch] & CharType.IsPunctuation) != CharType.IsPunctuation)
                            break;
                    }
                    break;
                }
            }
            return (right_len > 0) ? right_len : 0;
        }

        private int FindUrlEndOnTheLeft(int offsetToLeft)
        {
            var left_len = offsetToLeft;
            for (; ; left_len++)
            {
                var p = _ptr - left_len;
                if (p <= _BASE)
                {
                    while (p < _BASE)
                    {
                        p++;
                        left_len--;
                    }

                    for (; 0 <= left_len; left_len--)
                    {
                        var ch = *(_ptr - left_len);
                        if (ch == '/')
                            break;
                        var ct = _CTM[ch];
                        if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                            continue;
                        if ((ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                            break;
                    }
                    break;
                }

                if ((_CTM[*p] & CharType.IsUrlBreak) == CharType.IsUrlBreak)
                {
                    for (left_len--; 0 <= left_len; left_len--)
                    {
                        var ch = *(_ptr - left_len);
                        if (ch == '/')
                            break;
                        var ct = _CTM[ch];
                        if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                            continue;
                        if ((ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                            break;
                    }
                    break;
                }
            }
            return (left_len > 0) ? left_len : 0;
        }
    }
}

namespace LangAnalyzerStd.Urls
{
    internal static class UrlDetectorExt
    {
        public static HashSet<string> ToHashset_4Urls(this IEnumerable<string> seq)
        {
            var hs = new HashSet<string>(seq.Select(d => d?.Trim().ToUpperInvariant()).Where(d => !string.IsNullOrEmpty(d)));
            return hs;
        }
        public static HashSet<string> ToHashsetWithReverseValues_4Urls(this IEnumerable<string> seq)
        {
            var hs = new HashSet<string>(seq.Select(d => (d != null) ? new string(d.Trim().Reverse().ToArray()).ToUpperInvariant() : null).Where(d => !string.IsNullOrEmpty(d)));
            return hs;
        }
        public static int GetItemMaxLength_4Urls(this HashSet<string> hs)
        {
            return (hs.Count != 0) ? hs.Max(d => d.Length) : 0;
        }
    }
}
