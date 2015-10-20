using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UW.NLP.LanguageModels.UnitTests
{
    [TestClass]
    public class ExampleBackOffModelTests
    {
        private string _trainingSet = @"I want to.
I want you";

        private LanguageModelSettings _settings = new LanguageModelSettings
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

        [TestMethod]
        public void Probability_FixedBigramTogenerateAllPossibleTrigrams_SumIsCloseToTwo()
        {
            ExampleBackOffModel exampleModel = new ExampleBackOffModel();
            exampleModel.Train(_trainingSet.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            NGram testTrigram = new NGram(3, StringComparison.Ordinal);
            testTrigram[0] = "I";
            testTrigram[1] = "want";

            double sumOfAllProbabilities = 0;
            foreach (string word in exampleModel.Vocabulary)
            {
                testTrigram[2] = word;
                sumOfAllProbabilities += exampleModel.Probability(testTrigram);
            }

            Assert.AreEqual(2, Math.Round(sumOfAllProbabilities));
        }

        [TestMethod]
        public void Probability_AllPossibleTrigrams_SumIsCloseToTwoForEachOne()
        {
            ExampleBackOffModel exampleModel = new ExampleBackOffModel();
            exampleModel.Train(_trainingSet.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            List<string> vocabulary = new List<string>(exampleModel.Vocabulary);

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
                        sumOfAllProbabilities += exampleModel.Probability(nGram);
                    }

                    Assert.AreEqual(2, Math.Round(sumOfAllProbabilities));
                }
            }
        }

        [TestMethod]
        public void Probability_RandomSentencesFromVocabulary_ProbabilityGreaterThanOne()
        {
            ExampleBackOffModel exampleModel = new ExampleBackOffModel();
            exampleModel.Train(_trainingSet.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            List<string> vocabulary = new List<string>(exampleModel.Vocabulary);

            double sumProbabilities = 0;
            int i = 0;
            foreach (string sentence in GetTestCorpus(vocabulary, 1000).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                sumProbabilities += exampleModel.Probability(sentence);
                if (sumProbabilities > 1)
                {
                    System.Diagnostics.Trace.TraceInformation("Probabilty sums greater than one on i = {0}. Last sentence was <{1}>", i, sentence);
                    break;
                }
                i++;
            }

            Assert.IsTrue(sumProbabilities > 1);
        }

        [TestMethod]
        [TestCategory("Load")]
        public void TrainModel_RealCorpora_NoMemoryException()
        {
            ExampleBackOffModel exampleModel = new ExampleBackOffModel();
            using (StreamReader sr = new StreamReader("TestData\\brown.txt"))
            {
                exampleModel.Train(GetLines(sr));
            }

            exampleModel = new ExampleBackOffModel();
            using (StreamReader sr = new StreamReader("TestData\\gutenberg.txt"))
            {
                exampleModel.Train(GetLines(sr));
            }

            exampleModel = new ExampleBackOffModel();
            using (StreamReader sr = new StreamReader("TestData\\reuters.txt"))
            {
                exampleModel.Train(GetLines(sr));
            }
        }

        private IEnumerable<string> GetLines(StreamReader sr)
        {
            string line = sr.ReadLine();
            while (line != null)
            {
                yield return line;
                line = sr.ReadLine();
            }
        }

        private string GetTestCorpus(List<string> vocabulary, int sentences)
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

        private string GetRandomSentence(Random r, List<string> vocabulary)
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
