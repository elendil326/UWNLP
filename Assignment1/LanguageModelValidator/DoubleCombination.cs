using System;
using System.Text;

namespace UW.NLP.LanguageModelValidator
{
    public class DoubleCombination : IEquatable<DoubleCombination>
    {
        private double[] _doubles;

        public int NOrder { get; private set; }

        public double this[int i]
        {
            get { return _doubles[i]; }
            set { _doubles[i] = value; }
        }

        public DoubleCombination(int n)
        {
            NOrder = n;
            _doubles = new double[n];
        }

        public bool Equals(DoubleCombination other)
        {
            if (other == null || other.NOrder != NOrder)
                return false;

            for (int i = 0; i < NOrder; i++)
            {
                if (other[i] != this[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = NOrder;
            for (int i = 0; i < NOrder; i++)
            {
                hashCode = hashCode ^ _doubles[i].GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DoubleCombination);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(NOrder * 2);
            for (int i = 0; i < _doubles.Length; i++)
            {
                if (sb.Length > 0)
                {
                    sb.AppendFormat(",<<{0}>>", _doubles[i]);
                }
                else
                {
                    sb.AppendFormat("<<{0}>>", _doubles[i]);
                }
            }

            return sb.ToString();
        }
    }
}
