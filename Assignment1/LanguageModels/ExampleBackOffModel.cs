using System;
using System.Collections.Generic;
using System.Linq;

namespace UW.NLP.LanguageModels
{
    public class ExampleBackOffModel : BackOffModel
    {
        public ExampleBackOffModel()
            : base()
        { }

        public ExampleBackOffModel(LanguageModelSettings settings)
            : base(settings)
        { }

        public override double Probability(NGram nGram)
        {
            if (NGramCounter.GetNGramCount(nGram) > 0)
            {
                return GetP1(nGram);
            }
            else
            {
                NGram lastN_1Gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
                for (int i = 0; i < lastN_1Gram.NOrder; i++)
                {
                    lastN_1Gram[i] = nGram[i + 1];
                }

                if (NGramCounter.GetNGramCount(lastN_1Gram) > 0)
                {
                    return GetP2(nGram, lastN_1Gram);
                }
                else
                {
                    return GetP3(nGram);
                }
            }
        }

        private double GetP1(NGram nGram)
        {
            return GetPML(nGram);
        }

        private double GetP2(NGram nGram, NGram lastN_1Gram)
        {
            NGram firstN_1Gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
            for (int i = 0; i < firstN_1Gram.NOrder; i++)
            {
                firstN_1Gram[i] = nGram[i];
            }

            double backoffProbability = 0;
            NGram possibleN_1Gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
            possibleN_1Gram[0] = nGram[nGram.NOrder - 2];
            foreach (string word in GetListOfWordsForInexistentNGram(firstN_1Gram))
            {
                possibleN_1Gram[1] = word;
                backoffProbability += GetPML(possibleN_1Gram);
            }

            return GetPML(lastN_1Gram) / backoffProbability;
        }

        private double GetP3(NGram nGram)
        {
            NGram lastN_2Gram = new NGram(nGram.NOrder - 2, Settings.StringComparison);
            for (int i = 0; i < lastN_2Gram.NOrder; i++)
            {
                lastN_2Gram[i] = nGram[i + 2];
            }

            NGram middleN_2Gram = new NGram(nGram.NOrder - 2, Settings.StringComparison);
            for (int i = 0; i < middleN_2Gram.NOrder; i++)
            {
                middleN_2Gram[i] = nGram[i + 1];
            }

            double backoffProbability = 0;
            NGram possibleN_2Gram = new NGram(nGram.NOrder - 2, Settings.StringComparison);
            foreach (string word in GetListOfWordsForInexistentNGram(middleN_2Gram))
            {
                possibleN_2Gram[0] = word;
                backoffProbability += GetPML(possibleN_2Gram);
            }

            return GetPML(lastN_2Gram) / backoffProbability;
        }
    }
}
