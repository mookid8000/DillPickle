using System;
using System.Linq;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using NUnit.Framework;

namespace DillPickle.Tests.Integration
{
    [TestFixture, Description("Fix issue https://github.com/mookid8000/DillPickle/issues/24")]
    public class TestErrorScenario06 : IntegrationTestBase
    {
        public override void DoSetUp()
        {
            Steps.WasCalled = false;
        }

        [Test]
        public void FixIt()
        {
            var results =
                Run(
                    @"@performance
Feature: Verify and stress the performance of the PowerHub distributor.

    Background: 
        Given that the test site exists
        And the test site is set up to do cross-market optimization over 4 days
        And the test site is set up to do distribution plan optimization over 1.5 days

    Scenario: Verify performance with the minimum number of LUs that the system should be capable of handling
        Given that the test site contains 16 local units
        When the system runs for 10 minutes
        Then the agent has done work for at most 50 % of the time
        And preprocessing has taken place every 5 s +/- 1 s
        And plan execution has taken place every 5 s +/- 1 s
        And crossmarket optimization has taken place every 5 m +/- 10 s
        And distribution optimization has taken place every 5 m +/- 10 s

    Scenario: Verify performance with a greater number of LUs
        Given that the test site contains 30 local units
        When the system runs for 10 minutes
        Then the agent has done work for at most 50 % of the time
        And preprocessing has taken place every 5 s +/- 1 s
        And plan execution has taken place every 5 s +/- 1 s
        And crossmarket optimization has taken place every 5 m +/- 10 s
        And distribution optimization has taken place every 5 m +/- 10 s

    @current 
    Scenario: Verify performance with a greater number of LUs
        Given that the test site contains 50 local units
        When the system runs for 10 minutes
        Then the agent has done work for at most 50 % of the time
        And preprocessing has taken place every 5 s +/- 1 s
        And plan execution has taken place every 5 s +/- 1 s
        And crossmarket optimization has taken place every 5 m +/- 10 s
        And distribution optimization has taken place every 5 m +/- 10 s",
                    typeof (Steps),
                    new RunnerOptions
                        {
                            Filter = new TagFilter(new[] {"current"}, new string[0])
                        });


            var errorMessages = (from r in results
                                 from s in r.ScenarioResults
                                 from t in s.StepResults
                                 where t.Result == Result.Failed
                                 select t.ErrorMessage)
                .ToList();

            if (errorMessages.Any())
            {
                Assert.Fail(errorMessages.JoinToString(Environment.NewLine));
            }

            Assert.IsTrue(Steps.WasCalled, "Step method was not called as expected");
        }

        [ActionSteps]
        class Steps
        {
            public static bool WasCalled;
            
            [Given("that the test site contains $number local units")]
            public void RecordCall(int number)
            {
                Assert.IsFalse(WasCalled, "Step method seems to have been called before!");
                
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