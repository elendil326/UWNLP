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

            int numberOfSentences = 0;
            double sumOfProbabilities = 0;
            foreach (string sentence in corpus)
            {
                numberOfSentences++;
                sumOfProbabilities += _languageModel.ProbabilityInLogSpace(sentence);
            }

            return Math.Pow(_languageModel.Settings.LogBase, (sumOfProbabilities / numberOfSentences) * -1);
        }
    }
}
