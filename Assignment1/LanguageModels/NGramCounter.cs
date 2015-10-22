using System;
using System.Collections.Generic;

namespace UW.NLP.LanguageModels
{
    /// <summary>
    /// Stores how many times an Ngram has been seen before.
    /// </summary>
    public class NGramCounter
    {
        private LanguageModelSettings _settings;

        /// <summary>
        /// The dictionary of NGram counts based on the order.
        /// </summary>
        public Dictionary<int, Dictionary<NGram, int>> NGramCountDictionaries { get; private set; }

        public int MaxNOrder { get; private set; }

        public NGramCounter(LanguageModelSettings settings)
        {
            MaxNOrder = settings.NGramOrder;
            _settings = settings;

            NGramCountDictionaries = new Dictionary<int, Dictionary<NGram, int>>();
            for (int i = 1; i < MaxNOrder + 1; i++)
            {
                NGramCountDictionaries[i] = new Dictionary<NGram, int>();
            }
        }

        public void PopulateNGramCounts(List<string> tokens)
        {
            // Populate NGrams of the START token. The NGram of higher order N doesn't have this count.
            for (int nOrder = 1; nOrder < MaxNOrder; nOrder++)
            {
                NGram currentNGram = new NGram(nOrder, _settings.StringComparison);
                for (int i = 0; i < currentNGram.NOrder; i++)
                {
                    currentNGram[i] = tokens[i];
                }

                if (!NGramCountDictionaries[nOrder].ContainsKey(currentNGram))
                {
                    NGramCountDictionaries[nOrder][currentNGram] = 0;
                }

                NGramCountDictionaries[nOrder][currentNGram] += MaxNOrder - nOrder;
            }

            // Populate the NGrams starting from the non-START token.
            for (int currentTokenIndex = MaxNOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                // Add the new NGram to each of the dictionaries.
                for (int nOrder = 1; nOrder < NGramCountDictionaries.Count + 1; nOrder++)
                {
                    NGram currentNGram = new NGram(nOrder, _settings.StringComparison);

                    // Populate the NGram with the previous words according to the NOrder.
                    for (int nGramTokenIndex = 0; nGramTokenIndex < currentNGram.NOrder; nGramTokenIndex++)
                    {
                        int previousTokenIndex = currentTokenIndex - currentNGram.NOrder + nGramTokenIndex + 1;
                        currentNGram[nGramTokenIndex] = tokens[previousTokenIndex];
                    }

                    // If the NGram is new, the counter dictionary won't have it, so add it.
                    if (!NGramCountDictionaries[nOrder].ContainsKey(currentNGram))
                    {
                        NGramCountDictionaries[nOrder][currentNGram] = 0;
                    }

                    // Increase the count of this nGram.
                    NGramCountDictionaries[nOrder][currentNGram]++;
                }
            }
        }

        public int GetNGramCount(NGram nGram)
        {
            if (nGram == null) throw new ArgumentNullException("ngram");

            return (!NGramCountDictionaries.ContainsKey(nGram.NOrder) || !NGramCountDictionaries[nGram.NOrder].ContainsKey(nGram))
                ? 0
                : NGramCountDictionaries[nGram.NOrder][nGram]; 
        }
    }
}
