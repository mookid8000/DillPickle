using System;
using System.Collections.Generic;
using NUnit.Framework;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using Rhino.Mocks;

namespace DillPickle.Tests
{
    [TestFixture]
    public class TestRunner : FixtureBase
    {
        Runner runner;
        IFeatureRunner featureRunner;

        public override void DoSetUp()
        {
            featureRunner = Mock<IFeatureRunner>();
            runner = new Runner(featureRunner);
        }

        [Test]
        public void InvokesFeatureRunner()
        {
            var feature1 = new Feature("ey!", new string[0]);
            var feature2 = new Feature("ey!", new string[0]);
            var availableTypes = new Type[0];
            var result1 = new FeatureResult();
            var result2 = new FeatureResult();

            featureRunner.Stub(r => r.Run(feature1, availableTypes)).Return(result1);
            featureRunner.Stub(r => r.Run(feature2, availableTypes)).Return(result2);

            List<FeatureResult> results = runner.Run(new[] {feature1, feature2}, availableTypes);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(result1, results[0]);
            Assert.AreEqual(result2, results[1]);
        }
    }
}