using LangAnalyzer.Tokenizing;

namespace LangAnalyzer.Ner
{
    public enum NerInputType : byte
    {
        #region common
        O,      // other's (другой)
        allC,   // Все заглавные буквы (больше одной) [МТС]        
        latC,   // Только первая заглавная на латинице [Fox]
        mixC,   // Смешенные заглавные и прописные буквы; 
                //русский   : {латиница + кириллица [СевКавГПУ]}, 
                //английский: {заглавные и строчные, первая буква - заглавная, между буквами может быть тире, точка: St.-Petersburg , FireFox, Google.Maps}
        mixCP,  // Все заглавные буквы (больше одной) подряд с точкой (точками) [V.IV.I.PA]
        numC,   // Начинается с заглавной буквы и содержит хотябы одну цифру [МИГ-21]
        oneC,   // Одна заглавная буква без точки [F]
        oneCP,  // одна заглавная буква с точкой [F.]        
        iProd,  // первая буква строчная; в слове нет точек; обязательно присутствует заглавная буква
        Q,      // кавычки ["«“”»]
        NUM,    // цифры в любой комбинации со знаками препинаний без букв [2,4 ; 10000 ; 2.456.542 ; 8:45]
        #endregion

        #region russian-language
        allatC, // все буквы заглавные и все на латинице [POP]
        latNum, // Хотя бы одна римская цифра буква (без точки) [XVI] [X-XI]        
        C,      // Только первая заглавная на кириллице [Вася]            
        COMA,   // запятую и точку с запятой - COMA
        #endregion

        #region english-language
        allCP, // все заглавные буквы (больше одной) с точкой (точками), без тире: [U.N.]
        Z,     // только первая заглавная:  [Thatcher]
        #endregion
    }

    public enum NerOutputType : byte
    {
        O = 0,
        NAME = 1,
        ORG = 2,
        GEO = 3,
        ENTR = 4,
        PROD = 5
    }

    public enum BuildModelNerInputType : byte
    {
        __UNDEFINED__ = 0,
        O,
        B_NAME, I_NAME,
        B_ORG, I_ORG,
        B_GEO, I_GEO,
        B_ENTR, I_ENTR,
        B_PROD, I_PROD,
        __UNKNOWN__
    }

    public struct Buildmodel_word_t
    {
        public Word word;
        public BuildModelNerInputType buildModelNerInputType;

        public override string ToString()
        {
            return '\'' + word.valueOriginal + "'  [" + word.startIndex + ":" + word.length + "]  " +
                    '\'' + word.nerInputType.ToString() + "'  " +
                    '\'' + ((buildModelNerInputType == BuildModelNerInputType.O) ? "-" : buildModelNerInputType.ToString()) + '\'';
        }
    }

    public static class NerExtensions
    {
        public static string ToText(this NerInputType nerInputType)
        {
            switch (nerInputType)
            {
                case NerInputType.numC: return ("numC");
                case NerInputType.latNum: return ("latNum");
                case NerInputType.oneC: return ("oneC");
                case NerInputType.allC: return ("allC");
                case NerInputType.allatC: return ("allatC");
                case NerInputType.oneCP: return ("oneCP");
                case NerInputType.mixCP: return ("mixCP");
                case NerInputType.mixC: return ("mixC");
                case NerInputType.latC: return ("latC");
                case NerInputType.C: return ("C");
                case NerInputType.Q: return ("Q");
                case NerInputType.iProd: return ("iProd");
                case NerInputType.NUM: return ("NUM");
                case NerInputType.COMA: return ("COMA");

                case NerInputType.allCP: return ("allCP");
                case NerInputType.Z: return ("C");
                default: return ("O");
            }
        }

        public static string ToText(this NerOutputType nerOutputType)
        {
            switch (nerOutputType)
            {
                case NerOutputType.NAME: return "NAME";
                case NerOutputType.ORG: return "ORG";
                case NerOutputType.GEO: return "GEO";
                case NerOutputType.ENTR: return "ENTR";
                case NerOutputType.PROD: return "PROD";
                default: return "O";
            }
        }

        public static char ToCrfChar(this NerInputType nerInputType)
        {
            switch (nerInputType)
            {
                case NerInputType.allC: return ('A');
                case NerInputType.allatC: return ('B');
                case NerInputType.latNum: return ('D');
                case NerInputType.mixCP: return ('C');
                case NerInputType.numC: return ('K');
                case NerInputType.oneC: return ('F');
                case NerInputType.oneCP: return ('H');
                case NerInputType.mixC: return ('X');
                case NerInputType.latC: return ('S');
                case NerInputType.C: return ('Z');
                case NerInputType.Q: return ('Q');
                case NerInputType.iProd: return ('L');
                case NerInputType.NUM: return ('N');
                case NerInputType.COMA: return ('Y');
                case NerInputType.allCP: return ('B');
                case NerInputType.Z: return ('C');
                default: return ('O');
            }
        }
        public static char ToCrfChar(this NerOutputType nerOutputType)
        {
            switch (nerOutputType)
            {
                case NerOutputType.NAME: return ('N');
                case NerOutputType.ORG: return ('J');
                case NerOutputType.GEO: return ('G');
                case NerOutputType.ENTR: return ('E');
                case NerOutputType.PROD: return ('P');
                default: return ('O');
            }
        }

        unsafe public static NerOutputType ToNerOutputType(byte* value)
        {
            switch (((char)*value++))
            {
                case 'B':
                case 'I':
                    {
                        var ch = (char)*value++;
                        if (ch != '-') break;

                        switch ((char)*value++)
                        {
                            case 'N': return (NerOutputType.NAME);
                            case 'J': return (NerOutputType.ORG);
                            case 'G': return (NerOutputType.GEO);
                            case 'E': return (NerOutputType.ENTR);
                            case 'P': return (NerOutputType.PROD);
                        }
                    }
                    break;
            }

            return NerOutputType.O;
        }

        public static string ToText(this BuildModelNerInputType buildModelNerInputType)
        {
            switch (buildModelNerInputType)
            {
                case BuildModelNerInputType.B_NAME: return "B-N";
                case BuildModelNerInputType.I_NAME: return "I-N";

                case BuildModelNerInputType.B_ORG: return "B-J";
                case BuildModelNerInputType.I_ORG: return "I-J";

                case BuildModelNerInputType.B_GEO: return "B-G";
                case BuildModelNerInputType.I_GEO: return "I-G";

                case BuildModelNerInputType.B_ENTR: return "B-E";
                case BuildModelNerInputType.I_ENTR: return "I-E";

                case BuildModelNerInputType.B_PROD: return "B-P";
                case BuildModelNerInputType.I_PROD: return "I-P";

                default: return ("O");
            }
        }
        public static BuildModelNerInputType ToBuildModelNerInputTypeB(this NerOutputType nerOutputType)
        {
            switch (nerOutputType)
            {
                case NerOutputType.NAME: return (BuildModelNerInputType.B_NAME);
                case NerOutputType.ORG: return (BuildModelNerInputType.B_ORG);
                case NerOutputType.GEO: return (BuildModelNerInputType.B_GEO);
                case NerOutputType.ENTR: return (BuildModelNerInputType.B_ENTR);
                case NerOutputType.PROD: return (BuildModelNerInputType.B_PROD);
                default: return (BuildModelNerInputType.O);
            }
        }
        public static BuildModelNerInputType ToBuildModelNerInputTypeI(this NerOutputType nerOutputType)
        {
            switch (nerOutputType)
            {
                case NerOutputType.NAME: return (BuildModelNerInputType.I_NAME);
                case NerOutputType.ORG: return (BuildModelNerInputType.I_ORG);
                case NerOutputType.GEO: return (BuildModelNerInputType.I_GEO);
                case NerOutputType.ENTR: return (BuildModelNerInputType.I_ENTR);
                case NerOutputType.PROD: return (BuildModelNerInputType.I_PROD);
                default: return (BuildModelNerInputType.O);
            }
        }
    }
}
