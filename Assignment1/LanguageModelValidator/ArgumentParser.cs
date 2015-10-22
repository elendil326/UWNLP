using System;
using UW.NLP.LanguageModels;

namespace UW.NLP.LanguageModelValidator
{
    public static class ArgumentParser
    {
        public static string CorpusPath { get; private set; }

        public static LanguageModel LanguageModel { get; private set; }

        public static double TrainingPercentage { get; private set; }

        public static double ValidatePercentage { get; private set; }

        public static double TestPercentage { get; private set; }

        public static bool ShowedUsage { get; private set; }

        public static bool Optimize { get; private set; }

        public static double OptimzeValue { get; private set; }

        public static void Parse(string[] args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Length < 2)
            {
                ShowUsage();
                return;
            }

            CorpusPath = args[0];
            switch (args[1].ToUpperInvariant())
            {
                case "LINEARINTERPOLATION":
                    LanguageModel = new LinearInterpolationModel();
                    break;
                case "BACKOFF":
                    LanguageModel = new ExampleBackOffModelWithDiscounting();
                    break;
                default:
                    ShowUsage();
                    break;
            }

            if (args.Length > 2)
            {
                if (args.Length < 5)
                    ShowUsage();

                TrainingPercentage = double.Parse(args[2]);
                ValidatePercentage = double.Parse(args[3]);
                TestPercentage = double.Parse(args[4]);
            }
            else
            {
                TrainingPercentage = 80;
                ValidatePercentage = 10;
                TestPercentage = 10;
            }

            if (args.Length > 6)
            {
                if (args.Length < 7)
                    ShowUsage();
                if (string.Equals(args[5], "Optimize", StringComparison.OrdinalIgnoreCase))
                {
                    Optimize = true;
                    OptimzeValue = 1000;
                }
            }
        }

        public static void ShowUsage()
        {
            Console.WriteLine("LanguageModelValidator.exe");
            Console.WriteLine();
            Console.WriteLine("Usages:");
            Console.WriteLine();
            Console.WriteLine(@"LanguageModelValidator.exe <CorpusPath> <LanguageModel> 
\t( <training percent> <validate percent> <test percent> Optimze <Optimizevalue>)");
            Console.WriteLine();
            Console.WriteLine("/tLanguageModel:\tBackoff, LinearInterpolation");
            Console.WriteLine("/ttraining,validate,test percent:\tThe percent of the corporus to use for each propose. Default as 80% 10% 10%");
            Console.WriteLine("/tOptimizeValue:\tThe value of decimals to brute force an optimal combination.");
            Console.WriteLine();
            Console.WriteLine("Example");
            Console.WriteLine();
            Console.WriteLine("LanguageModelValidator.exe LinearInterpolation 80 10 10 Optimize 1000");
            ShowedUsage = true;
        }
    }
}
