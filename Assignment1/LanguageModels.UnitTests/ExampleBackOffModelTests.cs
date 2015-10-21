using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UW.NLP.LanguageModels.UnitTests
{
    [TestClass]
    public class ExampleBackOffModelTests
    {
        [TestMethod]
        public void Probability_FixedBigramTogenerateAllPossibleTrigrams_SumIsCloseToTwo()
        {
            LanguageModelBaseTests<ExampleBackOffModel>.Probability_FixedBigramTogenerateAllPossibleTrigrams
                (
                    (sumOfAllProbabilities) => Assert.AreEqual(2, Math.Round(sumOfAllProbabilities))
                );
        }

        [TestMethod]
        public void Probability_AllPossibleTrigrams_SumIsNotOne()
        {
            LanguageModelBaseTests<ExampleBackOffModel>.Probability_AllPossibleTrigrams
                (
                    (sumOfAllProbabilities) => Assert.AreNotEqual(1, Math.Round(sumOfAllProbabilities))
                );
        }

        [TestMethod]
        public void Probability_RandomSentencesFromVocabulary_ProbabilityGreaterThanOne()
        {
            LanguageModelBaseTests<ExampleBackOffModel>.Probability_RandomSentencesFromVocabulary
                (
                    1000,
                    (sumOfAllProbabilities) => Assert.IsTrue(sumOfAllProbabilities > 1)
                );
        }

        [TestMethod]
        [TestCategory("Load")]
        public void TrainModel_RealCorpora_NoMemoryException()
        {
            LanguageModelBaseTests<ExampleBackOffModel>.TrainModel_RealCorpora_NoMemoryException();
        }
    }
}
