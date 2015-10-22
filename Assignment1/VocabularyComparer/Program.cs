using System;
using System.Collections.Generic;
using System.IO;
using UW.NLP.LanguageModels;

namespace VocabularyComparer
{
    /// <summary>
    /// This programs compares two different sections of the same corpora to verify the percentage of unks between train and validation.
    /// I used to know how to manage UNKs, but then I realized doing 100% of words seen for the first time treated as UNKs worked better.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string file1 = @"C:\Users\azend\Documents\GitHubVisualStudio\UWNLP\Assignment1\LanguageModels.UnitTests\TestData\brown.txt";

            SentenceNormalizer normalizer = new SentenceNormalizer(1, "{{*}}", "{{END}}", " ", ".");
            HashSet<string> vocabulary = new HashSet<string>(StringComparer.Ordinal);

            List<List<string>> splitCorpus = SplitCorpus(file1, 80, 10, 10);
            foreach (string sentence in splitCorpus[0])
            {
                string normalizedSentence = normalizer.Normalize(sentence);
                foreach (string token in normalizer.Tokenize(normalizedSentence))
                {
                    vocabulary.Add(token);
                }
            }

            double validationWords = 0;
            double unkownWords = 0;
            foreach (string sentence in splitCorpus[1])
            {
                string normalizedSentence = normalizer.Normalize(sentence);
                foreach (string token in normalizer.Tokenize(normalizedSentence))
                {
                    validationWords++;
                    if (!vocabulary.Contains(token))
                    {
                        unkownWords++;
                    }
                }
            }

            Console.WriteLine("Total number of words in validation: {0}", validationWords);
            Console.WriteLine("Number of unseen words in validaiton is: {0}", unkownWords);
            Console.WriteLine("Percentage is: {0}", unkownWords / validationWords);
        }

        private static List<List<string>> SplitCorpus(string corpusPath, double trainingPercentage, double validatePercentage, double testPercentage)
        {
            if (trainingPercentage + validatePercentage + testPercentage != 100)
                throw new InvalidOperationException("Percentages must sum 100%");

            List<string> lines = new List<string>(File.ReadAllLines(corpusPath));
            int trainingLines = (int)Math.Round((trainingPercentage / 100) * lines.Count);
            int validateLines = (int)Math.Round((validatePercentage / 100) * lines.Count);
            int testLines = lines.Count - trainingLines - validateLines;

            List<List<string>> splittedCorpus = new List<List<string>>();
            splittedCorpus.Add(new List<string>());
            splittedCorpus.Add(new List<string>());
            splittedCorpus.Add(new List<string>());
            for (int i = 0; i < lines.Count; i++)
            {
                if (i < trainingLines)
                {
                    splittedCorpus[0].Add(lines[i]);
                }
                else if (i - trainingLines < validateLines)
                {
                    splittedCorpus[1].Add(lines[i]);
                }
                else
                {
                    splittedCorpus[2].Add(lines[i]);
                }
            }

            return splittedCorpus;
        }
    }
}
