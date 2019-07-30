using System;
using System.Runtime.InteropServices;

namespace LangAnalyzerStd.Сrfsuite
{
    /// <summary>
    /// N-грамма
    /// </summary>
    unsafe public sealed class CRFNgram : IDisposable
    {
        private readonly GCHandle _GCHandle;
        private char* _attributesHeaderBase;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crfAttributes">Составные части N-граммы</param>
        public CRFNgram(CRFAttribute[] crfAttributes)
        {
            CRFAttributes = crfAttributes;

            var attrs_len = CRFAttributes.Length;
            switch (attrs_len)
            {
                case 1:
                    {
                        CRFAttribute_0 = CRFAttributes[0];
                        AttributesHeader = $"{CRFAttribute_0.AttributeName}[{CRFAttribute_0.Position}{']'}{'='}";
                    }
                    break;
                case 2:
                    {
                        CRFAttribute_0 = CRFAttributes[0];
                        CRFAttribute_1 = CRFAttributes[1];
                        AttributesHeader = $"{CRFAttribute_0.AttributeName}[{CRFAttribute_0.Position}{']'}{'|'}{CRFAttribute_1.AttributeName}[{CRFAttribute_1.Position}{']'}{'='}";
                    }
                    break;
                case 3:
                    {
                        CRFAttribute_0 = CRFAttributes[0];
                        CRFAttribute_1 = CRFAttributes[1];
                        CRFAttribute_2 = CRFAttributes[2];
                        AttributesHeader = $"{CRFAttribute_0.AttributeName}[{CRFAttribute_0.Position}{']'}{'|'}{CRFAttribute_1.AttributeName}[{CRFAttribute_1.Position}{']'}{'|'}{CRFAttribute_2.AttributeName}[{CRFAttribute_2.Position}{']'}{'='}";
                    }
                    break;
                case 4:
                    {
                        CRFAttribute_0 = CRFAttributes[0];
                        CRFAttribute_1 = CRFAttributes[1];
                        CRFAttribute_2 = CRFAttributes[2];
                        CRFAttribute_3 = CRFAttributes[3];
                        AttributesHeader = $"{CRFAttribute_0.AttributeName}[{CRFAttribute_0.Position}{']'}{'|'}{CRFAttribute_1.AttributeName}[{CRFAttribute_1.Position}{']'}{'|'}{CRFAttribute_2.AttributeName}[{CRFAttribute_2.Position}{']'}{'|'}{CRFAttribute_3.AttributeName}[{CRFAttribute_3.Position}{']'}{'='}";
                    }
                    break;
                case 5:
                    {
                        CRFAttribute_0 = CRFAttributes[0];
                        CRFAttribute_1 = CRFAttributes[1];
                        CRFAttribute_2 = CRFAttributes[2];
                        CRFAttribute_3 = CRFAttributes[3];
                        CRFAttribute_4 = CRFAttributes[4];
                        AttributesHeader = $"{CRFAttribute_0.AttributeName}[{CRFAttribute_0.Position}{']'}{'|'}{CRFAttribute_1.AttributeName}[{CRFAttribute_1.Position}{']'}{'|'}{CRFAttribute_2.AttributeName}[{CRFAttribute_2.Position}{']'}{'|'}{CRFAttribute_3.AttributeName}[{CRFAttribute_3.Position}{']'}{'|'}{CRFAttribute_4.AttributeName}[{CRFAttribute_4.Position}{']'}{'='}";
                    }
                    break;
                case 6:
                    {
                        CRFAttribute_0 = CRFAttributes[0];
                        CRFAttribute_1 = CRFAttributes[1];
                        CRFAttribute_2 = CRFAttributes[2];
                        CRFAttribute_3 = CRFAttributes[3];
                        CRFAttribute_4 = CRFAttributes[4];
                        CRFAttribute_5 = CRFAttributes[5];
                        AttributesHeader = $"{CRFAttribute_0.AttributeName}[{CRFAttribute_0.Position}{']'}{'|'}{CRFAttribute_1.AttributeName}[{CRFAttribute_1.Position}{']'}{'|'}{CRFAttribute_2.AttributeName}[{CRFAttribute_2.Position}{']'}{'|'}{CRFAttribute_3.AttributeName}[{CRFAttribute_3.Position}{']'}{'|'}{CRFAttribute_4.AttributeName}[{CRFAttribute_4.Position}{']'}{'|'}{CRFAttribute_5.AttributeName}[{CRFAttribute_5.Position}{']'}{'='}";
                    }
                    break;
                default:
                    {
                        for (var j = 0; j < attrs_len; j++)
                        {
                            var attr = CRFAttributes[j];
                            AttributesHeader += $"{attr.AttributeName}[{attr.Position}{']'}{'|'}";
                        }
                        AttributesHeader = $"{AttributesHeader.Remove(AttributesHeader.Length - 1)}{'='}";
                    }
                    break;
            }

            CRFAttributesLength = attrs_len;
            AttributesHeaderLength = AttributesHeader.Length;

            _GCHandle = GCHandle.Alloc(AttributesHeader, GCHandleType.Pinned);
            _attributesHeaderBase = (char*)_GCHandle.AddrOfPinnedObject().ToPointer();
        }
        ~CRFNgram()
        {
            if (_attributesHeaderBase != null)
            {
                _GCHandle.Free();
                _attributesHeaderBase = null;
            }
        }

        public void Dispose()
        {
            if (_attributesHeaderBase != null)
            {
                _GCHandle.Free();
                _attributesHeaderBase = null;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Составные части N-граммы
        /// </summary>
        public readonly CRFAttribute[] CRFAttributes;

        public readonly int CRFAttributesLength;

        public CRFAttribute CRFAttribute_0
        {
            get;
            private set;
        }

        public CRFAttribute CRFAttribute_1
        {
            get;
            private set;
        }

        public CRFAttribute CRFAttribute_2
        {
            get;
            private set;
        }

        public CRFAttribute CRFAttribute_3
        {
            get;
            private set;
        }

        public CRFAttribute CRFAttribute_4
        {
            get;
            private set;
        }

        public CRFAttribute CRFAttribute_5
        {
            get;
            private set;
        }

        public readonly string AttributesHeader;
        public readonly int AttributesHeaderLength;
        unsafe public char* CopyAttributesHeaderChars(char* attributeBuffer)
        {
            for (var ptr = _attributesHeaderBase; ; ptr++)
            {
                var ch = *ptr;
                if (ch == '\0')
                    break;
                *(attributeBuffer++) = ch;
            }
            return attributeBuffer;
        }
        unsafe public byte* CopyAttributesHeaderChars(byte* attributeBuffer)
        {
            for (var ptr = _attributesHeaderBase; ; ptr++)
            {
                var ch = *ptr;
                if (ch == '\0')
                    break;
                *(attributeBuffer++) = (byte)ch;
            }
            return attributeBuffer;
        }

        public bool CanTemplateBeApplied(int wordIndex, int wordsCount)
        {
            foreach (CRFAttribute crfAttribute in CRFAttributes)
            {
                int index = wordIndex + crfAttribute.Position;
                if ((index < 0) || (wordsCount <= index))
                {
                    return (false);
                }
            }
            return true;
        }

        public override string ToString()
        {
            return AttributesHeader;
        }
    }
}