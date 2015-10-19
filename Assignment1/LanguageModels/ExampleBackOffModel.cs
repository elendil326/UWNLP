using System;
using System.Collections.Generic;
using System.Linq;

namespace UW.NLP.LanguageModels
{
    public class ExampleBackOffModel
    {
        private int _nGramOrder = 3;
        private int _logBase = 2;
        private string _startToken = "{{*}}";
        private string _endToken = "{{END}}";
        private string _separator = " ";
        private string _possibleEnd = ".";
        private StringComparison _stringComparison = StringComparison.Ordinal;
        private StringComparer _stringComparer = StringComparer.Ordinal;

        private SentenceNormalizer _normalizer;
        private NGramCounter _nGramCounter;

        public HashSet<string> Vocabulary { get { return _nGramCounter.Vocabulary; } }

        public ExampleBackOffModel()
        {
            _normalizer = new SentenceNormalizer(_nGramOrder, _startToken, _endToken, _separator, _possibleEnd);
            _nGramCounter = new NGramCounter(_nGramOrder, _stringComparison);
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

            for (int currentTokenIndex = _nGramOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                NGram currentNGram = new NGram(_nGramOrder, _stringComparison);
                for (int j = 0; j < currentNGram.NOrder; j++)
                {
                    currentNGram[j] = tokens[currentTokenIndex - currentNGram.NOrder + 1 + j];
                }

                if (_nGramCounter.GetNGramCount(currentNGram) > 0)
                {
                    sentenceProbability *= GetPML(currentNGram);
                }
                else
                {
                    NGram lastN_1Gram = new NGram(currentNGram.NOrder - 1, _stringComparison);
                    for (int i = 0; i < lastN_1Gram.NOrder; i++)
                    {
                        lastN_1Gram[i] = currentNGram[i + 1];
                    }

                    if (_nGramCounter.GetNGramCount(lastN_1Gram) > 0)
                    {
                        NGram firstN_1Gram = new NGram(currentNGram.NOrder - 1, _stringComparison);
                        for (int i = 0; i < firstN_1Gram.NOrder; i++)
                        {
                            firstN_1Gram[i] = currentNGram[i];
                        }

                        double correlation = 0;
                        NGram possibleN_1Gram = new NGram(currentNGram.NOrder - 1, _stringComparison);
                        possibleN_1Gram[0] = currentNGram[currentNGram.NOrder - 2];
                        foreach (string word in GetListOfWordsForUnexistentNgram(firstN_1Gram))
                        {
                            possibleN_1Gram[1] = word;
                            correlation += GetPML(possibleN_1Gram);
                        }

                        sentenceProbability *= GetPML(lastN_1Gram) / correlation;
                    }
                    else
                    {
                        NGram lastN_2Gram = new NGram(currentNGram.NOrder - 2, _stringComparison);
                        for (int i = 0; i < lastN_2Gram.NOrder; i++)
                        {
                            lastN_2Gram[i] = currentNGram[i + 2];
                        }

                        NGram middleN_2Gram = new NGram(currentNGram.NOrder - 2, _stringComparison);
                        for (int i = 0; i < middleN_2Gram.NOrder; i++)
                        {
                            middleN_2Gram[i] = currentNGram[i + 1];
                        }

                        double correlation = 0;
                        NGram possibleN_2Gram = new NGram(currentNGram.NOrder - 2, _stringComparison);
                        foreach (string word in GetListOfWordsForUnexistentNgram(middleN_2Gram))
                        {
                            possibleN_2Gram[0] = word;
                            correlation += GetPML(possibleN_2Gram);
                        }

                        sentenceProbability *= GetPML(lastN_2Gram) / correlation;
                    }
                }
            }

            return sentenceProbability;
        }

        private HashSet<string> GetListOfWordsForUnexistentNgram(NGram N_1gram)
        {
            NGram possibleNGram = new NGram(N_1gram.NOrder + 1, _stringComparison);
            for (int i = 0; i < N_1gram.NOrder; i++)
            {
                possibleNGram[i] = N_1gram[i];
            }

            HashSet<string> words = new HashSet<string>(_stringComparer);
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
            int numerator = _nGramCounter.GetNGramCount(nGram);
            int denominator = 0;
            if (nGram.NOrder == 1)
            {
                denominator = _nGramCounter.TotalWords;
            }
            else
            {
                NGram N_1gram = new NGram(nGram.NOrder - 1, _stringComparison);
                for (int i = 0; i < N_1gram.NOrder; i++)
                {
                    N_1gram[i] = nGram[i];
                }
                denominator = _nGramCounter.GetNGramCount(N_1gram);
            }

            return (denominator == 0) ? double.MaxValue : numerator / denominator;
        }
    }
}
