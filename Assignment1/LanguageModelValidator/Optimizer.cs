using System;
using System.Collections.Generic;
using System.Linq;
using UW.NLP.LanguageModels;

namespace UW.NLP.LanguageModelValidator
{
    public static class Optimizer
    {
        public static List<DoubleCombination> GetOptimumCombination(int rangeOfTrial, LanguageModel languageModel, IEnumerable<string> corpus)
        {
            if (languageModel == null) throw new ArgumentNullException("backoffModel");

            List<double> optimumBetas = new List<double>(languageModel.Settings.NGramOrder);
            Dictionary<double, List<DoubleCombination>> perplexityResults = new Dictionary<double, List<DoubleCombination>>();
            Dictionary<int, double> orderValues = languageModel is ExampleBackOffModelWithDiscounting
                                                ? languageModel.Settings.BackOffBetaPerOrder
                                                : languageModel.Settings.LinearInterpolationLambdaPerOrder;

            for (int i = 1; i < rangeOfTrial; i++)
            {
                for (int j = 1; j < languageModel.Settings.NGramOrder + 1; j++)
                {
                    // Initialize round
                    for (int k = 1; k < languageModel.Settings.NGramOrder + 1; k++)
                    {
                        orderValues[k] = i / (double)rangeOfTrial;
                    }

                    // Set each of the values to try
                    for (int k = 1; k < rangeOfTrial; k++)
                    {
                        orderValues[j] = k / (double)rangeOfTrial;
                        double sumValues = 0;
                        for (int l = 1; l < languageModel.Settings.NGramOrder + 1; l++)
                        {
                            sumValues += orderValues[l];
                        }

                        if (sumValues != 1)
                            continue;

                        languageModel.ClearCacheForDifferentSettings();
                        PerplexityCalculator calculator = new PerplexityCalculator(languageModel);

                        double perplexity = calculator.GetPerplexity(corpus);
                        if (!perplexityResults.ContainsKey(perplexity))
                        {
                            perplexityResults[perplexity] = new List<DoubleCombination>();
                        }

                        DoubleCombination configuration = new DoubleCombination(languageModel.Settings.NGramOrder);
                        for (int l = 0; l < configuration.NOrder; l++)
                        {
                            configuration[l] = orderValues[l + 1];
                        }
                        perplexityResults[perplexity].Add(configuration);
                    }
                }
            }

            return perplexityResults[perplexityResults.Keys.Min()];
        }
    }
}
