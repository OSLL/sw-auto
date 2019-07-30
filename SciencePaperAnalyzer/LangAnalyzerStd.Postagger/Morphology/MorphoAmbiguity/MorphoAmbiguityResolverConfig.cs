using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Postagger
{
    public sealed class MorphoAmbiguityResolverConfig
    {
        public MorphoAmbiguityResolverConfig()
        {
        }
        public MorphoAmbiguityResolverConfig(string modelFilename, string templateFilename_5g, string templateFilename_3g)
        {
            ModelFilename = modelFilename;
            TemplateFilename5g = templateFilename_5g;
            TemplateFilename3g = templateFilename_3g;
        }

        public string ModelFilename
        {
            get;
            set;
        }
        public string TemplateFilename5g
        {
            get;
            set;
        }
        public string TemplateFilename3g
        {
            get;
            set;
        }
    }

    internal sealed class ByteIntPtrEqualityComparer : IEqualityComparer<IntPtr>
    {
        public ByteIntPtrEqualityComparer()
        {
        }

        unsafe private int getLength(byte* ptr)
        {
            for (var i = 0; ; i++)
            {
                if (*ptr == 0)
                    return i;
                ptr++;
            }
        }

        unsafe public bool Equals(IntPtr x, IntPtr y)
        {
            var x_ptr = (byte*)x.ToPointer();
            var y_ptr = (byte*)y.ToPointer();

            for (; ; x_ptr++, y_ptr++)
            {
                var x_ch = *x_ptr;
                var y_ch = *y_ptr;

                if (x_ch != y_ch)
                    return false;
                if (x_ch == '\0')
                    return true;
            }
        }
        unsafe public int GetHashCode(IntPtr obj)
        {
            byte* ptr = (byte*)obj.ToPointer();
            int num = 5381;
            int num2 = num;
            byte* ptr2 = ptr;
            int num3;
            while ((num3 = *ptr2) != 0)
            {
                num = ((num << 5) + num ^ num3);
                num2 = ((num2 << 5) + num2 ^ num3);
                ptr2++;
            }
            return num + num2 * 1566083941;
        }
    }

    public sealed class MorphoAmbiguityResolverModel : IDisposable
    {
        public MorphoAmbiguityResolverModel(MorphoAmbiguityResolverConfig config)
        {
            config.ThrowIfNull("config");
            config.ModelFilename.ThrowIfNullOrWhiteSpace("ModelFilename");
            config.TemplateFilename5g.ThrowIfNullOrWhiteSpace("TemplateFilename_5g");
            config.TemplateFilename3g.ThrowIfNullOrWhiteSpace("TemplateFilename_3g");

            Config = config;

            DictionaryBytes = LoadModelBytes(config.ModelFilename);
        }

        ~MorphoAmbiguityResolverModel()
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
            if (DictionaryBytes != null)
            {
                foreach (var p in DictionaryBytes)
                {
                    Marshal.FreeHGlobal(p.Key);
                }
                DictionaryBytes = null;
            }
        }

        unsafe private static Dictionary<IntPtr, float> LoadModelBytes(string modelFilename)
        {
            var NF = new NumberFormatInfo() { NumberDecimalSeparator = "." };
            var NS = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign;
            var f = default(float);

            var dict = new Dictionary<IntPtr, float>(500000, new ByteIntPtrEqualityComparer());

            using (var sr = new StreamReader(modelFilename))
            {
                for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
                {
                    string key = default;

                    fixed (char* _base = line)
                    {
                        var ptr = _base + line.Length - 1;
                        for (; _base <= ptr; ptr--)
                        {
                            if (*ptr == '\t')
                            {
                                *(ptr++) = '\0';
                                var value = new string(ptr);
                                f = float.Parse(value, NS, NF);
                                key = new string(_base, 0, (int)(ptr - _base));

                                break;
                            }
                        }
                    }

                    if (key == default)
                    {
                        throw new InvalidDataException($"Invalid data foramt: '{line}{'\''}");
                    }

                    var bytes = Encoding.UTF8.GetBytes(key);
                    var bytesPtr = Marshal.AllocHGlobal(bytes.Length);
                    Marshal.Copy(bytes, 0, bytesPtr, bytes.Length);

                    dict.Add(bytesPtr, f);
                }
            }

            return dict;
        }

        public MorphoAmbiguityResolverConfig Config
        {
            get;
            private set;
        }

        public Dictionary<IntPtr, float> DictionaryBytes
        {
            get;
            private set;
        }
    }
}
