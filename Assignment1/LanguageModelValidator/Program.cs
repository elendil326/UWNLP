using System;
using System.Collections.Generic;
using System.IO;

namespace UW.NLP.LanguageModelValidator
{
    public class Program
    {
        static void Main(string[] args)
        {
            ArgumentParser.Parse(args);
            if (ArgumentParser.ShowedUsage) return;

            Console.WriteLine("Corpus path: {0}", ArgumentParser.CorpusPath);
            Console.WriteLine();

            Console.WriteLine("Splitting corpus.");
            List<List<string>> splitCorpus = SplitCorpus(ArgumentParser.CorpusPath, ArgumentParser.TrainingPercentage, ArgumentParser.ValidatePercentage, ArgumentParser.TestPercentage);
            Console.WriteLine("Splitted corpus as follow:");
            Console.WriteLine("Training: {0}", splitCorpus[0].Count);
            Console.WriteLine("Validate: {0}", splitCorpus[1].Count);
            Console.WriteLine("Test: {0}", splitCorpus[2].Count);

            Console.WriteLine("Training model.", ArgumentParser.TrainingPercentage);
            ArgumentParser.LanguageModel.Train(splitCorpus[0]);

            Console.WriteLine("Calculate Perplextiy with validate set.");
            PerplexityCalculator perplexityCalculator = new PerplexityCalculator(ArgumentParser.LanguageModel);
            double perplexity = perplexityCalculator.GetPerplexity(splitCorpus[1]);

            if (ArgumentParser.Optimize)
            {
                Console.WriteLine("Running optimizer with validation set.", ArgumentParser.ValidatePercentage);
                List<DoubleCombination> optimumCombinations = Optimizer.GetOptimumCombination(1000, ArgumentParser.LanguageModel, splitCorpus[1]);

                Console.WriteLine("These are the optimum combinations:");
                foreach (DoubleCombination combination in optimumCombinations)
                {
                    Console.WriteLine(combination.ToString());
                }
            }
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
