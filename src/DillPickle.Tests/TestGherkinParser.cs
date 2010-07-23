using System.Linq;
using NUnit.Framework;
using DillPickle.Framework.Parser;

namespace DillPickle.Tests
{
    [TestFixture]
    public class TestGherkinParser : FixtureBase
    {
        GherkinParser parser;

        public override void DoSetUp()
        {
            parser = new GherkinParser();
        }

        void AssertStep(Step step, StepType expectedStepType, string expectedText)
        {
            Assert.AreEqual(expectedStepType, step.StepType);
            Assert.AreEqual(expectedText, step.Text);
        }

        [Test]
        public void CanParseTagsAsWell()
        {
            var result = parser.Parse(
                @"
@business @something
Feature: Some terse yet descriptive text of what is desired
   In order to realize a named business value
   As an explicit system actor
   I want to gain some beneficial outcome which furthers the goal
 
    @important
   Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved
");

            var feature = result.Features[0];

            var tags = feature.Tags.OrderBy(t => t).ToList();
            Assert.AreEqual(2, tags.Count);
            Assert.AreEqual("business", tags[0]);
            Assert.AreEqual("something", tags[1]);

            var scenario = feature.Scenarios[0];

            tags = scenario.Tags.OrderBy(t => t).ToList();
            Assert.AreEqual(3, tags.Count);
            Assert.AreEqual("business", tags[0]);
            Assert.AreEqual("important", tags[1]);
            Assert.AreEqual("something", tags[2]);
        }

        [Test]
        public void CanParseValidGherkinStuffGood()
        {
            var result = parser.Parse(
                @"
 Feature: Some terse yet descriptive text of what is desired
   In order to realize a named business value
   As an explicit system actor
   I want to gain some beneficial outcome which furthers the goal
 
   Scenario: Some determinable business situation
     Given some precondition
       And some other precondition
      When some action by the actor
       And some other action
       And yet another action
      Then some testable outcome is achieved
       And something else we can check happens too
 
   Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved
");

            var feature = result.Features[0];

            Assert.AreEqual("Some terse yet descriptive text of what is desired", feature.Headline);
            Assert.AreEqual(
                @"In order to realize a named business value
As an explicit system actor
I want to gain some beneficial outcome which furthers the goal
",
                feature.Description);

            Assert.AreEqual(2, feature.Scenarios.Count);

            var scenario1 = feature.Scenarios[0];
            var scenario1Steps = scenario1.Steps;

            Assert.AreEqual("Some determinable business situation", scenario1.Headline);
            Assert.AreEqual(7, scenario1Steps.Count);

            AssertStep(scenario1Steps[0], StepType.Given, "some precondition");
            AssertStep(scenario1Steps[1], StepType.Given, "some other precondition");
            AssertStep(scenario1Steps[2], StepType.When, "some action by the actor");
            AssertStep(scenario1Steps[3], StepType.When, "some other action");
            AssertStep(scenario1Steps[4], StepType.When, "yet another action");
            AssertStep(scenario1Steps[5], StepType.Then, "some testable outcome is achieved");
            AssertStep(scenario1Steps[6], StepType.Then, "something else we can check happens too");

            var scenario2 = feature.Scenarios[1];
            var scenario2Steps = scenario2.Steps;

            Assert.AreEqual("A different situation", scenario2.Headline);
            Assert.AreEqual(3, scenario2Steps.Count);

            AssertStep(scenario2Steps[0], StepType.Given, "some precondition");
            AssertStep(scenario2Steps[1], StepType.When, "some action by the actor");
            AssertStep(scenario2Steps[2], StepType.Then, "some testable outcome is achieved");
        }

        [Test]
        public void DoesNotAccumulateTagsTooLong()
        {
            var result = parser.Parse(
                @"
@business @something
Feature: Some terse yet descriptive text of what is desired
   In order to realize a named business value
   As an explicit system actor
   I want to gain some beneficial outcome which furthers the goal
 
    @important
   Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved

    @notImportant
   Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved

@something
Feature: Some terse yet descriptive text of what is desired
   In order to realize a named business value
   As an explicit system actor
   I want to gain some beneficial outcome which furthers the goal
 
   Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved
");

            var feature1 = result.Features[0];
            Assert.AreEqual(2, feature1.Tags.Count);

            Assert.AreEqual(3, feature1.Scenarios[0].Tags.Count);
            Assert.IsTrue(feature1.Scenarios[0].Tags.Any(t => t == "important"));
            Assert.AreEqual(3, feature1.Scenarios[1].Tags.Count);
            Assert.IsTrue(feature1.Scenarios[1].Tags.Any(t => t == "notImportant"));

            var feature2 = result.Features[1];
            Assert.AreEqual(1, feature2.Tags.Count);
            Assert.IsTrue(feature1.Tags.Any(t => t == "something"));

            Assert.AreEqual(1, feature2.Scenarios[0].Tags.Count);
            Assert.IsTrue(feature2.Scenarios[0].Tags.Any(t => t == "something"));
        }

        [Test]
        public void DoesNotPutTagOfSecondFeatureIntoLastScenarioOfPrecedingScenario()
        {
            var result = parser.Parse(
                @"
Feature: Some terse yet descriptive text of what is desired
 
   Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved

@something
Feature: Some terse yet descriptive text of what is desired
 
   Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved
");

            Assert.AreEqual(1, result.Features[1].Tags.Count);
            Assert.AreEqual("something", result.Features[1].Tags[0]);
        }

        [Test]
        public void UnderstandsMultilineStepArguments()
        {
            var result = parser.Parse(
                @"
Feature: Whatever

    Scenario: Check if it works
        
    Given the following users:
        | username  | password  | birthYear |
        | joe       | secret    | 1979      |
        | mookid    | yay!      | 1979      |
    When I log in as mookid
    Then I get a brand spanking new session fo shizzle
");

            var scenario = result.Features[0].Scenarios[0];
            Assert.AreEqual(3, scenario.Steps.Count);
            
            var step = scenario.Steps[0];
            Assert.AreEqual(StepType.Given, step.StepType);
            Assert.AreEqual(2, step.Parameters.Count);
        }

        [Test]
        public void UnderstandsMultilineStepArgumentsAndDoesNotChokeIfMultipleTablesAreGiven()
        {
            var result = parser.Parse(
                @"
Feature: Whatever

    Scenario: Check if it works
        
    Given the following users:
        | username  | password  | birthYear |
        | joe       | secret    | 1979      |
        | mookid o c| yay!      | 1979      |
    When I log in as 
        | username  | password  |
        | joe       | secret    |
        | mookid o c| yay!      |
    Then I get a brand spanking new session fo shizzle for
        | username  |
        | joe       |
        | mookid o c|
");

            var scenario = result.Features[0].Scenarios[0];
            Assert.AreEqual(3, scenario.Steps.Count);

            var step = scenario.Steps[0];
            Assert.AreEqual(StepType.Given, step.StepType);
            Assert.AreEqual(2, step.Parameters.Count);
            var parameters = step.Parameters;
            
            var firstRow = parameters[0];
            Assert.AreEqual("joe", firstRow["username"]);
            Assert.AreEqual("secret", firstRow["password"]);
            Assert.AreEqual("1979", firstRow["birthYear"]);
  
            var nextRow = parameters[1];
            Assert.AreEqual("mookid o c", nextRow["username"]);
            Assert.AreEqual("yay!", nextRow["password"]);
            Assert.AreEqual("1979", nextRow["birthYear"]);

            step = scenario.Steps[1];
            Assert.AreEqual(StepType.When, step.StepType);
            Assert.AreEqual(2, step.Parameters.Count);

            step = scenario.Steps[2];
            Assert.AreEqual(StepType.Then, step.StepType);
            Assert.AreEqual(2, step.Parameters.Count);
        }

        [Test]
        public void CanParseScenarioOutlines()
        {
            var result = parser.Parse(
                @"
Scenario Outline: eating
  Given there are <start> cucumbers
  When I eat <eat> cucumbers
  Then I should have <left> cucumbers

  Examples:
    | start | eat | left |
    |  12   |  5  |  7   |
    |  20   |  5  |  15  |

");

            Assert.AreEqual(1, result.Features.Count);
            Assert.AreEqual(1, result.Features[0].Scenarios.Count);
            var scenario = (ScenarioOutline) result.Features[0].Scenarios[0];
            Assert.AreEqual(2, scenario.Examples.Count);
        }

        [Test]
        public void CanParseMultilineStepArgumentsAndScenarioOutlineInSameScenario()
        {
            var result = parser.Parse(
                @"
Scenario Outline: eating
  Given there are <start> cucumbers
    and the following users exist:
        | name  | age   |
        | joe   | 30    |
        | moe   | 31    |
  When I eat <eat> cucumbers
  Then I should have <left> cucumbers

  Examples:
    | start | eat | left |
    |  12   |  5  |  7   |
    |  20   |  5  |  15  |

Scenario Outline: eating II
  Given there are <start> cucumbers
    and the following users exist:
        | name  | age   |
        | joe   | 30    |
        | moe   | 31    |
  When I eat <eat> cucumbers
  Then I should have <left> cucumbers

  Examples:
    | start | eat | left |
    |  12   |  5  |  7   |
    |  20   |  5  |  15  |

");
            AssertScenario((ScenarioOutline)result.Features[0].Scenarios[0]);
            AssertScenario((ScenarioOutline)result.Features[0].Scenarios[1]);
        }

        void AssertScenario(ScenarioOutline scenario)
        {
            var step = scenario.Steps[1];
            
            Assert.AreEqual(StepType.Given, step.StepType);
            
            var parameters = step.Parameters;
            
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("joe", parameters[0]["name"]);
            Assert.AreEqual("30", parameters[0]["age"]);
            Assert.AreEqual("moe", parameters[1]["name"]);
            Assert.AreEqual("31", parameters[1]["age"]);

            var examples = scenario.Examples;

            Assert.AreEqual(2, examples.Count);
            Assert.AreEqual("12", examples[0]["start"]);
            Assert.AreEqual("5", examples[0]["eat"]);
            Assert.AreEqual("7", examples[0]["left"]);
            Assert.AreEqual("20", examples[1]["start"]);
            Assert.AreEqual("5", examples[1]["eat"]);
            Assert.AreEqual("15", examples[1]["left"]);
        }
    }
}