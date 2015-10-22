using System;
using System.Collections.Generic;
using System.Linq;

namespace UW.NLP.LanguageModels
{
    /// <summary>
    /// Represents a Language Model. It contains shared logic that can be used by differnet language models.
    /// </summary>
    public abstract class LanguageModel : ILanguageModel
    {
        /// <summary>
        /// Uset to replace words with UNK based on the percentage.
        /// </summary>
        private HashSet<string> _wordsSeenOnlyOnce;

        /// <summary>
        /// The normalizer of the corpus.
        /// </summary>
        internal SentenceNormalizer Normalizer { get; private set; }

        /// <summary>
        /// The counter of Ngrams in the corpus.
        /// </summary>
        public NGramCounter NGramCounter { get; private set; }

        /// <summary>
        /// The cache of PML calculations.
        /// </summary>
        internal Dictionary<NGram, double> PMLCache { get; private set; }

        /// <summary>
        /// The settings of the language model.
        /// </summary>
        public LanguageModelSettings Settings { get; set; }

        /// <summary>
        /// The list of unique words that conform the langauge model.
        /// </summary>
        public HashSet<string> Vocabulary { get; private set; }

        /// <summary>
        /// The number of words found during the training.
        /// </summary>
        public int TotalWords { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class with default settings.
        /// </summary>
        /// <remarks>
        /// Some of these defualt settings, like lambda, beta and unk percentage, are the result
        /// of multiple tests and optimizations.
        /// </remarks>
        public LanguageModel()
        {
            Settings = new LanguageModelSettings
            {
                NGramOrder = 3,
                LogBase = 2,
                StartToken = "{{*}}",
                EndToken = "{{END}}",
                UnkToken = "{{UNK}}",
                UnkPercentage = 100,
                Separator = " ",
                PossibleEnd = ".",
                StringComparison = StringComparison.Ordinal,
                StringComparer = StringComparer.Ordinal
            };

            Init();
        }

        public LanguageModel(LanguageModelSettings settings)
        {
            Settings = settings;
            Init();
        }

        /// <summary>
        /// Calculates the probability of a sentencen in log space.
        /// </summary>
        /// <param name="sentence">The sentence to calculate</param>
        /// <param name="totalWords">After running, will contain the total number of words found in the sentence.</param>
        /// <param name="totalUnkWords">After running, will contain the total number of unkown words found in the sentence.</param>
        /// <returns></returns>
        public virtual double ProbabilityInLogSpace(string sentence, out int totalWords, out int totalUnkWords)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            totalWords = 0;
            totalUnkWords = 0;
            string normalizedSentence = Normalizer.Normalize(sentence);
            List<string> tokens = Normalizer.Tokenize(normalizedSentence).ToList();
            double sentenceProbability = 0;

            // The normalizer adds Start tokens. We don't need to start on them, but in the first real word.
            for (int currentTokenIndex = Settings.NGramOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                NGram currentNGram = new NGram(Settings.NGramOrder, Settings.StringComparison);

                // Replace Unkown words with corresponding UNK symbols
                if (!Vocabulary.Contains(tokens[currentTokenIndex]))
                {
                    tokens[currentTokenIndex] = Settings.UnkToken;
                    totalUnkWords++;
                }

                // Populate current NGram.
                for (int j = 0; j < currentNGram.NOrder; j++)
                {
                    currentNGram[j] = tokens[currentTokenIndex - currentNGram.NOrder + 1 + j];
                }
                totalWords++;

                // Store probability.
                sentenceProbability += Math.Log(Probability(currentNGram), Settings.LogBase);
            }

            return sentenceProbability;
        }

        /// <summary>
        /// Calculates the probability of a sentence.
        /// </summary>
        /// <param name="sentence">The sentence to calculate.</param>
        /// <returns>The probability of the sentence.</returns>
        public virtual double Probability(string sentence)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            string normalizedSentence = Normalizer.Normalize(sentence);
            List<string> tokens = Normalizer.Tokenize(normalizedSentence).ToList();
            double sentenceProbability = 1;

            // The normalizer adds Start tokens. We don't need to start on them, but in the first real word.
            for (int currentTokenIndex = Settings.NGramOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                NGram currentNGram = new NGram(Settings.NGramOrder, Settings.StringComparison);

                // Replace Unkown words with corresponding UNK symbols
                if (!Vocabulary.Contains(tokens[currentTokenIndex]))
                    tokens[currentTokenIndex] = Settings.UnkToken;

                // Popualte current NGram
                for (int j = 0; j < currentNGram.NOrder; j++)
                {
                    currentNGram[j] = tokens[currentTokenIndex - currentNGram.NOrder + 1 + j];
                }

                // Store probability by multiplying.
                sentenceProbability *= Probability(currentNGram);
            }

            return sentenceProbability;
        }

        /// <summary>
        /// Calculates the probability of an NGram.
        /// </summary>
        /// <param name="nGram">The NGram to calculate.</param>
        /// <returns>The probability of the NGram.</returns>
        public abstract double Probability(NGram nGram);

        /// <summary>
        /// Trains the Laguage Model with the list of sentences specified.
        /// </summary>
        /// <param name="sentences">The corpus of training.</param>
        public virtual void Train(IEnumerable<string> sentences)
        {
            if (sentences == null) throw new ArgumentNullException("sentences");

            List<List<string>> corpus = new List<List<string>>();
            foreach (string sentence in sentences)
            {
                // Normalize and tokenize the sentence.
                string normalizedSentence = Normalizer.Normalize(sentence);
                List<string> tokens = Normalizer.Tokenize(normalizedSentence).ToList();

                // Keep track of words seen only once to make them UNK later.
                for (int i = 0; i < tokens.Count; i++)
                {
                    TotalWords++;
                    Vocabulary.Add(tokens[i]);
                    if (_wordsSeenOnlyOnce.Contains(tokens[i]))
                    {
                        _wordsSeenOnlyOnce.Remove(tokens[i]);
                    }
                    else
                    {
                        _wordsSeenOnlyOnce.Add(tokens[i]);
                    }
                }
                corpus.Add(tokens);
            }

            // Remove start from vocabulary
            Vocabulary.Remove(Settings.StartToken);

            // Replace Unks as needed, depending on the configuration.
            int wordsToMakeUnkown = (int)Math.Round((Settings.UnkPercentage / 100) * _wordsSeenOnlyOnce.Count);
            foreach (List<string> sentence in corpus)
            {
                for (int i = 0; i < sentence.Count; i++)
                {
                    if (wordsToMakeUnkown <= 0)
                        break;

                    // If seen only once, remove it from the vocabulary.
                    if (_wordsSeenOnlyOnce.Contains(sentence[i]))
                    {
                        Vocabulary.Remove(sentence[i]);
                        sentence[i] = Settings.UnkToken;
                        wordsToMakeUnkown--;
                    }
                }

                NGramCounter.PopulateNGramCounts(sentence);
            }
        }

        /// <summary>
        /// Clears the cache to have a clean run after changing the settings.
        /// </summary>
        public abstract void ClearCacheForDifferentSettings();

        /// <summary>
        /// Clears the cache for a new training session.
        /// </summary>
        public virtual void ClearCache()
        {
            PMLCache = new Dictionary<NGram, double>();
        }

        /// <summary>
        /// Gets the PML of an NGram using the counting function.
        /// </summary>
        /// <param name="nGram">The Ngram to be calculated.</param>
        /// <returns>The probability of the NGram.</returns>
        internal double GetPML(NGram nGram)
        {
            if (PMLCache.ContainsKey(nGram))
                return PMLCache[nGram];

            double numerator = NGramCounter.GetNGramCount(nGram);
            double denominator = 0;

            // If NGram is Unigram, use the total words instead.
            if (nGram.NOrder == 1)
            {
                denominator = TotalWords;
            }
            else
            {
                // Popualte the first N-1Gram
                NGram N_1gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
                for (int i = 0; i < N_1gram.NOrder; i++)
                {
                    N_1gram[i] = nGram[i];
                }
                denominator = NGramCounter.GetNGramCount(N_1gram);
            }

            PMLCache[nGram] = numerator / denominator;
            return PMLCache[nGram];
        }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        private void Init()
        {
            Normalizer = new SentenceNormalizer(Settings.NGramOrder, Settings.StartToken, Settings.EndToken, Settings.Separator, Settings.PossibleEnd);
            NGramCounter = new NGramCounter(Settings);
            Vocabulary = new HashSet<string>(Settings.StringComparer);
            _wordsSeenOnlyOnce = new HashSet<string>(Settings.StringComparer);
            PMLCache = new Dictionary<NGram, double>();
        }
    }
}
