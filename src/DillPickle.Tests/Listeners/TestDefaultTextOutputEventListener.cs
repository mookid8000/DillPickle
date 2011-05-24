using System;
using System.Collections.Generic;
using System.IO;
using DillPickle.Framework.Listeners;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner.Api;
using DillPickle.Framework.Types;
using NUnit.Framework;

namespace DillPickle.Tests.Listeners
{
    [TestFixture]
    public class TestDefaultTextOutputEventListener : FixtureBase
    {
        [Test]
        public void FormatsThingsNicely()
        {
            var listener = new OpenDefaultTextOutputEventListener();
            listener.Prepare();

            ExecuteFeature(listener);
            ExecuteFeature(listener);

            listener.Finish();

            Assert.AreEqual(
@"
(cyan)  @tag1 @tag2
(cyan)  Feature: Most important feature
(cyan)    This is one of the
(cyan)    most important features

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition
(yellow)    And another precondition - Pending
(green)     When I do this
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens - Failed

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition
(yellow)    And another precondition - Pending
(green)     When I do this
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens - Failed

(red)       OH NOES!!!
(red)       OH NOES!!!

(cyan)  @tag1 @tag2
(cyan)  Feature: Most important feature
(cyan)    This is one of the
(cyan)    most important features

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition
(yellow)    And another precondition - Pending
(green)     When I do this
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens - Failed

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition
(yellow)    And another precondition - Pending
(green)     When I do this
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens - Failed

(red)       OH NOES!!!
(red)       OH NOES!!!",
                       listener.Text);
        }

        [Test]
        public void FormatsThingsNicelyWithTimespans()
        {
            Time.SetTime(DateTime.Today + TimeSpan.FromHours(10) + TimeSpan.FromMinutes(23) + TimeSpan.FromSeconds(55));
            var listener = new OpenDefaultTextOutputEventListener {ShowCurrentTimes = true};
            listener.Prepare();

            ExecuteFeature(listener);
            ExecuteFeature(listener);

            listener.Finish();

            Assert.AreEqual(
@"
(cyan)  @tag1 @tag2
(cyan)  Feature: Most important feature
(cyan)    This is one of the
(cyan)    most important features

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition [10:23:55]
(yellow)    And another precondition [10:23:55] - Pending
(green)     When I do this [10:23:55]
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens [10:23:55] - Failed

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition [10:23:55]
(yellow)    And another precondition [10:23:55] - Pending
(green)     When I do this [10:23:55]
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens [10:23:55] - Failed

(red)       OH NOES!!!
(red)       OH NOES!!!

(cyan)  @tag1 @tag2
(cyan)  Feature: Most important feature
(cyan)    This is one of the
(cyan)    most important features

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition [10:23:55]
(yellow)    And another precondition [10:23:55] - Pending
(green)     When I do this [10:23:55]
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens [10:23:55] - Failed

(cyan)    @tag3 @tag4 @tag5
(cyan)    Scenario: Sunshine!
(green)     Given some precondition [10:23:55]
(yellow)    And another precondition [10:23:55] - Pending
(green)     When I do this [10:23:55]
(green)       | Name       | Value         |
(green)       | first name | some value    |
(green)       | last name  | another value |
(red)       Then that happens [10:23:55] - Failed

(red)       OH NOES!!!
(red)       OH NOES!!!",
                       listener.Text);
        }

        void ExecuteFeature(OpenDefaultTextOutputEventListener listener)
        {
            var feature = new Feature("Most important feature", WithTags("tag1", "tag2"));
            feature.AddText("This is one of the");
            feature.AddText("most important features");
            listener.BeforeFeature(feature);

            ExecuteScenario(listener, feature);
            ExecuteScenario(listener, feature);

            listener.AfterFeature(feature, new FeatureResult(feature));
        }

        void ExecuteScenario(OpenDefaultTextOutputEventListener listener, Feature feature)
        {
            var step1 = Step.Given("some precondition");
            var step2 = Step.And("another precondition", StepType.Given);
            var step3 = Step.When("I do this");
            step3.Parameters = new List<Dictionary<string, string>>
                                   {
                                       new Dictionary<string, string> {{"Name", "first name"}, {"Value", "some value"}},
                                       new Dictionary<string, string> {{"Name", "last name"}, {"Value", "another value"}},
                                   };
            var step4 = Step.Then("that happens");
            var scenario = new ExecutableScenario("Sunshine!", WithTags("tag3", "tag4", "tag5"))
                               {
                                   Steps = { step1, step2, step3, step4 }
                               };

            listener.BeforeScenario(feature, scenario);

            var stepResult1 = new StepResult("Given some precondition") { Result = Result.Success };
            var stepResult2 = new StepResult("And another precondition") { Result = Result.Pending };
            var stepResult3 = new StepResult("When I do this") { Result = Result.Success };
            var stepResult4 = new StepResult("Then that happens")
                                  {
                                      Result = Result.Failed,
                                      ErrorMessage = "OH NOES!!!"
                                  };

            listener.BeforeStep(feature, scenario, step1);
            listener.AfterStep(feature, scenario, step1, stepResult1);

            listener.BeforeStep(feature, scenario, step2);
            listener.AfterStep(feature, scenario, step2, stepResult2);

            listener.BeforeStep(feature, scenario, step3);
            listener.AfterStep(feature, scenario, step3, stepResult3);

            listener.BeforeStep(feature, scenario, step4);
            listener.AfterStep(feature, scenario, step4, stepResult4);

            var scenarioResult = new ScenarioResult("Sunshine!")
                                     {
                                         StepResults = { stepResult1, stepResult2, stepResult3, stepResult4, }
                                     };
            listener.AfterScenario(feature, scenario, scenarioResult);
        }

        IEnumerable<string> WithTags(params string[] tags)
        {
            return tags;
        }

        class OpenDefaultTextOutputEventListener : DefaultTextOutputEventListener
        {
            readonly List<string> lines = new List<string>();

            public string Text
            {
                get { return string.Join(Environment.NewLine, lines.ToArray()); }
            }

            protected override void WriteLineRaw(ConsoleColor color, int tabs, string text)
            {
                using (var reader = new StringReader(text))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(ColorAsString(color) + new string(' ', tabs * 2) + line);
                    }
                }
            }

            protected override void WriteLineRaw()
            {
                lines.Add("");
            }

            string ColorAsString(ConsoleColor color)
            {
                switch (color)
                {
                    case ConsoleColor.Cyan:
                        return "(cyan)  ";

                    case ConsoleColor.Green:
                        return "(green) ";

                    case ConsoleColor.Yellow:
                        return "(yellow)";

                    case ConsoleColor.Red:
                        return "(red)   ";

                    default:
                        throw new ArgumentException("Unknown color: " + color);
                }
            }
        }
    }
}