namespace UW.NLP.LanguageModels
{
    /// <summary>
    /// The implementation of the Back-off model described in Problem 1 of Assignment 1
    /// </summary>
    public class ExampleBackOffModel : BackOffModel
    {
        public ExampleBackOffModel()
            : base()
        { }

        public ExampleBackOffModel(LanguageModelSettings settings)
            : base(settings)
        { }

        /// <summary>
        /// Gets the probability of the NGram.
        /// </summary>
        /// <param name="nGram">The NGram to calculate.</param>
        /// <returns>The probability of the NGram based on the current model's training.</returns>
        public override double Probability(NGram nGram)
        {
            // If the NGram has been seen, return the PML of it.
            if (NGramCounter.GetNGramCount(nGram) > 0)
            {
                return GetP1(nGram);
            }
            else
            {
                // If the last bigram has been seen, use P2 formula.
                NGram lastN_1Gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
                for (int i = 0; i < lastN_1Gram.NOrder; i++)
                {
                    lastN_1Gram[i] = nGram[i + 1];
                }

                if (NGramCounter.GetNGramCount(lastN_1Gram) > 0)
                {
                    return GetP2(nGram, lastN_1Gram);
                }
                // Use P3 to get the probability of the unigram.
                else
                {
                    return GetP3(nGram);
                }
            }
        }

        /// <summary>
        /// Gets the probability of an existent NGram.
        /// </summary>
        /// <param name="nGram">The Ngram to calculate.</param>
        /// <returns>The probability of an existent NGram.</returns>
        private double GetP1(NGram nGram)
        {
            return GetPML(nGram);
        }

        /// <summary>
        /// Gets the probaility of non-exitent NGram based on the last N-1Gram
        /// probability, which exists.
        /// </summary>
        /// <param name="nGram">The NGram.</param>
        /// <param name="lastN_1Gram">The last N-1Gram of NGram which exists.</param>
        /// <returns>The probabilty of the NGram.</returns>
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

        /// <summary>
        /// Gets the probablity of an Ngram based on the Unigram of the last word.
        /// </summary>
        /// <param name="nGram">The NGram to calculate.</param>
        /// <returns>The probability of the Ngram.</returns>
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
