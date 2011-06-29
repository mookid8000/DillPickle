using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner.Api;
using NUnit.Framework;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Runner;

namespace DillPickle.Tests.Runner
{
    [TestFixture]
    public class TestFeatureRunner : FixtureBase
    {
        FeatureRunner runner;

        public override void DoSetUp()
        {
            runner = new FeatureRunner(new TrivialObjectActivator(), new TrivialPropertySetter());

            ClassWithActionSteps.Reset();
        }

        [Test]
        public void StopsExecutingScenarioIfStepFailsAndStopOnErrorIsSet()
        {
            RemembersStepsExecuted.Reset();

            var feature = new Feature("has an error", NoTags())
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("has an error", NoTags())
                                              {
                                                  Steps=
                                                      {
                                                          Step.Given("step1"),
                                                          Step.Given("step2"),
                                                          Step.Given("step3"),
                                                      }
                                              }
                                      }
                              };

            runner.Run(feature, new[] {typeof (RemembersStepsExecuted)}, new RunnerOptions {SuccessRequired = true});

            Assert.IsFalse(RemembersStepsExecuted.StepNames.Contains("step3"));
        }

        [Test]
        public void StopsExecutingScenarioIfStepIsPendingAndStopOnErrorIsSet()
        {
            RemembersStepsExecuted.Reset();

            var feature = new Feature("has an error", NoTags())
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("has an error", NoTags())
                                              {
                                                  Steps=
                                                      {
                                                          Step.Given("step1"),
                                                          Step.Given("step2 does not exist"),
                                                          Step.Given("step3"),
                                                      }
                                              }
                                      }
                              };

            runner.Run(feature, new[] {typeof (RemembersStepsExecuted)}, new RunnerOptions {SuccessRequired = true});

            Assert.IsFalse(RemembersStepsExecuted.StepNames.Contains("step3"));
        }

        [Test]
        public void DoesNotStopExecutingScenarioIfStepFailsAndStopOnErrorIsNotSet()
        {
            RemembersStepsExecuted.Reset();

            var feature = new Feature("has an error", NoTags())
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("has an error", NoTags())
                                              {
                                                  Steps=
                                                      {
                                                          Step.Given("step1"),
                                                          Step.Given("step2"),
                                                          Step.Given("step3"),
                                                      }
                                              }
                                      }
                              };

            runner.Run(feature, new[] {typeof (RemembersStepsExecuted)}, new RunnerOptions {SuccessRequired = false});

            Assert.IsTrue(RemembersStepsExecuted.StepNames.Contains("step3"));
        }

        [ActionSteps]
        class RemembersStepsExecuted
        {
            public static List<string> StepNames = new List<string>();

            public static void Reset()
            {
                StepNames.Clear();
            }

            [Given("step1")]
            public void Step1()
            {
                StepNames.Add("step1");
            }
            
            [Given("step2")]
            public void Step2()
            {
                StepNames.Add("step2");
                throw new CryptographicUnexpectedOperationException("as if");
            }
            
            [Given("step3")]
            public void Step3()
            {
                StepNames.Add("step3");
            }
        }

        [Test]
        public void IncludesTagsLikeExpected()
        {
            JustRemembersIfItHasBeenRun.Reset();

            var feature = new Feature("name", NoTags())
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("scenario1", new[] {"tag"})
                                              {
                                                  Steps = {Step.Given("should be run")}
                                              },
                                          new ExecutableScenario("scenario2", NoTags())
                                              {
                                                  Steps = {Step.Given("should not be run")}
                                              }
                                      }
                              };

            runner.Run(feature, new[] {typeof (JustRemembersIfItHasBeenRun)}, OptionsWithTag());

            Assert.IsTrue(JustRemembersIfItHasBeenRun.ShouldRunWasGood);
            Assert.IsTrue(JustRemembersIfItHasBeenRun.ShouldNotRunWasGood);
        }

        RunnerOptions OptionsWithTag()
        {
            return new RunnerOptions
                       {
                           Filter = new TagFilter(new[] {"tag"}, NoTags()),
                           DruRun = false,
                       };
        }

        [ActionSteps]
        class JustRemembersIfItHasBeenRun
        {
            public static bool ShouldRunWasGood { get; set; }
            public static bool ShouldNotRunWasGood { get; set; }

            public static void Reset()
            {
                ShouldRunWasGood = false;
                ShouldNotRunWasGood = true;
            }

            [Given("should be run")]
            public void GivenSomething()
            {
                ShouldRunWasGood = true;
            }

            [Given("should not be run")]
            public void GivenSomethingElse()
            {
                ShouldNotRunWasGood = false;
            }
        }

        [Test]
        public void HooksAreCalledAsExpected()
        {
            var feature = new Feature("feature", NoTags())
                              {
                                  BackgroundSteps =
                                      {
                                          Step.Given("some background"),
                                      },
                                  Scenarios =
                                      {
                                          new ExecutableScenario("scenario", NoTags())
                                              {
                                                  Steps =
                                                      {
                                                          Step.Given("something"),
                                                          Step.When("something"),
                                                          Step.Then("something"),
                                                      }
                                              },
                                          new ExecutableScenario("scenario", NoTags())
                                              {
                                                  Steps =
                                                      {
                                                          Step.Given("something"),
                                                          Step.Given("something"),
                                                          Step.When("something"),
                                                          Step.When("something"),
                                                          Step.Then("something"),
                                                          Step.Then("something"),
                                                      }
                                              }
                                      }
                              };

            runner.Run(feature, new[] { typeof(RecordsTheOrderOfThings) }, DefaultOptions());

            var expectedCalls = new[]
                                    {
                                        What.Ctor,
                                        What.BeforeFeature,

                                        What.BeforeScenario,

                                        What.BeforeStep,
                                        What.GivenSomeBackground,
                                        What.AfterStep,
                                        
                                        What.BeforeStep,
                                        What.GivenSomething,
                                        What.AfterStep,
                                        What.BeforeStep,
                                        What.WhenSomething,
                                        What.AfterStep,
                                        What.BeforeStep,
                                        What.ThenSomething,
                                        What.AfterStep,
                                        
                                        What.AfterScenario,



                                        What.BeforeScenario,
                                        
                                        What.BeforeStep,
                                        What.GivenSomeBackground,
                                        What.AfterStep,

                                        What.BeforeStep,
                                        What.GivenSomething,
                                        What.AfterStep,
                                        What.BeforeStep,
                                        What.GivenSomething,
                                        What.AfterStep,
                                        What.BeforeStep,
                                        What.WhenSomething,
                                        What.AfterStep,
                                        What.BeforeStep,
                                        What.WhenSomething,
                                        What.AfterStep,
                                        What.BeforeStep,
                                        What.ThenSomething,
                                        What.AfterStep,
                                        What.BeforeStep,
                                        What.ThenSomething,
                                        What.AfterStep,
                                        
                                        What.AfterScenario,

                                        What.AfterFeature,
                                        What.Disposed,
                                    };

            Assert.IsTrue(RecordsTheOrderOfThings.WhatHappened.SequenceEqual(expectedCalls),
                          @"
expected:
    {0}
but was:
    {1}
", expectedCalls.JoinToString(", "),
                          RecordsTheOrderOfThings.WhatHappened.JoinToString(", "));
        }

        RunnerOptions DefaultOptions()
        {
            return new RunnerOptions
                       {
                           Filter = TagFilter.Empty(),
                           DruRun = false,
                       };
        }

        public enum What
        {
            Ctor, Disposed,
            GivenSomething, WhenSomething, ThenSomething,
            GivenSomeBackground,
            BeforeFeature, AfterFeature,
            BeforeScenario, AfterScenario,
            BeforeStep, AfterStep
        }

        [ActionSteps]
        class RecordsTheOrderOfThings : IDisposable
        {
            public RecordsTheOrderOfThings()
            {
                WhatHappened = new List<What> {What.Ctor};
            }

            public static List<What> WhatHappened { get; private set; }
            
            [BeforeFeature]
            public void BeforeFeature()
            {
                WhatHappened.Add(What.BeforeFeature);
            }

            [AfterFeature]
            public void AfterFeature()
            {
                WhatHappened.Add(What.AfterFeature);
            }

            [BeforeScenario]
            public void BeforeScenario()
            {
                WhatHappened.Add(What.BeforeScenario);
            }

            [AfterScenario]
            public void AfterScenario()
            {
                WhatHappened.Add(What.AfterScenario);
            }

            [BeforeStep]
            public void BeforeStep()
            {
                WhatHappened.Add(What.BeforeStep);
            }

            [AfterStep]
            public void AfterStep()
            {
                WhatHappened.Add(What.AfterStep);
            }

            [Given("something")]
            public void Given()
            {
                WhatHappened.Add(What.GivenSomething);
            }

            [When("something")]
            public void When()
            {
                WhatHappened.Add(What.WhenSomething);
            }

            [Then("something")]
            public void Then()
            {
                WhatHappened.Add(What.ThenSomething);
            }

            [Given("some background")]
            public void GivenBackground()
            {
                WhatHappened.Add(What.GivenSomeBackground);
            }

            public void Dispose()
            {
                WhatHappened.Add(What.Disposed);
            }
        }

        [Test]
        public void CanRunFeatureWithBackgroundSteps()
        {
            var feature = new Feature("feature", NoTags());
            feature.BackgroundSteps.Add(Step.Given("something"));
            feature.BackgroundSteps.Add(Step.Given("something else"));
            feature.Scenarios.Add(new ExecutableScenario("scenario", NoTags())
                                      {
                                          Steps=
                                              {
                                                  Step.Given("the other stuff was called before"),
                                                  Step.When("the other stuff was called before"),
                                                  Step.Then("the other stuff was called before"),
                                              }
                                      });

            runner.Run(feature, new[] { typeof(HasSomeSteps) }, DefaultOptions());

            Assert.IsTrue(HasSomeSteps.GivenCalled);
            Assert.IsTrue(HasSomeSteps.WhenCalled);
            Assert.IsTrue(HasSomeSteps.ThenCalled);
        }

        [ActionSteps]
        class HasSomeSteps
        {
            public static bool SomethingCalled{ get; set;}
            public static bool SomethingElseCalled { get; set; }
            public static bool GivenCalled { get; set; }
            public static bool WhenCalled { get; set; }
            public static bool ThenCalled { get; set; }

            [Given("something")]
            public void GivenSomething()
            {
                SomethingCalled = true;
            }

            [Given("something else")]
            public void GivenSomethingElse()
            {
                SomethingElseCalled = true;
            }

            [Given("the other stuff was called before")]
            public void GivenTheOtherStuffWasCalledBefore()
            {
                GivenCalled = true;
                Assert.IsTrue(SomethingCalled);
                Assert.IsTrue(SomethingElseCalled);
            }

            [When("the other stuff was called before")]
            public void WhenTheOtherStuffWasCalledBefore()
            {
                WhenCalled = true;
                Assert.IsTrue(SomethingCalled);
                Assert.IsTrue(SomethingElseCalled);
            }

            [Then("the other stuff was called before")]
            public void ThenTheOtherStuffWasCalledBefore()
            {
                ThenCalled = true;
                Assert.IsTrue(SomethingCalled);
                Assert.IsTrue(SomethingElseCalled);
            }
        }

        [Test]
        public void CanRunFeatureWithScenarioOutline()
        {
            var feature = new Feature("feature", NoTags());
            feature.Scenarios.Add(new ScenarioOutline("outline", NoTags())
                                      {
                                          Steps =
                                              {
                                                  Step.Given("I have <have> cucumbers"),
                                                  Step.When("I eat <eat> cucumbers"),
                                                  Step.Then("I have <left> cucumbers left"),
                                              },
                                          Examples =
                                              {
                                                  new Dictionary<string, string>
                                                      {
                                                          {"have", "30"},
                                                          {"eat", "20"},
                                                          {"left", "10"},
                                                      },
                                                  new Dictionary<string, string>
                                                      {
                                                          {"have", "15"},
                                                          {"eat", "2"},
                                                          {"left", "13"},
                                                      },
                                              }
                                      });

            runner.Run(feature, new[] { typeof(Cucumbulator) }, DefaultOptions());

            Assert.AreEqual(2, Cucumbulator.Calls.Count);
            
            var dict1 = Cucumbulator.Calls[0];
            Assert.AreEqual(30, dict1["have"]);
            Assert.AreEqual(20, dict1["eat"]);
            Assert.AreEqual(10, dict1["left"]);
            
            var dict2 = Cucumbulator.Calls[1];
            Assert.AreEqual(15, dict2["have"]);
            Assert.AreEqual(2, dict2["eat"]);
            Assert.AreEqual(13, dict2["left"]);
        }

        [ActionSteps]
        public class Cucumbulator
        {
            public static List<Dictionary<string, int>> Calls = new List<Dictionary<string, int>>();

            static int _counter;

            [Given("I have $cukeCount cucumbers")]
            public void GivenHaveCukes(int cukeCount)
            {
                Console.WriteLine("Have {0} cukes", cukeCount);
                Calls.Add(new Dictionary<string, int>());
                Calls[_counter]["have"] = cukeCount;
            }

            [When("I eat $cukeCount cucumbers")]
            public void WhenEating(int cukeCount)
            {
                Console.WriteLine("Eating {0} cukes", cukeCount);
                Calls[_counter]["eat"] = cukeCount;
            }

            [Then("I have $cukeCount cucumbers left")]
            public void ThenHave(int cukeCount)
            {
                Console.WriteLine("Then I have {0} cukes", cukeCount);
                Calls[_counter]["left"] = cukeCount;
                _counter++;
            }
        }

        [Test]
        public void CanRunFeature()
        {
            var feature = new Feature("feature", NoTags())
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("scenario", NoTags())
                                              {
                                                  Steps =
                                                      {
                                                          Step.Given("i am logged in as administrator"),
                                                          Step.When(@"i change my name to ""joe bananas"""),
                                                          Step.Then("something fantastic happens")
                                                      }
                                              }
                                      }
                              };

            var result = runner.Run(feature, new[] { typeof(ClassWithActionSteps) }, DefaultOptions());

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

        [Test]
        public void DoesNotRunStuffWhenDryRunning()
        {
            var feature = new Feature("feature", NoTags())
                              {
                                  Scenarios =
                                      {
                                          new ExecutableScenario("scenario", NoTags())
                                              {
                                                  Steps =
                                                      {
                                                          Step.Given("i am logged in as administrator"),
                                                          Step.When(@"i change my name to ""joe bananas"""),
                                                          Step.Then("something fantastic happens")
                                                      }
                                              }
                                      }
                              };

            runner.Run(feature, new[] {typeof (ClassWithActionSteps)}, new RunnerOptions {DruRun = true});

            Assert.AreEqual(0, ClassWithActionSteps.GivenCalls);
            Assert.AreEqual(0, ClassWithActionSteps.WhenCalls);
            Assert.AreEqual(0, ClassWithActionSteps.ThenCalls);

            Assert.IsTrue(ClassWithActionSteps.Disposed);
        }

        [Test]
        public void CanRunClassWithMultilineStepArguments()
        {
            var given = Step.Given("the following users are created:");
            given.Parameters.Add(new Dictionary<string, string>
                                     {
                                         {"username", "joe"},
                                         {"password", "secret"},
                                         {"birthYear", "1979"}
                                     });
            given.Parameters.Add(new Dictionary<string, string>
                                     {
                                         {"username", "mookid"},
                                         {"password", "yay!"},
                                         {"birthYear", "1979"}
                                     });

            var when = Step.When("I log in as");
            when.Parameters.Add(new Dictionary<string, string>
                                    {
                                        {"username", "joe"},
                                        {"password", "secret"},
                                        {"age", "31"},
                                    });
            when.Parameters.Add(new Dictionary<string, string>
                                    {
                                        {"username", "mookid"},
                                        {"password", "yay!"},
                                        {"age", "29"},
                                    });

            var scenario = new ExecutableScenario("woot!", NoTags())
                               {
                                   Steps = {given, when}
                               };

            var feature = new Feature("woot!", NoTags()) {Scenarios = {scenario}};
            runner.Run(feature, new[] {typeof (ClassWithMultilineStepArguments)}, DefaultOptions());

            var dicts = ClassWithMultilineStepArguments.Given;
            Assert.IsNotNull(dicts);
            Assert.AreEqual(2, dicts.Count);
            Assert.AreEqual("joe", dicts[0]["username"]);
            Assert.AreEqual("secret", dicts[0]["password"]);
            Assert.AreEqual("1979", dicts[0]["birthYear"]);
            Assert.AreEqual("mookid", dicts[1]["username"]);
            Assert.AreEqual("yay!", dicts[1]["password"]);
            Assert.AreEqual("1979", dicts[1]["birthYear"]);

            var arr = ClassWithMultilineStepArguments.When;
            Assert.IsNotNull(arr);
            Assert.AreEqual("joe", arr[0].Username);
            Assert.AreEqual("secret", arr[0].Password);
            Assert.AreEqual(31, arr[0].Age);
            Assert.AreEqual("mookid", arr[1].Username);
            Assert.AreEqual("yay!", arr[1].Password);
            Assert.AreEqual(29, arr[1].Age);
        }

        [ActionSteps]
        class ClassWithMultilineStepArguments
        {
            public static List<Dictionary<string, string>> Given;
            public static UserLoggedIn[] When;

            [Given("the following users are created:")]
            public void GivenUsersAreCreated(List<Dictionary<string, string>> users)
            {
                Given = users;
            }

            [When("I log in as")]
            public void WhenLogInAs(UserLoggedIn[] userLoggedIn)
            {
                When = userLoggedIn;
            }

            public class UserLoggedIn
            {
                public string Username { get; set; }
                public string Password { get; set; }
                public int Age { get; set; }
            }
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

            public void Dispose()
            {
                Disposed = true;
            }

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
    }
}