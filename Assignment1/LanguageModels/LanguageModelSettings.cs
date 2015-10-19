using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UW.NLP.LanguageModels
{
    public class LanguageModelSettings
    {
        public int NGramOrder { get; set; }

        public int LogBase { get; set; }

        public string StartToken { get; set; }

        public string EndToken { get; set; }

        public string Separator { get; set; }

        public string PossibleEnd { get; set; }

        public StringComparison StringComparison { get; set; }

        public StringComparer StringComparer { get; set; }

        public double BackOffBeta { get; set; }
    }
}
