using System;
using System.Collections.Generic;

namespace UW.NLP.LanguageModels
{
    public class LanguageModelSettings
    {
        public int NGramOrder { get; set; }

        public int LogBase { get; set; }

        public string StartToken { get; set; }

        public string EndToken { get; set; }

        public string UnkToken { get; set; }

        public string UnkWordToken { get; set; }

        public string UnkNumberToken { get; set; }

        public string UnkAlphaNumericToken { get; set; }

        public string UnkSymbolToken { get; set; }

        public string Separator { get; set; }

        public string PossibleEnd { get; set; }

        public StringComparison StringComparison { get; set; }

        public StringComparer StringComparer { get; set; }

        public Dictionary<int, double> BackOffBetaPerOrder { get; private set; }

        public Dictionary<int, double> LineaInterpolationLambdaPerOrder { get; private set; }

        public LanguageModelSettings()
        {
            BackOffBetaPerOrder = new Dictionary<int, double>();
            LineaInterpolationLambdaPerOrder = new Dictionary<int, double>();
        }
    }
}
