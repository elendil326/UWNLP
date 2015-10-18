using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UW.NLP.LanguageModels
{
    public class NGram : IEquatable<NGram>
    {
        private string[] _tokens;
        private StringComparison _stringComparison;

        public int N { get; private set; }

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
            N = n;
            _tokens = new string[n];
            _stringComparison = stringComparison;
        }

        public bool Equals(NGram other)
        {
            if (other == null || other.N != N)
                return false;

            for (int i = 0; i < N; i++)
            {
                if (!string.Equals(other[i], this[i], _stringComparison))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = N;
            for (int i = 0; i < N; i++)
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
