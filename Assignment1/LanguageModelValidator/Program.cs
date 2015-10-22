using System;
using System.Collections.Generic;
using System.IO;
using UW.NLP.LanguageModels;

namespace UW.NLP.LanguageModelValidator
{
    public class Program
    {
        static void Main(string[] args)
        {
            ArgumentParser.Parse(args);
            if (ArgumentParser.ShowedUsage) return;

            Console.WriteLine("Splitting corpus.");
            List<List<string>> splitCorpus = SplitCorpus(ArgumentParser.CorpusPath, ArgumentParser.TrainingPercentage, ArgumentParser.ValidatePercentage, ArgumentParser.TestPercentage);

            Console.WriteLine("Training model at with {0}% of corpus.", ArgumentParser.TrainingPercentage);
            ArgumentParser.LanguageModel.Train(splitCorpus[0]);

            Console.WriteLine("Running optimizer with validation ({0}% of corpus)", ArgumentParser.ValidatePercentage);
            List<DoubleCombination> optimumCombinations = Optimizer.GetOptimumCombination(1000, ArgumentParser.LanguageModel, splitCorpus[1]);

            Console.WriteLine("These are the optimum combinations:");
            foreach (DoubleCombination combination in optimumCombinations)
            {
                Console.WriteLine(combination.ToString());
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
