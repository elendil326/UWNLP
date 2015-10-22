using System.Collections.Generic;

namespace UW.NLP.LanguageModels
{
    /// <summary>
    /// Represents a Back-off Model. Provides common methods for inheriters to use.
    /// </summary>
    public abstract class BackOffModel : LanguageModel
    {
        /// <summary>
        /// Caches the alpha results of an NGram.
        /// </summary>
        internal Dictionary<NGram, double> AlphaCache { get; private set; }

        /// <summary>
        /// Caches the result of computing the probability of an unexistent NGram.
        /// </summary>
        internal Dictionary<NGram, double> UnexistentNGramCache { get; private set; }

        /// <summary>
        /// Caches the result of computing the PML of an NGram with discount
        /// </summary>
        internal Dictionary<NGram, double> PMLWithDiscountCache { get; private set; }

        /// <summary>
        /// Caches the list of words that can conform an NGram based on the N-1Gram passed.
        /// </summary>
        internal Dictionary<NGram, HashSet<string>> ListOfWordsForExistentNGramCache { get; private set; }

        public BackOffModel()
            : base()
        {
            // Set the discount of each level to pass to the lower levels
            Settings.BackOffBetaPerOrder[1] = 0.75;
            Settings.BackOffBetaPerOrder[2] = 0.5;
            Settings.BackOffBetaPerOrder[3] = 0.5;
            Init();
        }

        public BackOffModel(LanguageModelSettings settings)
            : base(settings)
        {
            Init();
        }

        /// <summary>
        /// Clears the cache to have a clean run after changing the settings.
        /// </summary>
        public override void ClearCacheForDifferentSettings()
        {
            AlphaCache = new Dictionary<NGram, double>();
            UnexistentNGramCache = new Dictionary<NGram, double>();
            PMLWithDiscountCache = new Dictionary<NGram, double>();
        }

        /// <summary>
        /// Clears the cache for a new training session.
        /// </summary>
        public override void ClearCache()
        {
            base.ClearCache();
            ClearCacheForDifferentSettings();
            ListOfWordsForExistentNGramCache = new Dictionary<NGram, HashSet<string>>();
        }

        /// <summary>
        /// Gets the list of words that do not form an ngram with the N-1Gram passed.
        /// </summary>
        /// <param name="n_1gram">The fixed N-1Gram to use as base.</param>
        /// <returns>A Hashset of unique words that do no form a NGram with the N-1Gram passed.</returns>
        internal HashSet<string> GetListOfWordsForInexistentNGram(NGram n_1gram)
        {
            // Populate the possible NGram
            NGram possibleNGram = new NGram(n_1gram.NOrder + 1, Settings.StringComparison);
            for (int i = 0; i < n_1gram.NOrder; i++)
            {
                possibleNGram[i] = n_1gram[i];
            }

            // Traverse the vocabulry and look for words that form a trigram that hasn't been seen.
            HashSet<string> words = new HashSet<string>(Settings.StringComparer);
            foreach (string word in Vocabulary)
            {
                possibleNGram[n_1gram.NOrder] = word;
                if (NGramCounter.GetNGramCount(possibleNGram) == 0)
                {
                    words.Add(word);
                }
            }

            return words;
        }

        /// <summary>
        /// Gets the list of words that form an NGram with the base N-1Gram passed.
        /// </summary>
        /// <param name="n_1gram">The N-1Gram to use as base.</param>
        /// <returns>A hashset of unique words that form an ngram with the base N-1gram passed.</returns>
        internal HashSet<string> GetListOfWordsForExistentNGram(NGram n_1gram)
        {
            if (ListOfWordsForExistentNGramCache.ContainsKey(n_1gram))
                return ListOfWordsForExistentNGramCache[n_1gram];

            // Poopulate the possible NGram
            NGram possibleNGram = new NGram(n_1gram.NOrder + 1, Settings.StringComparison);
            for (int i = 0; i < n_1gram.NOrder; i++)
            {
                possibleNGram[i] = n_1gram[i];
            }

            // Traverse the vocabulary and look for NGrams that have been seen before.
            HashSet<string> words = new HashSet<string>(Settings.StringComparer);
            foreach (string word in Vocabulary)
            {
                possibleNGram[n_1gram.NOrder] = word;
                if (NGramCounter.GetNGramCount(possibleNGram) > 0)
                {
                    words.Add(word);
                }
            }

            ListOfWordsForExistentNGramCache[n_1gram] = words;
            return words;
        }

        /// <summary>
        /// Gets the PML calculation of an NGram with discount.
        /// </summary>
        /// <param name="nGram">The NGram to calculate</param>
        /// <returns>The PML with discount calculation of the NGram.</returns>
        internal double GetPMLWithDiscount(NGram nGram)
        {
            if (PMLWithDiscountCache.ContainsKey(nGram))
                return PMLWithDiscountCache[nGram];

            double numerator = NGramCounter.GetNGramCount(nGram) - Settings.BackOffBetaPerOrder[nGram.NOrder];
            double denominator = 0;

            // If this is an Unigram, the denominator is the total words of the corpus.
            if (nGram.NOrder == 1)
            {
                denominator = TotalWords;
            }
            else
            {
                // Get the count of the lower NGram.
                NGram N_1gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
                for (int i = 0; i < N_1gram.NOrder; i++)
                {
                    N_1gram[i] = nGram[i];
                }
                denominator = NGramCounter.GetNGramCount(N_1gram);
            }

            PMLWithDiscountCache[nGram] = numerator / denominator;
            return PMLWithDiscountCache[nGram];
        }

        /// <summary>
        /// Computes the Alpha value for a given NGram.
        /// </summary>
        /// <param name="n_1Gram">The N-1Gram to use as base.</param>
        /// <returns></returns>
        internal double Alpha(NGram n_1Gram)
        {
            if (AlphaCache.ContainsKey(n_1Gram))
                return AlphaCache[n_1Gram];

            double probability = 1;

            foreach (string word in  GetListOfWordsForExistentNGram(n_1Gram))
            {
                NGram possibleNGram = new NGram(n_1Gram.NOrder + 1, Settings.StringComparison);
                for (int i = 0; i < n_1Gram.NOrder; i++)
                {
                    possibleNGram[i] = n_1Gram[i];
                }
                possibleNGram[possibleNGram.NOrder - 1] = word;

                probability -= GetPMLWithDiscount(possibleNGram);
            }

            AlphaCache[n_1Gram] = probability;
            return probability;
        }

        /// <summary>
        /// Initialize the class.
        /// </summary>
        private void Init()
        {
            AlphaCache = new Dictionary<NGram, double>();
            UnexistentNGramCache = new Dictionary<NGram, double>();
            PMLWithDiscountCache = new Dictionary<NGram, double>();
            ListOfWordsForExistentNGramCache = new Dictionary<NGram, HashSet<string>>();
        }
    }
}
