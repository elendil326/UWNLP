using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UW.NLP.LanguageModels
{
    public abstract class BackOffModel : ILanguageModel
    {
        internal LanguageModelSettings Settings { get; private set; }

        internal SentenceNormalizer Normalizer { get; private set; }
        internal NGramCounter NGramCounter { get; private set; }

        public HashSet<string> Vocabulary { get { return NGramCounter.Vocabulary; } }

        public BackOffModel()
            :this(new LanguageModelSettings
            {
                NGramOrder = 3,
                LogBase = 2,
                StartToken = "{{*}}",
                EndToken = "{{END}}",
                Separator = " ",
                PossibleEnd = ".",
                StringComparison = StringComparison.Ordinal,
                StringComparer = StringComparer.Ordinal,
                BackOffBeta = 0.75
            })
        { }

        public BackOffModel(LanguageModelSettings settings)
        {
            Settings = settings;
            Normalizer = new SentenceNormalizer(settings.NGramOrder, settings.StartToken, settings.EndToken, settings.Separator, settings.PossibleEnd);
            NGramCounter = new NGramCounter(settings.NGramOrder, settings.StringComparison, settings.StringComparer);
        }

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

        public virtual double Probability(string sentence)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            string normalizedSentence = Normalizer.Normalize(sentence);
            List<string> tokens = Normalizer.Tokenize(normalizedSentence).ToList();
            double sentenceProbability = 1;

            for (int currentTokenIndex = Settings.NGramOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                NGram currentNGram = new NGram(Settings.NGramOrder, Settings.StringComparison);
                for (int j = 0; j < currentNGram.NOrder; j++)
                {
                    currentNGram[j] = tokens[currentTokenIndex - currentNGram.NOrder + 1 + j];
                }

                sentenceProbability *= Probability(currentNGram);
            }

            return sentenceProbability;
        }

        public abstract double Probability(NGram nGram);

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

        internal double GetPML(NGram nGram)
        {
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

            return numerator / denominator;
        }

        internal double GetPMLStar(NGram nGram)
        {
            double numerator = NGramCounter.GetNGramCount(nGram) - Settings.BackOffBeta;
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

            return numerator / denominator;
        }
    }
}
