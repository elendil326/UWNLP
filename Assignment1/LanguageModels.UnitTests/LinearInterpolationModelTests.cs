using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UW.NLP.LanguageModels.UnitTests
{
    [TestClass]
    public class LinearInterpolationModelTests
    {
        [TestMethod]
        public void Probability_FixedBigramTogenerateAllPossibleTrigrams_SumIsCloseToOne()
        {
            LanguageModelBaseTests<LinearInterpolationModel>.Probability_FixedBigramTogenerateAllPossibleTrigrams
                (
                    (sumOfAllProbabilities) => Assert.AreEqual(1, Math.Round(sumOfAllProbabilities))
                );
        }

        [TestMethod]
        public void Probability_AllPossibleTrigrams_SumIsCloseToOneEachOne()
        {
            LanguageModelBaseTests<LinearInterpolationModel>.Probability_AllPossibleTrigrams
                (
                    (sumOfAllProbabilities) => Assert.AreEqual(1, Math.Round(sumOfAllProbabilities))
                );
        }

        [TestMethod]
        [TestCategory("Load")]
        public void Probability_RandomSentencesFromVocabulary_ProbabilityLessThanOne()
        {
            LanguageModelBaseTests<LinearInterpolationModel>.Probability_RandomSentencesFromVocabulary
                (
                    int.MaxValue >> 11,
                    (sumOfAllProbabilities) => Assert.IsFalse(sumOfAllProbabilities > 1)
                );
        }

        [TestMethod]
        [TestCategory("Load")]
        public void TrainModel_RealCorpora_NoMemoryException()
        {
            LanguageModelBaseTests<LinearInterpolationModel>.TrainModel_RealCorpora_NoMemoryException();
        }
    }
}
