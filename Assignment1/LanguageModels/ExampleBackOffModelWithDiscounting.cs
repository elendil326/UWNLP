using System;

namespace UW.NLP.LanguageModels
{
    public class ExampleBackOffModelWithDiscounting : BackOffModel
    {
        public ExampleBackOffModelWithDiscounting()
            : base()
        { }

        public ExampleBackOffModelWithDiscounting(LanguageModelSettings settings)
            : base(settings)
        { }

        public override double Probability(NGram nGram)
        {
            Func<NGram, double> probabilityFunction;
            if (nGram.NOrder == 2)
            {
                probabilityFunction = GetPML;
            }
            else
            {
                probabilityFunction = Probability;
            }
            return NGramCounter.GetNGramCount(nGram) > 0 ? GetP1(nGram) : GetP2(nGram, probabilityFunction);
        }

        private double GetP1(NGram nGram)
        {
            return GetPMLWithDiscount(nGram);
        }

        private double GetP2(NGram nGram, Func<NGram, double> probabilityFunction)
        {
            if (UnexistentNGramCache.ContainsKey(nGram))
                return UnexistentNGramCache[nGram];

            NGram firstN_1Gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
            for (int i = 0; i < firstN_1Gram.NOrder; i++)
            {
                firstN_1Gram[i] = nGram[i];
            }

            NGram lastN_1Gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
            for (int i = 0; i < lastN_1Gram.NOrder; i++)
            {
                lastN_1Gram[i] = nGram[i + 1];
            }

            NGram possibleLastN_1Gram = new NGram(lastN_1Gram.NOrder, Settings.StringComparison);
            for (int i = 0; i < lastN_1Gram.NOrder - 1; i++)
            {
                possibleLastN_1Gram[i] = lastN_1Gram[i];
            }

            double denominator = 1;
            foreach (string word in GetListOfWordsForExistentNGram(firstN_1Gram))
            {
                possibleLastN_1Gram[possibleLastN_1Gram.NOrder - 1] = word;
                denominator -= probabilityFunction(possibleLastN_1Gram);
            }

            UnexistentNGramCache[nGram] = Alpha(firstN_1Gram) * (probabilityFunction(lastN_1Gram) / denominator);
            return UnexistentNGramCache[nGram];
        }
    }
}
