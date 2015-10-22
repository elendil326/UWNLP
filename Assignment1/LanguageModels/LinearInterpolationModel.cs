using System;

namespace UW.NLP.LanguageModels
{
    public class LinearInterpolationModel : LanguageModel
    {
        public LinearInterpolationModel()
            : base()
        {
            Settings.LineaInterpolationLambdaPerOrder[1] = 0.30;
            Settings.LineaInterpolationLambdaPerOrder[2] = 0.20;
            Settings.LineaInterpolationLambdaPerOrder[3] = 0.50;
        }

        public LinearInterpolationModel(LanguageModelSettings settings)
            : base(settings)
        { }

        public override double Probability(NGram nGram)
        {
            double probabilty = 0;

            for (int i = 0; i < nGram.NOrder; i++)
            {
                NGram n_IGram = new NGram(nGram.NOrder - i, Settings.StringComparison);
                for (int j = 0; j < n_IGram.NOrder; j++)
                {
                    n_IGram[j] = nGram[j + i];
                }
                double pml = GetPML(n_IGram);
                probabilty += double.IsInfinity(pml) || double.IsNaN(pml)
                    ? 0
                    : Settings.LineaInterpolationLambdaPerOrder[i + 1] * pml;
            }

            return probabilty;
        }

        public override void ClearCacheForDifferentSettings()
        {
            throw new NotSupportedException();
        }
    }
}
