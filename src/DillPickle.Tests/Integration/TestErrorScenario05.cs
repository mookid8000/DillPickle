using System;
using System.Linq;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Runner.Api;
using NUnit.Framework;

namespace DillPickle.Tests.Integration
{
    [TestFixture, Description("Fix issue https://github.com/mookid8000/DillPickle/issues/23")]
    public class TestErrorScenario05 : IntegrationTestBase
    {
        public override void DoSetUp()
        {
            Steps.WasCalled = false;
        }

        [Test]
        public void FixIt()
        {
            var results = Run(@"
Scenario: Error

	Then the list dataGridSites should at least contain the following rows:
		| Alias | Name	|
		| DK1	| West	|
		| DK2	| East	|

",
                              typeof(Steps));


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
            public static bool WasCalled;

            [Then(@"the list $listName should at least contain the following rows:")]
            public void AssertRowsInGrid(string listName, Row[] rows)
            {
                Assert.AreEqual("dataGridSites", listName);

                Assert.AreEqual(2, rows.Length);
                Assert.AreEqual("West", rows.Single(r => r.Alias == "DK1").Name);
                Assert.AreEqual("East", rows.Single(r => r.Alias == "DK2").Name);

                WasCalled = true;
            }
        }

        class Row
        {
            public string Alias { get; set; }
            public string Name { get; set; }
        }
    }
}