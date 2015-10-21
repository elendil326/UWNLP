using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UW.NLP.LanguageModels.UnitTests
{
    [TestClass]
    public class ExampleBackOffModelWithDiscountingTests
    {
        [TestMethod]
        public void Probability_FixedBigramTogenerateAllPossibleTrigrams_SumIsCloseToOne()
        {
            LanguageModelBaseTests<ExampleBackOffModelWithDiscounting>.Probability_FixedBigramTogenerateAllPossibleTrigrams
                (
                    (sumOfAllProbabilities) => Assert.AreEqual(1, Math.Round(sumOfAllProbabilities))
                );
        }

        [TestMethod]
        public void Probability_AllPossibleTrigrams_SumIsCloseToOneEachOne()
        {
            LanguageModelBaseTests<ExampleBackOffModelWithDiscounting>.Probability_AllPossibleTrigrams
                (
                    (sumOfAllProbabilities) => Assert.AreEqual(1, Math.Round(sumOfAllProbabilities))
                );
        }

        [TestMethod]
        [TestCategory("Load")]
        public void Probability_RandomSentencesFromVocabulary_ProbabilityLessThanOne()
        {
            LanguageModelBaseTests<ExampleBackOffModelWithDiscounting>.Probability_RandomSentencesFromVocabulary
                (
                    int.MaxValue >> 12,
                    (sumOfAllProbabilities) => Assert.IsFalse(sumOfAllProbabilities > 1)
                );
        }

        [TestMethod]
        [TestCategory("Load")]
        public void TrainModel_RealCorpora_NoMemoryException()
        {
            LanguageModelBaseTests<ExampleBackOffModelWithDiscounting>.TrainModel_RealCorpora_NoMemoryException();
        }
    }
}
