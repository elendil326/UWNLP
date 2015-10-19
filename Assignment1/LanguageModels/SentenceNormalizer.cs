using System;
using System.Collections.Generic;
using System.Text;

namespace UW.NLP.LanguageModels
{
    public class SentenceNormalizer
    {
        public int N { get; private set; }

        public string StartToken { get; private set; }

        public string EndToken { get; private set; }

        public string Separator { get; private set; }

        public string PossibleEndToken { get; private set; }

        public SentenceNormalizer(int n, string startToken, string endToken, string separator, string possibleEndToken)
        {
            if (n < 1) throw new ArgumentOutOfRangeException("n", "N for the N-gram must be greater than one.");
            if (string.IsNullOrEmpty(startToken)) throw new ArgumentNullException("startToken");
            if (string.IsNullOrEmpty(endToken)) throw new ArgumentNullException("endToken");
            if (string.IsNullOrEmpty(separator)) throw new ArgumentNullException("separator");
            if (string.IsNullOrEmpty(possibleEndToken)) throw new ArgumentNullException("possibleEndToken");

            N = n;
            StartToken = startToken;
            EndToken = endToken;
            Separator = separator;
        }

        public string Normalize(string sentence)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            StringBuilder normalizedSentence = new StringBuilder(GetNormalizedLength(sentence.Length));

            normalizedSentence = AddStartTokens(normalizedSentence);

            normalizedSentence = AddEndToken(sentence, normalizedSentence);

            return normalizedSentence.ToString();
        }

        public IEnumerable<string> Tokenize(string sentence)
        {
            if (sentence == null) throw new ArgumentNullException("sentence");

            return sentence.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        private int GetNormalizedLength(int originalLength)
        {
            return originalLength + ((N - 1) * (StartToken.Length + Separator.Length)) + Separator.Length + EndToken.Length;
        }

        private StringBuilder AddStartTokens(StringBuilder normalizedSentence)
        {
            for (int i = 0; i < N - 1; i++)
            {
                normalizedSentence.Insert(0, Separator);
                normalizedSentence.Insert(0, StartToken);
            }

            return normalizedSentence;
        }

        private StringBuilder AddEndToken(string sentence, StringBuilder normalizedSentence)
        {
            if (sentence.EndsWith(PossibleEndToken))
            {
                normalizedSentence.Remove(normalizedSentence.Length - PossibleEndToken.Length, PossibleEndToken.Length);
            }
            normalizedSentence.Append(Separator);
            normalizedSentence.Append(EndToken);

            return normalizedSentence;
        }
    }
}
