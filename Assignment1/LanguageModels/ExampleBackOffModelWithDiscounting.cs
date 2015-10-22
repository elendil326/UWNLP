using System;

namespace UW.NLP.LanguageModels
{
    /// <summary>
    /// This is the implementation of the modified langauge model presented in Problem1 of Assignment1.
    /// This implementaiton takes in consideration discounting.
    /// </summary>
    public class ExampleBackOffModelWithDiscounting : BackOffModel
    {
        public ExampleBackOffModelWithDiscounting()
            : base()
        { }

        public ExampleBackOffModelWithDiscounting(LanguageModelSettings settings)
            : base(settings)
        { }

        /// <summary>
        /// Gets recursevily the probablity of the NGram.
        /// </summary>
        /// <param name="nGram">The Ngram to calculate</param>
        /// <returns>The probability of the NGram</returns>
        public override double Probability(NGram nGram)
        {
            Func<NGram, double> probabilityFunction;
            // If this is the bigram, stop recursion and use the PML formula instead.
            if (nGram.NOrder == 2)
            {
                probabilityFunction = GetPML;
            }
            else
            {
                probabilityFunction = Probability;
            }

            // If Ngram exists, call P1, else, calculate recurseivly using P2
            return NGramCounter.GetNGramCount(nGram) > 0 ? GetP1(nGram) : GetP2(nGram, probabilityFunction);
        }

        /// <summary>
        /// Gets the probability of an existent NGram.
        /// </summary>
        /// <param name="nGram">The NGram to calculate.</param>
        /// <returns>The probability of the NGram.</returns>
        private double GetP1(NGram nGram)
        {
            return GetPMLWithDiscount(nGram);
        }

        /// <summary>
        /// Gets the probability of a non-existent Ngram based on its last N-1Gram
        /// </summary>
        /// <param name="nGram">The NGram to calculate.</param>
        /// <param name="probabilityFunction">The probability function to use to calculate the probability of the N-1Grams.</param>
        /// <returns>The probabilty of the Ngram</returns>
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

            // Since A(u,v) and B(u,v) are exclusive, the sum of all probablities of w in B (u,v) can
            // be calculated by 1 - sum of all probabilities of w in A (u,v).
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
