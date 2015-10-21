using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UW.NLP.LanguageModels
{
    public class NGramCounter
    {
        private Dictionary<int, Dictionary<NGram, int>> _nGramCountDictionaries;
        private LanguageModelSettings _settings;
        private HashSet<string> _seenWords;

        public int MaxNOrder { get; private set; }

        public HashSet<string> Vocabulary { get; private set; }

        public int TotalWords { get; private set; }

        public NGramCounter(LanguageModelSettings settings)
        {
            MaxNOrder = settings.NGramOrder;
            _settings = settings;
            _seenWords = new HashSet<string>(_settings.StringComparer);
            Vocabulary = new HashSet<string>(_settings.StringComparer);

            _nGramCountDictionaries = new Dictionary<int, Dictionary<NGram, int>>();
            for (int i = 1; i < MaxNOrder + 1; i++)
            {
                _nGramCountDictionaries[i] = new Dictionary<NGram, int>();
            }
        }

        public void PopulateNGramCounts(List<string> tokens)
        {
            // Add start tokens to the count.
            TotalWords += MaxNOrder - 1;

            // Populate NGrams of the START token. The NGram of higher order N doesn't have this count.
            for (int nOrder = 1; nOrder < MaxNOrder; nOrder++)
            {
                NGram currentNGram = new NGram(nOrder, _settings.StringComparison);
                for (int i = 0; i < currentNGram.NOrder; i++)
                {
                    currentNGram[i] = tokens[i];
                }

                if (!_nGramCountDictionaries[nOrder].ContainsKey(currentNGram))
                {
                    _nGramCountDictionaries[nOrder][currentNGram] = 0;
                }

                _nGramCountDictionaries[nOrder][currentNGram] += MaxNOrder - nOrder;
            }

            // Add end token first to avoid it being replaced with an Unk token.
            _seenWords.Add(_settings.EndToken);

            // Populate the NGrams starting from the non-START token.
            for (int currentTokenIndex = MaxNOrder - 1; currentTokenIndex < tokens.Count; currentTokenIndex++)
            {
                // Make the first occurrence of a word an Unkown word.
                if (!_seenWords.Contains(tokens[currentTokenIndex]))
                {
                    _seenWords.Add(tokens[currentTokenIndex]);
                    tokens[currentTokenIndex] = GetUnkSymbol(tokens[currentTokenIndex]);
                }

                Vocabulary.Add(tokens[currentTokenIndex]);
                TotalWords++;

                // Add the new NGram to each of the dictionaries.
                for (int nOrder = 1; nOrder < _nGramCountDictionaries.Count + 1; nOrder++)
                {
                    NGram currentNGram = new NGram(nOrder, _settings.StringComparison);

                    // Populate the NGram with the previous words according to the NOrder.
                    for (int nGramTokenIndex = 0; nGramTokenIndex < currentNGram.NOrder; nGramTokenIndex++)
                    {
                        int previousTokenIndex = currentTokenIndex - currentNGram.NOrder + nGramTokenIndex + 1;
                        currentNGram[nGramTokenIndex] = tokens[previousTokenIndex];
                    }

                    // If the NGram is new, the counter dictionary won't have it, so add it.
                    if (!_nGramCountDictionaries[nOrder].ContainsKey(currentNGram))
                    {
                        _nGramCountDictionaries[nOrder][currentNGram] = 0;
                    }

                    // Increase the count of this nGram.
                    _nGramCountDictionaries[nOrder][currentNGram]++;
                }
            }
        }

        public int GetNGramCount(NGram nGram)
        {
            if (nGram == null) throw new ArgumentNullException("ngram");

            return (!_nGramCountDictionaries.ContainsKey(nGram.NOrder) || !_nGramCountDictionaries[nGram.NOrder].ContainsKey(nGram))
                ? 0
                : _nGramCountDictionaries[nGram.NOrder][nGram]; 
        }

        public string GetUnkSymbol(string word)
        {
            if (word.All(char.IsDigit))
                return _settings.UnkNumberToken;

            if (word.All(char.IsLetter))
                return _settings.UnkWordToken;

            if (word.All(char.IsLetterOrDigit))
                return _settings.UnkAlphaNumericToken;

            if (word.All(char.IsSymbol))
                return _settings.UnkSymbolToken;

            return _settings.UnkToken;
        }
    }
}
