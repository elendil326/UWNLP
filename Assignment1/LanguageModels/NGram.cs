using System;

namespace UW.NLP.LanguageModels
{
    public class NGram : IEquatable<NGram>
    {
        private string[] _tokens;
        private StringComparison _stringComparison;

        public int NOrder { get; private set; }

        public string this[int i]
        {
            get { return _tokens[i]; }
            set
            {
                if (value == null) throw new ArgumentNullException("value", "The set value of the NGram cannot be null.");

                _tokens[i] = value;
            }
        }

        public NGram(int n, StringComparison stringComparison)
        {
            NOrder = n;
            _tokens = new string[n];
            _stringComparison = stringComparison;
        }

        public bool Equals(NGram other)
        {
            if (other == null || other.NOrder != NOrder)
                return false;

            for (int i = 0; i < NOrder; i++)
            {
                if (!string.Equals(other[i], this[i], _stringComparison))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = NOrder;
            for (int i = 0; i < NOrder; i++)
            {
                hashCode = hashCode ^ _tokens[i].GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NGram);
        }
    }
}
