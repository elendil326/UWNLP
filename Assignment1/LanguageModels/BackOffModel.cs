using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UW.NLP.LanguageModels
{
    public abstract class BackOffModel : LanguageModel
    {
        public BackOffModel()
            : base()
        {
            Settings.BackOffBetaPerOrder[1] = 0.75;
            Settings.BackOffBetaPerOrder[2] = 0.5;
            Settings.BackOffBetaPerOrder[3] = 0.5;
        }

        public BackOffModel(LanguageModelSettings settings)
            : base(settings)
        { }

        internal HashSet<string> GetListOfWordsForInexistentNGram(NGram N_1gram)
        {
            NGram possibleNGram = new NGram(N_1gram.NOrder + 1, Settings.StringComparison);
            for (int i = 0; i < N_1gram.NOrder; i++)
            {
                possibleNGram[i] = N_1gram[i];
            }

            HashSet<string> words = new HashSet<string>(Settings.StringComparer);
            foreach (string word in Vocabulary)
            {
                possibleNGram[N_1gram.NOrder] = word;
                if (NGramCounter.GetNGramCount(possibleNGram) == 0)
                {
                    words.Add(word);
                }
            }

            return words;
        }

        internal HashSet<string> GetListOfWordsForExistentNGram(NGram N_1gram)
        {
            NGram possibleNGram = new NGram(N_1gram.NOrder + 1, Settings.StringComparison);
            for (int i = 0; i < N_1gram.NOrder; i++)
            {
                possibleNGram[i] = N_1gram[i];
            }

            HashSet<string> words = new HashSet<string>(Settings.StringComparer);
            foreach (string word in Vocabulary)
            {
                possibleNGram[N_1gram.NOrder] = word;
                if (NGramCounter.GetNGramCount(possibleNGram) > 0)
                {
                    words.Add(word);
                }
            }

            return words;
        }

        internal double GetPMLWithDiscount(NGram nGram)
        {
            double numerator = NGramCounter.GetNGramCount(nGram) - Settings.BackOffBetaPerOrder[nGram.NOrder];
            double denominator = 0;
            if (nGram.NOrder == 1)
            {
                denominator = NGramCounter.TotalWords;
            }
            else
            {
                NGram N_1gram = new NGram(nGram.NOrder - 1, Settings.StringComparison);
                for (int i = 0; i < N_1gram.NOrder; i++)
                {
                    N_1gram[i] = nGram[i];
                }
                denominator = NGramCounter.GetNGramCount(N_1gram);
            }

            return numerator / denominator;
        }

        internal double Alpha(NGram n_1Gram)
        {
            double probability = 1;

            foreach (string word in  GetListOfWordsForExistentNGram(n_1Gram))
            {
                NGram possibleNGram = new NGram(n_1Gram.NOrder + 1, Settings.StringComparison);
                for (int i = 0; i < n_1Gram.NOrder; i++)
                {
                    possibleNGram[i] = n_1Gram[i];
                }
                possibleNGram[possibleNGram.NOrder - 1] = word;

                probability -= GetPMLWithDiscount(possibleNGram);
            }

            return probability;
        }
    }
}
