using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Runner.Api;
using NUnit.Framework;

namespace DillPickle.Tests.Integration
{
    [TestFixture]
    public class TestErrorScenario03 : IntegrationTestBase
    {
        CultureInfo currentCulture;

        public override void DoSetUp()
        {
            currentCulture = Thread.CurrentThread.CurrentCulture;
            SetCulture(CultureInfo.InvariantCulture);
        }

        public override void DoTearDown()
        {
            SetCulture(currentCulture);
        }

        void SetCulture(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        [Test]
        public void Fixit()
        {
            var results = Run(@"
Scenario: Error

    Given REG_T1_001 reports its current observable as 0.5 m",
                              typeof (Steps));


            var errorMessages = (from r in results
                                 from s in r.ScenarioResults
                                 from t in s.StepResults
                                 where t.Result != Result.Success
                                 select t.ErrorMessage)
                .ToList();

            if (errorMessages.Any())
            {
                Assert.Fail(errorMessages.JoinToString(Environment.NewLine));
            }

            Assert.IsTrue(Steps.WasCalled, "Apparently, the step was not called");
        }

        [ActionSteps]
        class Steps
        {
            public static bool WasCalled = false;

            [Given("$alias reports its current observable as $value m")]
            public void GivenStuff(string alias, double value)
            {
                Assert.AreEqual("REG_T1_001", alias);
                Assert.AreEqual(0.5, value);
                WasCalled = true;
            }
        }
    }
}