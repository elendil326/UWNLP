using System;
using System.Collections.Generic;

namespace UW.NLP.LanguageModels
{
    /// <summary>
    /// Reperesents the different settings to train and run the Language model.
    /// </summary>
    public class LanguageModelSettings
    {
        public int NGramOrder { get; set; }

        public int LogBase { get; set; }

        public string StartToken { get; set; }

        public string EndToken { get; set; }

        public string UnkToken { get; set; }

        public double UnkPercentage { get; set; }

        public string Separator { get; set; }

        public string PossibleEnd { get; set; }

        public StringComparison StringComparison { get; set; }

        public StringComparer StringComparer { get; set; }

        /// <summary>
        /// The dictionary of values of beta to use per order when using Back-off model
        /// </summary>
        public Dictionary<int, double> BackOffBetaPerOrder { get; private set; }

        /// <summary>
        /// The dictionary of values of lambda to use per order when using linear interpolation model.
        /// </summary>
        public Dictionary<int, double> LinearInterpolationLambdaPerOrder { get; private set; }

        public LanguageModelSettings()
        {
            BackOffBetaPerOrder = new Dictionary<int, double>();
            LinearInterpolationLambdaPerOrder = new Dictionary<int, double>();
        }
    }
}
