using System.Collections.Generic;

namespace UW.NLP.LanguageModels
{
    public interface ILanguageModel
    {
        HashSet<string> Vocabulary { get; }

        double Probability(string sentence);

        double Probability(NGram nGram);

        void Train(IEnumerable<string> sentences);
    }
}
