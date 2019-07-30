using System;
using System.Runtime.InteropServices;

namespace LangAnalyzerStd.Сrfsuite
{
    public enum NgramTypeEnum : int
    {
        Ngram_First = 0,
        Ngram_Middle = 1,
        Ngram_Last = 2,
    }

    public enum NgramOrderTypeEnum : int
    {
        NgramOrder_Default = 0,
        NgramOrder_BOS = 1,
        NgramOrder_EOS = 2,
    }

    unsafe public static class native
    {
        static native()
        {
            LoadNativeCrfSuite();
        }

        private static bool IsLinux()
        {
            var p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
        private static bool Isx64()
        {
            return (IntPtr.Size == 8);
        }

        private const string DLL_NAME_WINDOWS_x64 = "crfsuite_x64.dll";
        private const string DLL_NAME_WINDOWS_x86 = "crfsuite_x86.dll";
        private const string DLL_NAME_LINUX_x64 = "libcrfsuite.so";
        private const string DLL_NAME_LINUX_x86 = DLL_NAME_LINUX_x64;
        private static bool DLL_LOADED = false;
        private static readonly object _Lock = new object();

        private const string crf_tagger_initialize_name = "crf_tagger_initialize";
        private const string crf_tagger_beginAddItemSequence_name = "crf_tagger_beginAddItemSequence";
        private const string crf_tagger_beginAddItemAttribute_name = "crf_tagger_beginAddItemAttribute";
        private const string crf_tagger_addItemAttribute_name = "crf_tagger_addItemAttribute";
        private const string crf_tagger_addItemAttributeNameOnly_name = "crf_tagger_addItemAttributeNameOnly";
        private const string crf_tagger_endAddItemAttribute_name = "crf_tagger_endAddItemAttribute";
        private const string crf_tagger_endAddItemSequence_name = "crf_tagger_endAddItemSequence";
        private const string crf_tagger_tag_name = "crf_tagger_tag";
        private const string crf_tagger_tag_with_probability_name = "crf_tagger_tag_with_probability";
        private const string crf_tagger_tag_with_marginal_name = "crf_tagger_tag_with_marginal";
        private const string crf_tagger_getResultLength_name = "crf_tagger_getResultLength";
        private const string crf_tagger_getResultValue_name = "crf_tagger_getResultValue";
        private const string crf_tagger_getResultMarginal_name = "crf_tagger_getResultMarginal";
        private const string crf_tagger_uninitialize_name = "crf_tagger_uninitialize";

        private const string crf_tagger_ma_initialize_name = "crf_tagger_ma_initialize";
        private const string crf_tagger_ma_beginAddNgramSequence_name = "crf_tagger_ma_beginAddNgramSequence";
        private const string crf_tagger_ma_addNgramSequence_name = "crf_tagger_ma_addNgramSequence";
        private const string crf_tagger_ma_endAddNgramSequence_name = "crf_tagger_ma_endAddNgramSequence";
        private const string crf_tagger_ma_setNgramValue_name = "crf_tagger_ma_setNgramValue";
        private const string crf_tagger_ma_tagNgram_with_probability_name = "crf_tagger_ma_tagNgram_with_probability";
        private const string crf_tagger_ma_getResultValue_name = "crf_tagger_ma_getResultValue";
        private const string crf_tagger_ma_uninitialize_name = "crf_tagger_ma_uninitialize";

        public delegate IntPtr crf_tagger_initialize_Delegate(IntPtr name);
        public delegate void crf_tagger_beginAddItemSequence_Delegate(IntPtr taggerWrapper);
        public delegate void crf_tagger_beginAddItemAttribute_Delegate(IntPtr taggerWrapper);
        public delegate bool crf_tagger_addItemAttribute_Delegate(IntPtr taggerWrapper, byte* name, double val);
        public delegate void crf_tagger_addItemAttributeNameOnly_Delegate(IntPtr taggerWrapper, byte* name);
        public delegate void crf_tagger_endAddItemAttribute_Delegate(IntPtr taggerWrapper);
        public delegate void crf_tagger_endAddItemSequence_Delegate(IntPtr taggerWrapper);
        public delegate void crf_tagger_tag_Delegate(IntPtr taggerWrapper);
        public delegate double crf_tagger_tag_with_probability_Delegate(IntPtr taggerWrapper);
        public delegate double crf_tagger_tag_with_marginal_Delegate(IntPtr taggerWrapper);
        public delegate uint crf_tagger_getResultLength_Delegate(IntPtr taggerWrapper);
        public delegate IntPtr crf_tagger_getResultValue_Delegate(IntPtr taggerWrapper, uint index);
        public delegate double crf_tagger_getResultMarginal_Delegate(IntPtr taggerWrapper, uint index);
        public delegate void crf_tagger_uninitialize_Delegate(IntPtr taggerWrapper);

        public delegate IntPtr crf_tagger_ma_initialize_Delegate(IntPtr name);
        public delegate void crf_tagger_ma_beginAddNgramSequence_Delegate(IntPtr taggerWrapper, NgramTypeEnum ngramType);
        public delegate void crf_tagger_ma_addNgramSequence_Delegate(IntPtr taggerWrapper, byte* ngram);
        public delegate void crf_tagger_ma_endAddNgramSequence_Delegate(IntPtr taggerWrapper);
        public delegate void crf_tagger_ma_setNgramValue_Delegate(IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte*/*IntPtr*/ /*const char* */ value);
        public delegate double crf_tagger_ma_tagNgram_with_probability_Delegate(IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType);
        public delegate IntPtr crf_tagger_ma_getResultValue_Delegate(IntPtr taggerWrapper);
        public delegate void crf_tagger_ma_uninitialize_Delegate(IntPtr taggerWrapper);

        #region Windows
        #region x64
        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr Crf_tagger_initialize_win_x64(IntPtr name);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void Crf_tagger_beginAddItemSequence_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void Crf_tagger_beginAddItemAttribute_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool Crf_tagger_addItemAttribute_win_x64(IntPtr taggerWrapper, byte* name, double val);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name)]
        private extern static void Crf_tagger_addItemAttributeNameOnly_win_x64(IntPtr taggerWrapper, byte* name);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void Crf_tagger_endAddItemAttribute_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void Crf_tagger_endAddItemSequence_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void Crf_tagger_tag_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name)]
        private extern static double Crf_tagger_tag_with_probability_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name)]
        private extern static double Crf_tagger_tag_with_marginal_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint Crf_tagger_getResultLength_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr Crf_tagger_getResultValue_win_x64(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name)]
        private extern static double Crf_tagger_getResultMarginal_win_x64(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void Crf_tagger_uninitialize_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name)]
        private extern static IntPtr Crf_tagger_ma_initialize_win_x64(IntPtr name);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_beginAddNgramSequence_win_x64(IntPtr taggerWrapper, NgramTypeEnum ngramType);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name)]
        private extern static void Crf_tagger_ma_addNgramSequence_win_x64(IntPtr taggerWrapper, byte* ngram);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_endAddNgramSequence_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name)]
        private extern static void Crf_tagger_ma_setNgramValue_win_x64(IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name)]
        private extern static double Crf_tagger_ma_tagNgram_with_probability_win_x64(IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name)]
        private extern static IntPtr Crf_tagger_ma_getResultValue_win_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name)]
        private extern static void Crf_tagger_ma_uninitialize_win_x64(IntPtr taggerWrapper);
        #endregion

        #region x86
        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr Crf_tagger_initialize_win_x86(IntPtr name);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void Crf_tagger_beginAddItemSequence_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void Crf_tagger_beginAddItemAttribute_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool Crf_tagger_addItemAttribute_win_x86(IntPtr taggerWrapper, byte* name, double val);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name)]
        private extern static void Crf_tagger_addItemAttributeNameOnly_win_x86(IntPtr taggerWrapper, byte* name);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void Crf_tagger_endAddItemAttribute_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void Crf_tagger_endAddItemSequence_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void Crf_tagger_tag_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name)]
        private extern static double Crf_tagger_tag_with_probability_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name)]
        private extern static double Crf_tagger_tag_with_marginal_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint Crf_tagger_getResultLength_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr Crf_tagger_getResultValue_win_x86(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name)]
        private extern static double Crf_tagger_getResultMarginal_win_x86(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void Crf_tagger_uninitialize_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name)]
        private extern static IntPtr Crf_tagger_ma_initialize_win_x86(IntPtr name);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_beginAddNgramSequence_win_x86(IntPtr taggerWrapper, NgramTypeEnum ngramType);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name)]
        private extern static void Crf_tagger_ma_addNgramSequence_win_x86(IntPtr taggerWrapper, byte* ngram);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_endAddNgramSequence_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name)]
        private extern static void Crf_tagger_ma_setNgramValue_win_x86(IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name)]
        private extern static double Crf_tagger_ma_tagNgram_with_probability_win_x86(IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name)]
        private extern static IntPtr Crf_tagger_ma_getResultValue_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name)]
        private extern static void Crf_tagger_ma_uninitialize_win_x86(IntPtr taggerWrapper);
        #endregion
        #endregion

        #region Linux
        #region x64
        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr Crf_tagger_initialize_lin_x64(IntPtr name);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void Crf_tagger_beginAddItemSequence_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void Crf_tagger_beginAddItemAttribute_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool Crf_tagger_addItemAttribute_lin_x64(IntPtr taggerWrapper, byte* name, double val);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name)]
        private extern static void Crf_tagger_addItemAttributeNameOnly_lin_x64(IntPtr taggerWrapper, byte* name);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void Crf_tagger_endAddItemAttribute_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void Crf_tagger_endAddItemSequence_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void Crf_tagger_tag_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name)]
        private extern static double Crf_tagger_tag_with_probability_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name)]
        private extern static double Crf_tagger_tag_with_marginal_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint Crf_tagger_getResultLength_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr Crf_tagger_getResultValue_lin_x64(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name)]
        private extern static double Crf_tagger_getResultMarginal_lin_x64(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void Crf_tagger_uninitialize_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name)]
        private extern static IntPtr Crf_tagger_ma_initialize_lin_x64(IntPtr name);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_beginAddNgramSequence_lin_x64(IntPtr taggerWrapper, NgramTypeEnum ngramType);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name)]
        private extern static void Crf_tagger_ma_addNgramSequence_lin_x64(IntPtr taggerWrapper, byte* ngram);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_endAddNgramSequence_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name)]
        private extern static void Crf_tagger_ma_setNgramValue_lin_x64(IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name)]
        private extern static double Crf_tagger_ma_tagNgram_with_probability_lin_x64(IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name)]
        private extern static IntPtr Crf_tagger_ma_getResultValue_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name)]
        private extern static void Crf_tagger_ma_uninitialize_lin_x64(IntPtr taggerWrapper);
        #endregion

        #region x86
        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr Crf_tagger_initialize_lin_x86(IntPtr name);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void Crf_tagger_beginAddItemSequence_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void Crf_tagger_beginAddItemAttribute_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool Crf_tagger_addItemAttribute_lin_x86(IntPtr taggerWrapper, byte* name, double val);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name)]
        private extern static void Crf_tagger_addItemAttributeNameOnly_lin_x86(IntPtr taggerWrapper, byte* name);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void Crf_tagger_endAddItemAttribute_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void Crf_tagger_endAddItemSequence_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void Crf_tagger_tag_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name)]
        private extern static double Crf_tagger_tag_with_probability_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name)]
        private extern static double Crf_tagger_tag_with_marginal_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint Crf_tagger_getResultLength_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr Crf_tagger_getResultValue_lin_x86(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name)]
        private extern static double Crf_tagger_getResultMarginal_lin_x86(IntPtr taggerWrapper, uint index);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void Crf_tagger_uninitialize_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name)]
        private extern static IntPtr Crf_tagger_ma_initialize_lin_x86(IntPtr name);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_beginAddNgramSequence_lin_x86(IntPtr taggerWrapper, NgramTypeEnum ngramType);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name)]
        private extern static void Crf_tagger_ma_addNgramSequence_lin_x86(IntPtr taggerWrapper, byte* ngram);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name)]
        private extern static void Crf_tagger_ma_endAddNgramSequence_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name)]
        private extern static void Crf_tagger_ma_setNgramValue_lin_x86(IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name)]
        private extern static double Crf_tagger_ma_tagNgram_with_probability_lin_x86(IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name)]
        private extern static IntPtr Crf_tagger_ma_getResultValue_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name)]
        private extern static void Crf_tagger_ma_uninitialize_lin_x86(IntPtr taggerWrapper);
        #endregion
        #endregion

        public static crf_tagger_initialize_Delegate crf_tagger_initialize;
        public static crf_tagger_beginAddItemSequence_Delegate crf_tagger_beginAddItemSequence;
        public static crf_tagger_beginAddItemAttribute_Delegate crf_tagger_beginAddItemAttribute;
        public static crf_tagger_addItemAttribute_Delegate crf_tagger_addItemAttribute;
        public static crf_tagger_addItemAttributeNameOnly_Delegate crf_tagger_addItemAttributeNameOnly;
        public static crf_tagger_endAddItemAttribute_Delegate crf_tagger_endAddItemAttribute;
        public static crf_tagger_endAddItemSequence_Delegate crf_tagger_endAddItemSequence;
        public static crf_tagger_tag_Delegate crf_tagger_tag;
        public static crf_tagger_tag_with_probability_Delegate crf_tagger_tag_with_probability;
        public static crf_tagger_tag_with_marginal_Delegate crf_tagger_tag_with_marginal;
        public static crf_tagger_getResultLength_Delegate crf_tagger_getResultLength;
        public static crf_tagger_getResultValue_Delegate crf_tagger_getResultValue;
        public static crf_tagger_getResultMarginal_Delegate crf_tagger_getResultMarginal;
        public static crf_tagger_uninitialize_Delegate crf_tagger_uninitialize;

        public static crf_tagger_ma_initialize_Delegate crf_tagger_ma_initialize;
        public static crf_tagger_ma_beginAddNgramSequence_Delegate crf_tagger_ma_beginAddNgramSequence;
        public static crf_tagger_ma_addNgramSequence_Delegate crf_tagger_ma_addNgramSequence;
        public static crf_tagger_ma_endAddNgramSequence_Delegate crf_tagger_ma_endAddNgramSequence;
        public static crf_tagger_ma_setNgramValue_Delegate crf_tagger_ma_setNgramValue;
        public static crf_tagger_ma_tagNgram_with_probability_Delegate crf_tagger_ma_tagNgram_with_probability;
        public static crf_tagger_ma_getResultValue_Delegate crf_tagger_ma_getResultValue;
        public static crf_tagger_ma_uninitialize_Delegate crf_tagger_ma_uninitialize;

        public static void LoadNativeCrfSuite()
        {
            if (DLL_LOADED)
            {
                return;
            }

            lock (_Lock)
            {
                if (DLL_LOADED)
                {
                    return;
                }
                if (IsLinux())
                {
                    if (Isx64())
                    {
                        crf_tagger_initialize = Crf_tagger_initialize_lin_x64;
                        crf_tagger_beginAddItemSequence = Crf_tagger_beginAddItemSequence_lin_x64;
                        crf_tagger_beginAddItemAttribute = Crf_tagger_beginAddItemAttribute_lin_x64;
                        crf_tagger_addItemAttribute = Crf_tagger_addItemAttribute_lin_x64;
                        crf_tagger_addItemAttributeNameOnly = Crf_tagger_addItemAttributeNameOnly_lin_x64;
                        crf_tagger_endAddItemAttribute = Crf_tagger_endAddItemAttribute_lin_x64;
                        crf_tagger_endAddItemSequence = Crf_tagger_endAddItemSequence_lin_x64;
                        crf_tagger_tag = Crf_tagger_tag_lin_x64;
                        crf_tagger_tag_with_probability = Crf_tagger_tag_with_probability_lin_x64;
                        crf_tagger_tag_with_marginal = Crf_tagger_tag_with_marginal_lin_x64;
                        crf_tagger_getResultLength = Crf_tagger_getResultLength_lin_x64;
                        crf_tagger_getResultValue = Crf_tagger_getResultValue_lin_x64;
                        crf_tagger_getResultMarginal = Crf_tagger_getResultMarginal_lin_x64;
                        crf_tagger_uninitialize = Crf_tagger_uninitialize_lin_x64;

                        crf_tagger_ma_initialize = Crf_tagger_ma_initialize_lin_x64;
                        crf_tagger_ma_beginAddNgramSequence = Crf_tagger_ma_beginAddNgramSequence_lin_x64;
                        crf_tagger_ma_addNgramSequence = Crf_tagger_ma_addNgramSequence_lin_x64;
                        crf_tagger_ma_endAddNgramSequence = Crf_tagger_ma_endAddNgramSequence_lin_x64;
                        crf_tagger_ma_setNgramValue = Crf_tagger_ma_setNgramValue_lin_x64;
                        crf_tagger_ma_tagNgram_with_probability = Crf_tagger_ma_tagNgram_with_probability_lin_x64;
                        crf_tagger_ma_getResultValue = Crf_tagger_ma_getResultValue_lin_x64;
                        crf_tagger_ma_uninitialize = Crf_tagger_ma_uninitialize_lin_x64;
                    }
                    else
                    {
                        crf_tagger_initialize = Crf_tagger_initialize_lin_x86;
                        crf_tagger_beginAddItemSequence = Crf_tagger_beginAddItemSequence_lin_x86;
                        crf_tagger_beginAddItemAttribute = Crf_tagger_beginAddItemAttribute_lin_x86;
                        crf_tagger_addItemAttribute = Crf_tagger_addItemAttribute_lin_x86;
                        crf_tagger_addItemAttributeNameOnly = Crf_tagger_addItemAttributeNameOnly_lin_x86;
                        crf_tagger_endAddItemAttribute = Crf_tagger_endAddItemAttribute_lin_x86;
                        crf_tagger_endAddItemSequence = Crf_tagger_endAddItemSequence_lin_x86;
                        crf_tagger_tag = Crf_tagger_tag_lin_x86;
                        crf_tagger_tag_with_probability = Crf_tagger_tag_with_probability_lin_x86;
                        crf_tagger_tag_with_marginal = Crf_tagger_tag_with_marginal_lin_x86;
                        crf_tagger_getResultLength = Crf_tagger_getResultLength_lin_x86;
                        crf_tagger_getResultValue = Crf_tagger_getResultValue_lin_x86;
                        crf_tagger_getResultMarginal = Crf_tagger_getResultMarginal_lin_x86;
                        crf_tagger_uninitialize = Crf_tagger_uninitialize_lin_x86;

                        crf_tagger_ma_initialize = Crf_tagger_ma_initialize_lin_x86;
                        crf_tagger_ma_beginAddNgramSequence = Crf_tagger_ma_beginAddNgramSequence_lin_x86;
                        crf_tagger_ma_addNgramSequence = Crf_tagger_ma_addNgramSequence_lin_x86;
                        crf_tagger_ma_endAddNgramSequence = Crf_tagger_ma_endAddNgramSequence_lin_x86;
                        crf_tagger_ma_setNgramValue = Crf_tagger_ma_setNgramValue_lin_x86;
                        crf_tagger_ma_tagNgram_with_probability = Crf_tagger_ma_tagNgram_with_probability_lin_x86;
                        crf_tagger_ma_getResultValue = Crf_tagger_ma_getResultValue_lin_x86;
                        crf_tagger_ma_uninitialize = Crf_tagger_ma_uninitialize_lin_x86;
                    }
                }
                else
                {
                    if (Isx64())
                    {
                        crf_tagger_initialize = Crf_tagger_initialize_win_x64;
                        crf_tagger_beginAddItemSequence = Crf_tagger_beginAddItemSequence_win_x64;
                        crf_tagger_beginAddItemAttribute = Crf_tagger_beginAddItemAttribute_win_x64;
                        crf_tagger_addItemAttribute = Crf_tagger_addItemAttribute_win_x64;
                        crf_tagger_addItemAttributeNameOnly = Crf_tagger_addItemAttributeNameOnly_win_x64;
                        crf_tagger_endAddItemAttribute = Crf_tagger_endAddItemAttribute_win_x64;
                        crf_tagger_endAddItemSequence = Crf_tagger_endAddItemSequence_win_x64;
                        crf_tagger_tag = Crf_tagger_tag_win_x64;
                        crf_tagger_tag_with_probability = Crf_tagger_tag_with_probability_win_x64;
                        crf_tagger_tag_with_marginal = Crf_tagger_tag_with_marginal_win_x64;
                        crf_tagger_getResultLength = Crf_tagger_getResultLength_win_x64;
                        crf_tagger_getResultValue = Crf_tagger_getResultValue_win_x64;
                        crf_tagger_getResultMarginal = Crf_tagger_getResultMarginal_win_x64;
                        crf_tagger_uninitialize = Crf_tagger_uninitialize_win_x64;

                        crf_tagger_ma_initialize = Crf_tagger_ma_initialize_win_x64;
                        crf_tagger_ma_beginAddNgramSequence = Crf_tagger_ma_beginAddNgramSequence_win_x64;
                        crf_tagger_ma_addNgramSequence = Crf_tagger_ma_addNgramSequence_win_x64;
                        crf_tagger_ma_endAddNgramSequence = Crf_tagger_ma_endAddNgramSequence_win_x64;
                        crf_tagger_ma_setNgramValue = Crf_tagger_ma_setNgramValue_win_x64;
                        crf_tagger_ma_tagNgram_with_probability = Crf_tagger_ma_tagNgram_with_probability_win_x64;
                        crf_tagger_ma_getResultValue = Crf_tagger_ma_getResultValue_win_x64;
                        crf_tagger_ma_uninitialize = Crf_tagger_ma_uninitialize_win_x64;
                    }
                    else
                    {
                        crf_tagger_initialize = Crf_tagger_initialize_win_x86;
                        crf_tagger_beginAddItemSequence = Crf_tagger_beginAddItemSequence_win_x86;
                        crf_tagger_beginAddItemAttribute = Crf_tagger_beginAddItemAttribute_win_x86;
                        crf_tagger_addItemAttribute = Crf_tagger_addItemAttribute_win_x86;
                        crf_tagger_addItemAttributeNameOnly = Crf_tagger_addItemAttributeNameOnly_win_x86;
                        crf_tagger_endAddItemAttribute = Crf_tagger_endAddItemAttribute_win_x86;
                        crf_tagger_endAddItemSequence = Crf_tagger_endAddItemSequence_win_x86;
                        crf_tagger_tag = Crf_tagger_tag_win_x86;
                        crf_tagger_tag_with_probability = Crf_tagger_tag_with_probability_win_x86;
                        crf_tagger_tag_with_marginal = Crf_tagger_tag_with_marginal_win_x86;
                        crf_tagger_getResultLength = Crf_tagger_getResultLength_win_x86;
                        crf_tagger_getResultValue = Crf_tagger_getResultValue_win_x86;
                        crf_tagger_getResultMarginal = Crf_tagger_getResultMarginal_win_x86;
                        crf_tagger_uninitialize = Crf_tagger_uninitialize_win_x86;

                        crf_tagger_ma_initialize = Crf_tagger_ma_initialize_win_x86;
                        crf_tagger_ma_beginAddNgramSequence = Crf_tagger_ma_beginAddNgramSequence_win_x86;
                        crf_tagger_ma_addNgramSequence = Crf_tagger_ma_addNgramSequence_win_x86;
                        crf_tagger_ma_endAddNgramSequence = Crf_tagger_ma_endAddNgramSequence_win_x86;
                        crf_tagger_ma_setNgramValue = Crf_tagger_ma_setNgramValue_win_x86;
                        crf_tagger_ma_tagNgram_with_probability = Crf_tagger_ma_tagNgram_with_probability_win_x86;
                        crf_tagger_ma_getResultValue = Crf_tagger_ma_getResultValue_win_x86;
                        crf_tagger_ma_uninitialize = Crf_tagger_ma_uninitialize_win_x86;
                    }
                }

                DLL_LOADED = true;
            }
        }
    }
}
