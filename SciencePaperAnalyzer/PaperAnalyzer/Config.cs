using System;
using System.Configuration;
using System.Linq;

namespace PaperAnalyzer
{
    internal static class Config
    {
        public static readonly string URL_DETECTOR_RESOURCES_XML_FILENAME = ConfigurationManager.AppSettings["URL_DETECTOR_RESOURCES_XML_FILENAME"];
        public static readonly string SENT_SPLITTER_RESOURCES_XML_FILENAME = ConfigurationManager.AppSettings["SENT_SPLITTER_RESOURCES_XML_FILENAME"];
        public static readonly string TOKENIZER_RESOURCES_XML_FILENAME = ConfigurationManager.AppSettings["TOKENIZER_RESOURCES_XML_FILENAME"];
        public static readonly string POSTAGGER_MODEL_FILENAME = ConfigurationManager.AppSettings["POSTAGGER_MODEL_FILENAME"];
        public static readonly string POSTAGGER_TEMPLATE_FILENAME = ConfigurationManager.AppSettings["POSTAGGER_TEMPLATE_FILENAME"];
        public static readonly string POSTAGGER_RESOURCES_XML_FILENAME = ConfigurationManager.AppSettings["POSTAGGER_RESOURCES_XML_FILENAME"];

        public static readonly string MORPHO_BASE_DIRECTORY = ConfigurationManager.AppSettings["MORPHO_BASE_DIRECTORY"];
        public static readonly string[] MORPHO_MORPHOTYPES_FILENAMES = ConfigurationManager.AppSettings["MORPHO_MORPHOTYPES_FILENAMES"].ToFilesArray();
        public static readonly string[] MORPHO_PROPERNAMES_FILENAMES = ConfigurationManager.AppSettings["MORPHO_PROPERNAMES_FILENAMES"].ToFilesArray();
        public static readonly string[] MORPHO_COMMON_FILENAMES = ConfigurationManager.AppSettings["MORPHO_COMMON_FILENAMES"].ToFilesArray();

        private static string[] ToFilesArray(this string value)
        {
            if (value == null)
            {
                return new string[] { };
            }

            var array = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(f => f.Trim())
                             .ToArray();
            return array;
        }

        public static readonly string MORPHO_AMBIGUITY_MODEL_FILENAME = ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_MODEL_FILENAME"];
        public static readonly string MORPHO_AMBIGUITY_TEMPLATE_FILENAME_5G = ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_TEMPLATE_FILENAME_5G"];
        public static readonly string MORPHO_AMBIGUITY_TEMPLATE_FILENAME_3G = ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_TEMPLATE_FILENAME_3G"];
    }
}
