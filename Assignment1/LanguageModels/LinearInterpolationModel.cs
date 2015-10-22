namespace UW.NLP.LanguageModels
{
    /// <summary>
    /// The implementation of a Linear Interpolation language model.
    /// </summary>
    public class LinearInterpolationModel : LanguageModel
    {
        /// <summary>
        /// Creates a new instance of the class with default values.
        /// </summary>
        public LinearInterpolationModel()
            : base()
        {
            Settings.LinearInterpolationLambdaPerOrder[1] = 0.30;
            Settings.LinearInterpolationLambdaPerOrder[2] = 0.20;
            Settings.LinearInterpolationLambdaPerOrder[3] = 0.50;
        }

        public LinearInterpolationModel(LanguageModelSettings settings)
            : base(settings)
        { }

        /// <summary>
        /// Gets the probability of an Ngram.
        /// </summary>
        /// <param name="nGram">The NGram to calculate.</param>
        /// <returns>The probability of the NGram.</returns>
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

                // If is Infinity or undefined, take it as zero.
                probabilty += double.IsInfinity(pml) || double.IsNaN(pml)
                    ? 0
                    : Settings.LinearInterpolationLambdaPerOrder[i + 1] * pml;
            }

            return probabilty;
        }

        public override void ClearCacheForDifferentSettings()
        { }
    }
}
