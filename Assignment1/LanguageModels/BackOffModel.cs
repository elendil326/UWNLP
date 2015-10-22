using System.Collections.Generic;

namespace UW.NLP.LanguageModels
{
    public abstract class BackOffModel : LanguageModel
    {
        internal Dictionary<NGram, double> AlphaCache { get; private set; }

        internal Dictionary<NGram, double> UnexistentNGramCache { get; private set; }

        internal Dictionary<NGram, double> PMLWithDiscountCache { get; private set; }

        internal Dictionary<NGram, HashSet<string>> ListOfWordsForExistentNGramCache { get; private set; }

        public BackOffModel()
            : base()
        {
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

        public override void ClearCacheForDifferentSettings()
        {
            AlphaCache = new Dictionary<NGram, double>();
            UnexistentNGramCache = new Dictionary<NGram, double>();
            PMLWithDiscountCache = new Dictionary<NGram, double>();
        }

        public override void ClearCache()
        {
            base.ClearCache();
            ClearCacheForDifferentSettings();
            ListOfWordsForExistentNGramCache = new Dictionary<NGram, HashSet<string>>();
        }

        internal HashSet<string> GetListOfWordsForInexistentNGram(NGram N_1gram)
        {
            NGram possibleNGram = new NGram(N_1gram.NOrder + 1, Settings.StringComparison);
            for (int i = 0; i < N_1gram.NOrder; i++)
            {
                possibleNGram[i] = N_1gram[i];
            }

            HashSet<string> words = new HashSet<string>(Settings.StringComparer);
            foreach (string word in Vocabulary)
            {
                possibleNGram[N_1gram.NOrder] = word;
                if (NGramCounter.GetNGramCount(possibleNGram) == 0)
                {
                    words.Add(word);
                }
            }

            return words;
        }

        internal HashSet<string> GetListOfWordsForExistentNGram(NGram n_1gram)
        {
            if (ListOfWordsForExistentNGramCache.ContainsKey(n_1gram))
                return ListOfWordsForExistentNGramCache[n_1gram];

            NGram possibleNGram = new NGram(n_1gram.NOrder + 1, Settings.StringComparison);
            for (int i = 0; i < n_1gram.NOrder; i++)
            {
                possibleNGram[i] = n_1gram[i];
            }

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

        internal double GetPMLWithDiscount(NGram nGram)
        {
            if (PMLWithDiscountCache.ContainsKey(nGram))
                return PMLWithDiscountCache[nGram];

            double numerator = NGramCounter.GetNGramCount(nGram) - Settings.BackOffBetaPerOrder[nGram.NOrder];
            double denominator = 0;
            if (nGram.NOrder == 1)
            {
                denominator = TotalWords;
            }
            else
            {
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

        private void Init()
        {
            AlphaCache = new Dictionary<NGram, double>();
            UnexistentNGramCache = new Dictionary<NGram, double>();
            PMLWithDiscountCache = new Dictionary<NGram, double>();
            ListOfWordsForExistentNGramCache = new Dictionary<NGram, HashSet<string>>();
        }
    }
}
