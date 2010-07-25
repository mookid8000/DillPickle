using System;
using System.Collections.Generic;
using DillPickle.Framework.Runner.Api;
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

            runner.Run(feature, new[]{typeof(Cucumbulator)});

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
            
            var result = runner.Run(feature, new[] {typeof (ClassWithActionSteps)});

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
            runner.Run(feature, new[] {typeof (ClassWithMultilineStepArguments)});

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
            public static List<Dictionary<string, string>> Then;

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