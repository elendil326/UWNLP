using System;
using System.Collections.Generic;
using System.Linq;

namespace UW.NLP.LanguageModels
{
    public class ExampleBackOffModel
    {
        private LanguageModelSettings _settings;

        private SentenceNormalizer _normalizer;
        private NGramCounter _nGramCounter;

        public HashSet<string> Vocabulary { get { return _nGramCounter.Vocabulary; } }

        public ExampleBackOffModel()
            :this(new LanguageModelSettings
            {
                NGramOrder = 3,
                LogBase = 2,
                StartToken = "{{*}}",
                EndToken = "{{END}}",
                Separator = " ",
                PossibleEnd = ".",
                StringComparison = StringComparison.Ordinal,
                StringComparer = StringComparer.Ordinal
            })
        { }

        public ExampleBackOffModel(LanguageModelSettings settings)
        {
            _settings = settings;
            _normalizer = new SentenceNormalizer(settings.NGramOrder, settings.StartToken, settings.EndToken, settings.Separator, settings.PossibleEnd);
            _nGramCounter = new NGramCounter(settings.NGramOrder, settings.StringComparison, settings.StringComparer);
        }

        public void TrainModel(IEnumerable<string> sentences)
        {
            if (sentences == null) throw new ArgumentNullException("sentences");

            foreach (string sentence in sentences)
            {
                string normalizedSentence = _normalizer.Normalize(sentence);
                List<string> tokens = _normalizer.Tokenize(normalizedSentence).ToList();
                _nGramCounter.PopulateNGramCounts(tokens);
            }
        }

        public double Probability(string sentence)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            string normalizedSentence = _normalizer.Normalize(sentence);
            List<string> tokens = _normalizer.Tokenize(normalizedSentence).ToList();
            double sentenceProbability = 1;

            for (int currentTokenIndex = _settings.NGramOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                NGram currentNGram = new NGram(_settings.NGramOrder, _settings.StringComparison);
                for (int j = 0; j < currentNGram.NOrder; j++)
                {
                    currentNGram[j] = tokens[currentTokenIndex - currentNGram.NOrder + 1 + j];
                }

                sentenceProbability *= Probability(currentNGram);
            }

            return sentenceProbability;
        }

        public double Probability(NGram nGram)
        {
            if (_nGramCounter.GetNGramCount(nGram) > 0)
            {
                return GetP1(nGram);
            }
            else
            {
                NGram lastN_1Gram = new NGram(nGram.NOrder - 1, _settings.StringComparison);
                for (int i = 0; i < lastN_1Gram.NOrder; i++)
                {
                    lastN_1Gram[i] = nGram[i + 1];
                }

                if (_nGramCounter.GetNGramCount(lastN_1Gram) > 0)
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
            NGram firstN_1Gram = new NGram(nGram.NOrder - 1, _settings.StringComparison);
            for (int i = 0; i < firstN_1Gram.NOrder; i++)
            {
                firstN_1Gram[i] = nGram[i];
            }

            double correlation = 0;
            NGram possibleN_1Gram = new NGram(nGram.NOrder - 1, _settings.StringComparison);
            possibleN_1Gram[0] = nGram[nGram.NOrder - 2];
            foreach (string word in GetListOfWordsForInexistentNgram(firstN_1Gram))
            {
                possibleN_1Gram[1] = word;
                correlation += GetPML(possibleN_1Gram);
            }

            return GetPML(lastN_1Gram) / correlation;
        }

        private double GetP3(NGram nGram)
        {
            NGram lastN_2Gram = new NGram(nGram.NOrder - 2, _settings.StringComparison);
            for (int i = 0; i < lastN_2Gram.NOrder; i++)
            {
                lastN_2Gram[i] = nGram[i + 2];
            }

            NGram middleN_2Gram = new NGram(nGram.NOrder - 2, _settings.StringComparison);
            for (int i = 0; i < middleN_2Gram.NOrder; i++)
            {
                middleN_2Gram[i] = nGram[i + 1];
            }

            double correlation = 0;
            NGram possibleN_2Gram = new NGram(nGram.NOrder - 2, _settings.StringComparison);
            foreach (string word in GetListOfWordsForInexistentNgram(middleN_2Gram))
            {
                possibleN_2Gram[0] = word;
                correlation += GetPML(possibleN_2Gram);
            }

            return GetPML(lastN_2Gram) / correlation;
        }

        private HashSet<string> GetListOfWordsForInexistentNgram(NGram N_1gram)
        {
            NGram possibleNGram = new NGram(N_1gram.NOrder + 1, _settings.StringComparison);
            for (int i = 0; i < N_1gram.NOrder; i++)
            {
                possibleNGram[i] = N_1gram[i];
            }

            HashSet<string> words = new HashSet<string>(_settings.StringComparer);
            foreach (string word in Vocabulary)
            {
                possibleNGram[N_1gram.NOrder] = word;
                if (_nGramCounter.GetNGramCount(possibleNGram) == 0)
                {
                    words.Add(word);
                }
            }

            return words;
        }

        private double GetPML(NGram nGram)
        {
            double numerator = _nGramCounter.GetNGramCount(nGram);
            double denominator = 0;
            if (nGram.NOrder == 1)
            {
                denominator = _nGramCounter.TotalWords;
            }
            else
            {
                NGram N_1gram = new NGram(nGram.NOrder - 1, _settings.StringComparison);
                for (int i = 0; i < N_1gram.NOrder; i++)
                {
                    N_1gram[i] = nGram[i];
                }
                denominator = _nGramCounter.GetNGramCount(N_1gram);
            }

            return numerator / denominator;
        }
    }
}
