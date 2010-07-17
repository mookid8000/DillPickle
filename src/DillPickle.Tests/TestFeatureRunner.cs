using System;
using NUnit.Framework;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;

namespace DillPickle.Tests
{
    [TestFixture]
    public class TestFeatureRunner : FixtureBase
    {
        FeatureRunner runner;

        public override void DoSetUp()
        {
            runner = new FeatureRunner();

            ClassWithActionSteps.Reset();
        }

        void AssertSuccess(StepResult stepResult)
        {
            Assert.AreEqual(Result.Success,
                            stepResult.Result,
                            stepResult.ErrorMessage);
        }

        string[] NoTags()
        {
            return new string[0];
        }

        [ActionSteps]
        class ClassWithActionSteps : IDisposable
        {
            public static bool Disposed { get; set; }

            public static int GivenCalls { get; set; }
            public static int WhenCalls { get; set; }
            public static int ThenCalls { get; set; }

            public static string Role { get; set; }
            public static string NewName { get; set; }
            public static string What { get; set; }
            public static string Description { get; set; }

            #region IDisposable Members

            public void Dispose()
            {
                Disposed = true;
            }

            #endregion

            public static void Reset()
            {
                Disposed = false;
                GivenCalls = WhenCalls = ThenCalls = 0;
                Role = NewName = What = Description = null;
            }

            [Given("i am logged in as $role")]
            public void GivenLoggedInAs(string role)
            {
                Role = role;
                GivenCalls++;
            }

            [When(@"i change my name to ""$newName""")]
            public void WhenChangingNameTo(string newName)
            {
                NewName = newName;
                WhenCalls++;
            }

            [Then(@"$what $description happens")]
            public void ThenSomeKindOfOutcome(string description, string what)
            {
                What = what;
                Description = description;
                ThenCalls++;
            }
        }

        [Test]
        public void CanRunFeature()
        {
            FeatureResult result = runner.Run(new Feature("feature", NoTags())
                                                  {
                                                      Scenarios =
                                                          {
                                                              new Scenario("scenario", NoTags())
                                                                  {
                                                                      Steps =
                                                                          {
                                                                              Step.Given(
                                                                                  "i am logged in as administrator"),
                                                                              Step.When(
                                                                                  @"i change my name to ""joe bananas"""),
                                                                              Step.Then("something fantastic happens")
                                                                          }
                                                                  }
                                                          }
                                                  }, new[] {typeof (ClassWithActionSteps)});

            Assert.AreEqual("feature", result.Headline);
            Assert.AreEqual("scenario", result.ScenarioResults[0].Headline);
            AssertSuccess(result.ScenarioResults[0].StepResults[0]);
            AssertSuccess(result.ScenarioResults[0].StepResults[1]);
            AssertSuccess(result.ScenarioResults[0].StepResults[2]);

            Assert.AreEqual(1, ClassWithActionSteps.GivenCalls);
            Assert.AreEqual(1, ClassWithActionSteps.WhenCalls);
            Assert.AreEqual(1, ClassWithActionSteps.ThenCalls);

            Assert.AreEqual("administrator", ClassWithActionSteps.Role);
            Assert.AreEqual("joe bananas", ClassWithActionSteps.NewName);
            Assert.AreEqual("something", ClassWithActionSteps.What);
            Assert.AreEqual("fantastic", ClassWithActionSteps.Description);

            Assert.IsTrue(ClassWithActionSteps.Disposed);
        }
    }
}