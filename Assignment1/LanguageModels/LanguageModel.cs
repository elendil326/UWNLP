using System;
using System.Collections.Generic;
using System.Linq;

namespace UW.NLP.LanguageModels
{
    public abstract class LanguageModel : ILanguageModel
    {
        internal SentenceNormalizer Normalizer { get; private set; }

        internal NGramCounter NGramCounter { get; private set; }

        internal Dictionary<NGram, double> PMLCache { get; private set; }

        public LanguageModelSettings Settings { get; set; }

        public HashSet<string> Vocabulary { get { return NGramCounter.Vocabulary; } }

        public LanguageModel()
        {
            Settings = new LanguageModelSettings
            {
                NGramOrder = 3,
                LogBase = 2,
                StartToken = "{{*}}",
                EndToken = "{{END}}",
                UnkToken = "{{UNK}}",
                UnkAlphaNumericToken = "{{UNK_APLHANUMERIC}}",
                UnkNumberToken = "{{UNK_NUMBER}}",
                UnkSymbolToken = "{{UNK_SYMBOL}}",
                UnkWordToken = "{{UNK_WORD}}",
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

        public virtual double ProbabilityInLogSpace(string sentence)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            string normalizedSentence = Normalizer.Normalize(sentence);
            List<string> tokens = Normalizer.Tokenize(normalizedSentence).ToList();
            double sentenceProbability = 0;

            for (int currentTokenIndex = Settings.NGramOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                NGram currentNGram = new NGram(Settings.NGramOrder, Settings.StringComparison);

                // Replace Unkown words with corresponding UNK symbols
                if (!Vocabulary.Contains(tokens[currentTokenIndex]))
                    tokens[currentTokenIndex] = NGramCounter.GetUnkSymbol(tokens[currentTokenIndex]);

                for (int j = 0; j < currentNGram.NOrder; j++)
                {
                    currentNGram[j] = tokens[currentTokenIndex - currentNGram.NOrder + 1 + j];
                }

                sentenceProbability += Math.Log(Probability(currentNGram), Settings.LogBase);
            }

            return sentenceProbability;
        }

        public virtual double Probability(string sentence)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            string normalizedSentence = Normalizer.Normalize(sentence);
            List<string> tokens = Normalizer.Tokenize(normalizedSentence).ToList();
            double sentenceProbability = 1;

            for (int currentTokenIndex = Settings.NGramOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                NGram currentNGram = new NGram(Settings.NGramOrder, Settings.StringComparison);

                // Replace Unkown words with corresponding UNK symbols
                if (!Vocabulary.Contains(tokens[currentTokenIndex]))
                    tokens[currentTokenIndex] = NGramCounter.GetUnkSymbol(tokens[currentTokenIndex]);

                for (int j = 0; j < currentNGram.NOrder; j++)
                {
                    currentNGram[j] = tokens[currentTokenIndex - currentNGram.NOrder + 1 + j];
                }

                sentenceProbability *= Probability(currentNGram);
            }

            return sentenceProbability;
        }

        public abstract double Probability(NGram nGram);

        public virtual void Train(IEnumerable<string> sentences)
        {
            if (sentences == null) throw new ArgumentNullException("sentences");

            foreach (string sentence in sentences)
            {
                string normalizedSentence = Normalizer.Normalize(sentence);
                List<string> tokens = Normalizer.Tokenize(normalizedSentence).ToList();
                NGramCounter.PopulateNGramCounts(tokens);
            }
        }

        public abstract void ClearCacheForDifferentSettings();

        public virtual void ClearCache()
        {
            PMLCache = new Dictionary<NGram, double>();
        }

        internal double GetPML(NGram nGram)
        {
            if (PMLCache.ContainsKey(nGram))
                return PMLCache[nGram];

            double numerator = NGramCounter.GetNGramCount(nGram);
            double denominator = 0;
            if (nGram.NOrder == 1)
            {
                denominator = NGramCounter.TotalWords;
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

            PMLCache[nGram] = numerator / denominator;
            return PMLCache[nGram];
        }

        private void Init()
        {
            Normalizer = new SentenceNormalizer(Settings.NGramOrder, Settings.StartToken, Settings.EndToken, Settings.Separator, Settings.PossibleEnd);
            NGramCounter = new NGramCounter(Settings);
            PMLCache = new Dictionary<NGram, double>();
        }
    }
}
