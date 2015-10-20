using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UW.NLP.LanguageModels.UnitTests
{
    public static class LanguageModelBaseTests<T> where T : LanguageModel, new()
    {
        private static string _trainingSet = @"I want to.
I want you";

        private static LanguageModelSettings _settings = new LanguageModelSettings
        {
            NGramOrder = 3,
            LogBase = 2,
            StartToken = "{{*}}",
            EndToken = "{{END}}",
            Separator = " ",
            PossibleEnd = ".",
            StringComparison = StringComparison.Ordinal,
            StringComparer = StringComparer.Ordinal
        };

        public static void Probability_FixedBigramTogenerateAllPossibleTrigrams(Action<double> assert)
        {
            T model = new T();
            model.Train(_trainingSet.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            NGram testTrigram = new NGram(3, StringComparison.Ordinal);
            testTrigram[0] = "I";
            testTrigram[1] = "want";

            double sumOfAllProbabilities = 0;
            foreach (string word in model.Vocabulary)
            {
                testTrigram[2] = word;
                sumOfAllProbabilities += model.Probability(testTrigram);
            }

            assert(Math.Round(sumOfAllProbabilities));
        }

        public static void Probability_AllPossibleTrigrams(Action<double> assert)
        {
            T model = new T();
            model.Train(_trainingSet.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            List<string> vocabulary = new List<string>(model.Vocabulary);

            for (int i = 0; i < vocabulary.Count; i++)
            {
                if (vocabulary[i] == _settings.EndToken)
                    continue;

                for (int j = 0; j < vocabulary.Count; j++)
                {
                    if (vocabulary[j] == _settings.EndToken)
                        continue;

                    NGram nGram = new NGram(3, StringComparison.Ordinal);
                    nGram[0] = vocabulary[i];
                    nGram[1] = vocabulary[j];

                    double sumOfAllProbabilities = 0;
                    for (int k = 0; k < vocabulary.Count; k++)
                    {
                        nGram[2] = vocabulary[k];
                        sumOfAllProbabilities += model.Probability(nGram);
                    }

                    assert(Math.Round(sumOfAllProbabilities));
                }
            }
        }

        public static void Probability_RandomSentencesFromVocabulary(int sentences, Action<double> assert)
        {
            T model = new T();
            model.Train(_trainingSet.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            List<string> vocabulary = new List<string>(model.Vocabulary);

            double sumProbabilities = 0;
            int i = 0;
            foreach (string sentence in GetTestCorpus(vocabulary, sentences).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                sumProbabilities += model.Probability(sentence);
                if (sumProbabilities > 1)
                {
                    System.Diagnostics.Trace.TraceInformation("Probabilty sums greater than one on i = {0}. Last sentence was <{1}>", i, sentence);
                    break;
                }
                i++;
            }

            System.Diagnostics.Trace.TraceInformation("Sum is: {0}", sumProbabilities);
            assert(sumProbabilities);
        }

        public static void TrainModel_RealCorpora_NoMemoryException()
        {
            T exampleModel = new T();
            using (StreamReader sr = new StreamReader("TestData\\brown.txt"))
            {
                exampleModel.Train(GetLines(sr));
            }

            exampleModel = new T();
            using (StreamReader sr = new StreamReader("TestData\\gutenberg.txt"))
            {
                exampleModel.Train(GetLines(sr));
            }

            exampleModel = new T();
            using (StreamReader sr = new StreamReader("TestData\\reuters.txt"))
            {
                exampleModel.Train(GetLines(sr));
            }
        }

        private static IEnumerable<string> GetLines(StreamReader sr)
        {
            string line = sr.ReadLine();
            while (line != null)
            {
                yield return line;
                line = sr.ReadLine();
            }
        }

        private static string GetTestCorpus(List<string> vocabulary, int sentences)
        {
            StringBuilder sb = new StringBuilder();
            Random r = new Random();
            HashSet<string> uniqueSentences = new HashSet<string>(_settings.StringComparer);

            for (int i = 0; i < sentences; i++)
            {
                string sentence = GetRandomSentence(r, vocabulary);
                if (!uniqueSentences.Contains(sentence))
                {
                    sb.AppendLine(sentence);
                    uniqueSentences.Add(sentence);
                }
                else
                {
                    i--;
                }
            }

            return sb.ToString();
        }

        private static string GetRandomSentence(Random r, List<string> vocabulary)
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int i = r.Next(0, vocabulary.Count);
                if (vocabulary[i] == _settings.EndToken)
                {
                    break;
                }
                sb.Append(vocabulary[i]);
                sb.Append(_settings.Separator);
            }

            return sb.ToString();
        }
    }
}
