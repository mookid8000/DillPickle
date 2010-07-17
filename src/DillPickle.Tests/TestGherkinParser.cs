using System.Collections.Generic;
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
            ParseResult result = parser.Parse(
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

            Feature feature = result.Features[0];

            List<string> tags = feature.Tags.OrderBy(t => t).ToList();
            Assert.AreEqual(2, tags.Count);
            Assert.AreEqual("business", tags[0]);
            Assert.AreEqual("something", tags[1]);

            Scenario scenario = feature.Scenarios[0];

            tags = scenario.Tags.OrderBy(t => t).ToList();
            Assert.AreEqual(3, tags.Count);
            Assert.AreEqual("business", tags[0]);
            Assert.AreEqual("important", tags[1]);
            Assert.AreEqual("something", tags[2]);
        }

        [Test]
        public void CanParseValidGherkinStuffGood()
        {
            ParseResult result = parser.Parse(
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

            Feature feature = result.Features[0];

            Assert.AreEqual("Some terse yet descriptive text of what is desired", feature.Headline);
            Assert.AreEqual(
                @"In order to realize a named business value
As an explicit system actor
I want to gain some beneficial outcome which furthers the goal
",
                feature.Description);

            Assert.AreEqual(2, feature.Scenarios.Count);

            Scenario scenario1 = feature.Scenarios[0];
            List<Step> scenario1Steps = scenario1.Steps;

            Assert.AreEqual("Some determinable business situation", scenario1.Headline);
            Assert.AreEqual(7, scenario1Steps.Count);

            AssertStep(scenario1Steps[0], StepType.Given, "some precondition");
            AssertStep(scenario1Steps[1], StepType.Given, "some other precondition");
            AssertStep(scenario1Steps[2], StepType.When, "some action by the actor");
            AssertStep(scenario1Steps[3], StepType.When, "some other action");
            AssertStep(scenario1Steps[4], StepType.When, "yet another action");
            AssertStep(scenario1Steps[5], StepType.Then, "some testable outcome is achieved");
            AssertStep(scenario1Steps[6], StepType.Then, "something else we can check happens too");

            Scenario scenario2 = feature.Scenarios[1];
            List<Step> scenario2Steps = scenario2.Steps;

            Assert.AreEqual("A different situation", scenario2.Headline);
            Assert.AreEqual(3, scenario2Steps.Count);

            AssertStep(scenario2Steps[0], StepType.Given, "some precondition");
            AssertStep(scenario2Steps[1], StepType.When, "some action by the actor");
            AssertStep(scenario2Steps[2], StepType.Then, "some testable outcome is achieved");
        }

        [Test]
        public void DoesNotAccumulateTagsTooLong()
        {
            ParseResult result = parser.Parse(
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

            Feature feature1 = result.Features[0];
            Assert.AreEqual(2, feature1.Tags.Count);

            Assert.AreEqual(3, feature1.Scenarios[0].Tags.Count);
            Assert.IsTrue(feature1.Scenarios[0].Tags.Any(t => t == "important"));
            Assert.AreEqual(3, feature1.Scenarios[1].Tags.Count);
            Assert.IsTrue(feature1.Scenarios[1].Tags.Any(t => t == "notImportant"));

            Feature feature2 = result.Features[1];
            Assert.AreEqual(1, feature2.Tags.Count);
            Assert.IsTrue(feature1.Tags.Any(t => t == "something"));

            Assert.AreEqual(1, feature2.Scenarios[0].Tags.Count);
            Assert.IsTrue(feature2.Scenarios[0].Tags.Any(t => t == "something"));
        }

        [Test]
        public void DoesNotPutTagOfSecondFeatureIntoLastScenarioOfPrecedingScenario()
        {
            ParseResult result = parser.Parse(
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
    }
}