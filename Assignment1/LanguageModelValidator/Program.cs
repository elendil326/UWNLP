using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UW.NLP.LanguageModels;

namespace UW.NLP.LanguageModelValidator
{
    /// <summary>
    /// This program executes steps by step the questions in the assignment.
    /// 1. Analyzes the three corpus usin 80% training, 10% validaiton and 10% test with both linear interpolation and back-off.
    /// 2. Optimzes lambda based on the three corpus
    /// 3. Trains in one corpus, tests in a different one.
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            //ArgumentParser.Parse(args);
            //if (ArgumentParser.ShowedUsage) return;

            string brownCorpus = @"C:\Users\azend\Documents\GitHubVisualStudio\UWNLP\Assignment1\LanguageModels.UnitTests\TestData\brown.txt";
            string gutenberg = @"C:\Users\azend\Documents\GitHubVisualStudio\UWNLP\Assignment1\LanguageModels.UnitTests\TestData\gutenberg.txt";
            string reuters = @"C:\Users\azend\Documents\GitHubVisualStudio\UWNLP\Assignment1\LanguageModels.UnitTests\TestData\reuters.txt";

            // Analyze with Linear interpolation
            Console.WriteLine("First analyzing lambas");
            LinearInterpolationModel linearModel = new LinearInterpolationModel();
            Analyze(brownCorpus, linearModel);
            linearModel = new LinearInterpolationModel();
            Analyze(gutenberg, linearModel);
            linearModel = new LinearInterpolationModel();
            Analyze(reuters, linearModel);

            // Analyze with back-off
            Console.WriteLine("Analyze back-off");
            ExampleBackOffModelWithDiscounting backOff = new ExampleBackOffModelWithDiscounting();
            Analyze(brownCorpus, backOff);
            backOff = new ExampleBackOffModelWithDiscounting();
            Analyze(gutenberg, backOff);
            backOff = new ExampleBackOffModelWithDiscounting();
            Analyze(reuters, backOff);

            // Optimze Lambda
            List<List<string>> splitBrownCorpus = SplitCorpus(brownCorpus, 80, 10, 10);
            Console.WriteLine("Running optimizer with validation set for Brown.", 1000);
            linearModel = new LinearInterpolationModel();
            List<DoubleCombination> optimumCombinations = Optimizer.GetOptimumCombination(1000, linearModel, splitBrownCorpus[1]);

            Console.WriteLine("These are the optimum combinations:");
            foreach (DoubleCombination combination in optimumCombinations)
            {
                Console.WriteLine(combination.ToString());
            }

            // Optimze Lambda
            List<List<string>> splitGutenbergsCorpus = SplitCorpus(gutenberg, 80, 10, 10);
            Console.WriteLine("Running optimizer with validation set for Gutenberg.", 1000);
            linearModel = new LinearInterpolationModel();
            optimumCombinations = Optimizer.GetOptimumCombination(1000, linearModel, splitGutenbergsCorpus[1]);

            Console.WriteLine("These are the optimum combinations:");
            foreach (DoubleCombination combination in optimumCombinations)
            {
                Console.WriteLine(combination.ToString());
            }

            // Optimze Lambda
            List<List<string>> splitReutersCorpus = SplitCorpus(reuters, 80, 10, 10);
            Console.WriteLine("Running optimizer with validation set for Reuters.", 1000);
            linearModel = new LinearInterpolationModel();
            optimumCombinations = Optimizer.GetOptimumCombination(1000, linearModel, splitReutersCorpus[1]);

            Console.WriteLine("These are the optimum combinations:");
            foreach (DoubleCombination combination in optimumCombinations)
            {
                Console.WriteLine(combination.ToString());
            }

            // Train and test in differnet corpus
            Console.WriteLine("Train in Reuters, test in Brown");
            linearModel = new LinearInterpolationModel();
            Analyze(reuters, brownCorpus, linearModel);
            IEnumerable<KeyValuePair<NGram, int>> commonTrigrams = linearModel.NGramCounter.NGramCountDictionaries[3].OrderByDescending(kvp => kvp.Value).Take(100);
            Console.WriteLine("------------ Common 100 Trigrams -------");
            foreach (KeyValuePair<NGram, int> kvp in commonTrigrams)
            {
                Console.WriteLine("{0}\t\t{1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine();

            // Train and test in differnet corpus
            Console.WriteLine("Train in Brown, test in Gutenberg");
            linearModel = new LinearInterpolationModel();
            Analyze(brownCorpus, gutenberg, linearModel);
            commonTrigrams = linearModel.NGramCounter.NGramCountDictionaries[3].OrderByDescending(kvp => kvp.Value).Take(100);
            Console.WriteLine("------------ Common 100 Trigrams -------");
            foreach (KeyValuePair<NGram, int> kvp in commonTrigrams)
            {
                Console.WriteLine("{0}\t\t{1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine();

            // Train and test in differnet corpus
            Console.WriteLine("Train in Gutenbergs, test in Reuters");
            linearModel = new LinearInterpolationModel();
            Analyze(gutenberg, reuters, linearModel);
            commonTrigrams = linearModel.NGramCounter.NGramCountDictionaries[3].OrderByDescending(kvp => kvp.Value).Take(100);
            Console.WriteLine("------------ Common 100 Trigrams -------");
            foreach (KeyValuePair<NGram, int> kvp in commonTrigrams)
            {
                Console.WriteLine("{0}\t\t{1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine();
        }

        private static void Analyze(string corpusPath, LanguageModel lm)
        {
            Console.WriteLine("Corpus path: {0}", corpusPath);
            Console.WriteLine();

            Console.WriteLine("Splitting corpus.");
            List<List<string>> splitCorpus = SplitCorpus(corpusPath,80, 10, 10);
            Console.WriteLine("Splitted corpus as follow:");
            Console.WriteLine("Training: {0}", splitCorpus[0].Count);
            Console.WriteLine("Validate: {0}", splitCorpus[1].Count);
            Console.WriteLine("Test: {0}", splitCorpus[2].Count);

            Console.WriteLine("Training model.");
            lm.Train(splitCorpus[0]);

            Console.WriteLine("Calculate Perplextiy with validate set.");
            PerplexityCalculator perplexityCalculator = new PerplexityCalculator(lm);
            int unkWords = 0;
            double perplexity = perplexityCalculator.GetPerplexity(splitCorpus[1], out unkWords);
            Console.WriteLine("Found {0} unknown words:", unkWords);
            Console.WriteLine("Perplexity of validation is {0}", perplexity);

            perplexity = perplexityCalculator.GetPerplexity(splitCorpus[2], out unkWords);
            Console.WriteLine("Found {0} unknown words:", unkWords);
            Console.WriteLine("Perplexity of testing is {0}", perplexity);
        }

        private static void Analyze(string trainCorpusPath, string testCorpusPath, LanguageModel lm)
        {
            Console.WriteLine("Corpus path: {0}", trainCorpusPath);
            Console.WriteLine();

            Console.WriteLine("Splitting corpus.");
            List<List<string>> splitCorpus = SplitCorpus(trainCorpusPath, 100, 0, 0);
            Console.WriteLine("Splitted corpus as follow:");
            Console.WriteLine("Training: {0}", splitCorpus[0].Count);
            Console.WriteLine("Validate: {0}", splitCorpus[1].Count);
            Console.WriteLine("Test: {0}", splitCorpus[2].Count);

            Console.WriteLine("Training model.");
            lm.Train(splitCorpus[0]);

            List<List<string>> splitTestCorpus = SplitCorpus(testCorpusPath, 100, 0, 0);
            Console.WriteLine("Calculate Perplextiy with test corpus.");
            PerplexityCalculator perplexityCalculator = new PerplexityCalculator(lm);
            int unkWords = 0;
            double perplexity = perplexityCalculator.GetPerplexity(splitTestCorpus[0], out unkWords);
            Console.WriteLine("Found {0} unknown words:", unkWords);
            Console.WriteLine("Perplexity of testing is {0}", perplexity);
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
