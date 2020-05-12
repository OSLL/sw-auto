using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzeResults.Errors
{
    public class UseOfForbiddenWordsError : Error
    {
        public UseOfForbiddenWordsError(string dictionaryName, string word)
            : base(ErrorType.UseOfForbiddenWord, "Использование запрещенного слова", word,
                  $"Использование данного слова запрещено списком {dictionaryName}.")
        {

        }

    }
}
