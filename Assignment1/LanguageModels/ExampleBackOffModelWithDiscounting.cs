using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UW.NLP.LanguageModels
{
    public class ExampleBackOffModelWithDiscounting : BackOffModel
    {
        public ExampleBackOffModelWithDiscounting()
            : base()
        { }

        public ExampleBackOffModelWithDiscounting(LanguageModelSettings settings)
            : base(settings)
        { }

        public override double Probability(NGram nGram)
        {
            if (NGramCounter.GetNGramCount(nGram) > 0)
            {
                return GetP1(nGram);
            }
            else
            {
                NGram lastN_1Gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
                for (int i = 0; i < lastN_1Gram.NOrder; i++)
                {
                    lastN_1Gram[i] = nGram[i + 1];
                }

                if (NGramCounter.GetNGramCount(lastN_1Gram) > 0)
                {
                    return GetP2(nGram, lastN_1Gram);
                }
                else
                {
                    return GetP3(nGram);
                }
            }
        }

        private double GetP1(NGram nGram)
        {
            return GetPMLStar(nGram);
        }

        private double GetP2(NGram nGram)
        {

        }

        private double GetP3(NGram nGram)
        {

        }
    }
}
