using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UW.NLP.LanguageModels.UnitTests
{
    [TestClass]
    public class ExampleBackOffModelTests
    {
        private string _trainingSet = @"I want to eat.
I want to drink.
I want chinese food
This is Andres";

        [TestMethod]
        public void Probability_RandomSentencesFromVocabulary_ProbabilityGreaterThanOne()
        {
            ExampleBackOffModel example = new ExampleBackOffModel();
            example.TrainModel(_trainingSet.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            List<string> vocabulary = new List<string>(example.Vocabulary);

            double sumProbabilities = 0;
            foreach (string sentence in GetTestCorpus(vocabulary, 1000).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                sumProbabilities += example.Probability(sentence);
                Assert.IsTrue(sumProbabilities <= 1);
            }
        }

        private string GetTestCorpus(List<string> vocabulary, int sentences)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sentences; i++)
            {
                sb.AppendLine(GetRandomSentence(vocabulary));
            }

            return sb.ToString();
        }

        private string GetRandomSentence(List<string> vocabulary)
        {
            StringBuilder sb = new StringBuilder();
            string endToken = "{{END}}";
            string separator = " ";
            Random r = new Random();

            while (true)
            {
                int i = r.Next(0, vocabulary.Count);
                sb.Append(vocabulary[i]);
                sb.Append(separator);
                if (vocabulary[i] == endToken)
                {
                    break;
                }
            }

            return sb.ToString();
        }
    }
}
