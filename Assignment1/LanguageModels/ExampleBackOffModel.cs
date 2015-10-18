using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UW.NLP.LanguageModels
{
    public class ExampleBackOffModel
    {
        private int _nGram = 3;
        private string _startToken = "{{*}}";
        private string _endToken = "{{END}}";
        private string _separator = " ";
        private string _possibleEnd = ".";

        private SentenceNormalizer _normalizer;
        private HashSet<string> _alphabet;
        private Dictionary<int, Dictionary<NGram, int>> _nGramCounts;

        public ExampleBackOffModel()
        {
            _normalizer = new SentenceNormalizer(_nGram, _startToken, _endToken, _separator, _possibleEnd);
            _alphabet = new HashSet<string>(StringComparer.Ordinal);

            _nGramCounts = new Dictionary<int, Dictionary<NGram, int>>();
            for (int i = 1; i < _nGram + 1; i++)
            {
                _nGramCounts[i] = new Dictionary<NGram, int>();
            }
        }

        public void TrainModel(IEnumerable<string> sentences)
        {
            if (sentences == null) throw new ArgumentNullException("sentences");

            foreach (string sentence in sentences)
            {
                string normalizedSentence = _normalizer.Normalize(sentence);
                List<string> tokens = _normalizer.Tokenize(normalizedSentence).ToList();
                PopulateNGramCounts(tokens);
            }
        }

        private void PopulateNGramCounts(List<string> tokens)
        {
            int count = 0;

            // Traverse the list of tokens once per N-Order dictionary, and shift it to the left so
            // new NGrams are added to the higher N-Order dictionaries.
            for (int nOrder = 1; nOrder < _nGram + 1; nOrder++)
            {
                Dictionary<int, NGram> lastNGramOfOrderN = new Dictionary<int, NGram>();

                // Shift the list of tokens to the left on each iteration.
                for (int i = nOrder - 1; i < tokens.Count; i++)
                {
                    _alphabet.Add(tokens[i]);
                    PopulateNGramPerDictionary(nOrder, tokens[i], count, lastNGramOfOrderN);
                    count++;
                }

                // Verify if the last token populated extra NGrams.
                PopulateNGramPerDictionary(nOrder, string.Empty, count, lastNGramOfOrderN);
            }
        }

        private void PopulateNGramPerDictionary(int firstNOrder, string token, int count, Dictionary<int, NGram> lastNGramOfOrderN)
        {
            // Traverse the dictionary of NGrams to add the token to each one.
            for (int nOrder = firstNOrder; nOrder < _nGramCounts.Count + 1; nOrder++)
            {
                // Determine if the previous NGram of order N is full and ready to be stored.
                if (count % nOrder == 0)
                {
                    // Verify this is not the first round.
                    if (lastNGramOfOrderN.ContainsKey(nOrder))
                    {
                        NGram previousBuiltNGram = lastNGramOfOrderN[nOrder];

                        // If the NGram is new, the counter dictionary won't have it, so add it.
                        if (!_nGramCounts[nOrder].ContainsKey(previousBuiltNGram))
                        {
                            _nGramCounts[nOrder][previousBuiltNGram] = 0;
                        }

                        _nGramCounts[nOrder][previousBuiltNGram]++;
                    }

                    // The last NGram is full, create a new one with for the current token.
                    lastNGramOfOrderN[nOrder] = new NGram(nOrder, StringComparison.Ordinal);
                }

                // Store the token into the NGram.
                lastNGramOfOrderN[nOrder][count % nOrder] = token;
            }
        }
    }
}
