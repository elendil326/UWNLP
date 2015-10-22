using System;
using System.Collections.Generic;
using UW.NLP.LanguageModels;

namespace UW.NLP.LanguageModelValidator
{
    public class PerplexityCalculator
    {
        private LanguageModel _languageModel;

        public PerplexityCalculator(LanguageModel languageModel)
        {
            _languageModel = languageModel;
        }

        public double GetPerplexity(IEnumerable<string> corpus)
        {
            if (corpus == null) throw new ArgumentNullException("corpus");
            
            double sumOfProbabilities = 0;
            int totalWords = 0;
            foreach (string sentence in corpus)
            {
                int wordsInSentence;
                sumOfProbabilities += _languageModel.ProbabilityInLogSpace(sentence, out wordsInSentence);
                totalWords += wordsInSentence;
            }

            return Math.Pow(_languageModel.Settings.LogBase, (sumOfProbabilities / totalWords) * -1);
        }
    }
}
