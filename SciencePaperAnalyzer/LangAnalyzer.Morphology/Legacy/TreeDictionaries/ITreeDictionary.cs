using System.Collections.Generic;

namespace LangAnalyzer.Morphology
{
    internal interface ITreeDictionary
    {
        void AddWord(string word, MorphoType morphoType, MorphoAttributePair? nounType);
        bool GetWordFormMorphologies(string wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode);
        unsafe bool GetWordFormMorphologies(char* wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode);
        bool GetWordForms(string wordUpper, List<WordForm> result);
    }
}
