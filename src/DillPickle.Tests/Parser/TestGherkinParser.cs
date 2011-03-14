using System;
using System.Collections.Generic;
using System.Linq;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Parser.Api;
using NUnit.Framework;
using DillPickle.Framework.Parser;

namespace DillPickle.Tests.Parser
{
    [TestFixture]
    public class TestGherkinParser : FixtureBase
    {
        StupidGherkinParser parser;

        public override void DoSetUp()
        {
            parser = new StupidGherkinParser();
        }

        [Test]
        public void IgnoresCommentsAnywhereTheyAreEncountered()
        {
            var result = parser.Parse(@"
# comment outside of anything

Feature: Distribution with energy constraints
    # comment in the story text
	Verifies that the energy constraints are taken into account when distributing.
    # comment in the story text

	Background:
        # comment in background
		Given that the test site exists

	Scenario: Test Scenario
        # comment in scenario
		Given the following analog local units:
			| Alias			| TechMin	| TechMax	| ForecastMin	| ForecastMax	| EstMaxCap	| ObsMin	| ObsMax	| ProdOrConsCost	| Efficiency	|
			| REG_T1_001	| 9 kW		| 12 kW		| 0 kW			| 12 kW			| 2 kWh		| 0 m		| 2 m		| 10				| 1				|
			| REG_T1_002	| 9 kW		| 12 kW		| 0 kW			| 12 kW			| 2 kWh		| 0 m		| 2 m		| 100				| 1				|
        # comment in scenario
        # comment in scenario
        # comment in scenario
		and the following signals:
			| Tag				| Value	|
			| REG_T1_001.CurObs	| 2		|
			| REG_T1_002.CurObs	| 2		|");

            var features = result.Features;
            Assert.AreEqual(1, features.Count);
            var feature = features[0];
            Assert.AreEqual("Distribution with energy constraints", feature.Headline);
            Assert.AreEqual("Verifies that the energy constraints are taken into account when distributing.\r\n", feature.Description);
            var scenarios = feature.Scenarios;
            Assert.AreEqual(1, scenarios.Count);
            var scenario = scenarios[0];
            Assert.AreEqual("Test Scenario", scenario.Headline);
            var steps = scenario.Steps;
            Assert.AreEqual(2, steps.Count);
            Assert.AreEqual(Step.Given("the following analog local units:"), steps[0]);
            Assert.AreEqual(Step.And("the following signals:", StepType.Given), steps[1]);
        }

        [Test]
        public void CanParseTwoTablesInARow()
        {
            var result =
                parser.Parse(
                    @"
Story: Distribution with energy constraints
	Verifies that the energy constraints are taken into account when distributing.

	Background:
		Given that the test site exists

	Scenario: Test Scenario
		Given the following analog local units:
			| Alias			| TechMin	| TechMax	| ForecastMin	| ForecastMax	| EstMaxCap	| ObsMin	| ObsMax	| ProdOrConsCost	| Efficiency	|
			| REG_T1_001	| 9 kW		| 12 kW		| 0 kW			| 12 kW			| 2 kWh		| 0 m		| 2 m		| 10				| 1				|
			| REG_T1_002	| 9 kW		| 12 kW		| 0 kW			| 12 kW			| 2 kWh		| 0 m		| 2 m		| 100				| 1				|

		and the following signals:
			| Tag				| Value	|
			| REG_T1_001.CurObs	| 2		|
			| REG_T1_002.CurObs	| 2		|");

            var features = result.Features;
            Assert.AreEqual(1, features.Count);
            var scenarios = features[0].Scenarios;
            Assert.AreEqual(1, scenarios.Count);
            var scenario = scenarios[0].GetExecutableScenarios().Single();
            var steps = scenario.Steps;
            Assert.AreEqual(2, steps.Count);

            var expectedColumnNamesFirstTable = new[]{"Alias", "TechMin", "TechMax", "ForecastMin", "ForecastMax", "EstMaxCap", "ObsMin", "ObsMax", "ProdOrConsCost", "Efficiency"};
            var actualColumnNamesFirstTable = steps[0].Parameters.First().Keys;
            AssertNames(expectedColumnNamesFirstTable, actualColumnNamesFirstTable);

            var expectedColumnNamesSecondTable = new[]{"Tag", "Value"};
            var actualColumnNamesSecondTable = steps[1].Parameters.First().Keys;
            AssertNames(expectedColumnNamesSecondTable, actualColumnNamesSecondTable);
        }

        private void AssertNames(IEnumerable<string> expected, IEnumerable<string> actual)
        {
            Assert.AreEqual(expected.OrderBy(e => e).ToArray(), actual.OrderBy(e => e).ToArray());
        }

        [Test]
        public void CanParseLargeFeature()
        {
            var result = parser.Parse(
                @"

Story: Stuff
 
            Scenario: Primary regulation reservation
                         Given the following signals
                                     | Tag                  | Value |
                                     | TEST_T1_411.Ready    | True  |
                                     | TEST_T1_411.Running  | True  |
                                     | TEST_T1_411.CommOk   | True  |
                                     | TEST_T1_412.Ready    | True  |
                                     | TEST_T1_412.Running  | True  |
                                     | TEST_T1_412.CommOk   | True  |
                                     | TEST_T1_413.Ready    | True  |
                                     | TEST_T1_413.Running  | True  |
                                     | TEST_T1_413.CommOk   | True  |
 
                         Given the following lus
                                     | Type         | Alias         | MinPower  | MaxPower  | RampUp    | RampDown  | ActivationMode    | ActivationType        | ObservableMin | ObservableMax |
                                     | AnalogHydro  | TEST_T1_411   | 50 KW     | 350 KW    | 30 s      | 30 s      | Auto              | AnalogueProduction    | 24,6          | 25            |
                                     | AnalogHydro  | TEST_T1_412   | 50 KW     | 350 KW    | 30 s      | 30 s      | Auto              | AnalogueProduction    | 24,6          | 25            |
                                     | AnalogHydro  | TEST_T1_413   | 50 KW     | 350 KW    | 30 s      | 30 s      | Man_BR            | AnalogueProduction    | 24,6          | 25            |
                                    
                         Given the following owner settings
                                     | Alias        | BaseLoad  | PriceUF   | PriceOF   |
                                     | TEST_T1_411  | 150 KW    | 0,5       | 0,5       |
                                     | TEST_T1_412  | 150 KW    | 0,6       | 0,6       |
                                     | TEST_T1_413  | 150 KW    | 0,1       | 0,1       |
                        
                         When we wait 30 s
                        
                         and CPC makes PR reservation
                                     | UFDB | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | 20   | 200   | 0,40          | 20    | 200   | 0,20          |
 
                         and we wait 12 s
                        
                         Then the following PR reservations should have been made:
                                     | Alias        | UFDB  | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | TEST_T1_411  | 20    | 110   | 200 KW        | 20    | 110   | 100 KW        |
                                     | TEST_T1_412  | 110   | 200   | 200 KW        | 110   | 200   | 100 KW        |
                                    
                         When CPC makes PR reservation
                                     | UFDB | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | 20   | 200   | 0             | 20    | 200   | 0,20          |
 
                         and we wait 12 s
                        
                         Then the following PR reservations should have been made:
                                     | Alias        | UFDB  | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | TEST_T1_411  | 20    | 110   | 0 KW          | 20    | 110   | 100 KW        |
                                     | TEST_T1_412  | 110   | 200   | 0 KW          | 110   | 200   | 100 KW        |
                        
                         When CPC makes PR reservation
                                     | UFDB | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | 20   | 200   | 0,40          | 20    | 200   | 0             |
 
                         and we wait 12 s
                        
                         Then the following PR reservations should have been made:
                                     | Alias        | UFDB  | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | TEST_T1_411  | 20    | 110   | 200 KW        | 20    | 110   | 0 KW          |
                                     | TEST_T1_412  | 110   | 200   | 200 KW        | 110   | 200   | 0 KW          |
                        
                         When CPC makes PR reservation
                                     | UFDB | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | 20   | 200   | 0             | 20    | 200   | 0             |
 
                         and we wait 12 s
                        
                         Then the following PR reservations should have been made:
                                     | Alias        | UFDB  | UFRB  | UF_MaxPower   | OFDB  | OFRB  | OF_MaxPower   |
                                     | TEST_T1_411  | 20    | 110   | 0 KW          | 20    | 110   | 0 KW          |
                                     | TEST_T1_412  | 110   | 200   | 0 KW          | 110   | 200   | 0 KW          |
                        
");

            Assert.AreEqual(1, result.Features.Count);
            var feature = result.Features[0];
            Assert.AreEqual(1, feature.Scenarios.Count);
            var scenario = feature.Scenarios[0];
            var steps = scenario.Steps;
            Assert.AreEqual(16, steps.Count);

            AssertStepWithParameters(steps[0], StepType.Given, "the following signals",
                                     new Dictionary<string, string[]>
                                         {
                                             {
                                                 "Tag", new[]
                                                            {
                                                                "TEST_T1_411.Ready", "TEST_T1_411.Running",
                                                                "TEST_T1_411.CommOk",
                                                                "TEST_T1_412.Ready", "TEST_T1_412.Running",
                                                                "TEST_T1_412.CommOk",
                                                                "TEST_T1_413.Ready", "TEST_T1_413.Running",
                                                                "TEST_T1_413.CommOk",
                                                            }
                                                 },
                                             {
                                                 "Value",
                                                 new[]
                                                     {
                                                         "True", "True", "True", "True", "True", "True", "True", "True",
                                                         "True"
                                                     }
                                                 }
                                         });

            AssertStepWithParameters(steps[1], StepType.Given, "the following lus",
                new Dictionary<string, string[]>{{"Type", new[]{"AnalogHydro", "AnalogHydro", "AnalogHydro"}}});

            AssertStepWithParameters(steps[2], StepType.Given, "the following owner settings", new Dictionary<string, string[]>{{"Alias", new[]{"TEST_T1_411", "TEST_T1_412", "TEST_T1_413"}}});
            AssertStepNoParameters(steps[3], StepType.When, "we wait 30 s");
            AssertStepWithParameters(steps[4], StepType.When, "CPC makes PR reservation",
                                     new Dictionary<string, string[]>
                                         {
                                             {"UFDB", new[] {"20"}},
                                             {"UFRB", new[] {"200"}},
                                             {"UF_MaxPower", new[] {"0,40"}},
                                             {"OFDB", new[] {"20"}},
                                             {"OFRB", new[] {"200"}},
                                             {"OF_MaxPower", new[] {"0,20"}}
                                         });
            AssertStepNoParameters(steps[5], StepType.When, "we wait 12 s");

        }

        void AssertStepNoParameters(Step step, StepType expectedStepType, string expectedText)
        {
            AssertStep(step, expectedStepType, expectedText);
            Assert.AreEqual(0, step.Parameters.Count);
        }

        void AssertStepWithParameters(Step step, StepType expectedStepType, string expectedText, Dictionary<string, string[]> expectedParameterColumns)
        {
            AssertStep(step, expectedStepType, expectedText);
            AssertParameters(step, expectedParameterColumns);
        }

        void AssertParameters(Step step, ICollection<KeyValuePair<string, string[]>> dictionary)
        {
            foreach(var kvp in dictionary)
            {
                var columnName = kvp.Key;

                Assert.AreEqual(kvp.Value.Length, step.Parameters.Count, "Wrong number of rows for column {0}", columnName);

                var index = 0;
                foreach(var parameter in step.Parameters)
                {
                    Assert.AreEqual(kvp.Value[index++], parameter[columnName]);
                }
            }
        }

        [Test]
        public void FeatureCanHaveBackgroundSteps()
        {
            var result = parser.Parse(
                @"
Feature: Some terse yet descriptive text of what is desired
   In order to realize a named business value
   As an explicit system actor
   I want to gain some beneficial outcome which furthers the goal
 
    Background:
        Given something
            and something else

    Scenario: A different situation
     Given some precondition
      When some action by the actor
      Then some testable outcome is achieved
");

            var backgroundSteps = result.Features[0].BackgroundSteps;

            Assert.AreEqual(2, backgroundSteps.Count);
            
            Assert.AreEqual(StepType.Given, backgroundSteps[0].StepType);
            Assert.AreEqual("something", backgroundSteps[0].Text);
            
            Assert.AreEqual(StepType.Given, backgroundSteps[1].StepType);
            Assert.AreEqual("something else", backgroundSteps[1].Text);
        }

        [Test]
        public void WhenOrThenStepsMayNotAppearInsideTheBackgroundSection()
        {
            ShouldThrow(@"
    Background: 
        Given something
        When I do something
    ");
        }

        [Test]
        public void ThrowsOnVariousScenarioOutlineErrors()
        {
            ShouldThrow(@"
Scenario outline:
    Given I have <some> stuff
        and I have <somethingElse> stuff

    Examples:
        | somethingElse |
        | 234           |");

            ShouldThrow(@"
    Scenario outline:
        Given I have 5 stuff
");

            ShouldThrow(@"
    Scenario outline:
        Given I have <howMany> stuff
");
            
            ShouldThrow(@"Scenario: something

    Given something

    Examples:
        | shouldOnlyPutExamplesInScenarioOutline |
");
        }

        void ShouldThrow(string text)
        {
            Assert.Throws<GherkinParseException>(
                () => parser.Parse(text));
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

scenario outline: eating II
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

            var scenarios = scenario.GetExecutableScenarios();
            
            Assert.AreEqual(2, scenarios.Count);
            
            var firstScenario = scenarios[0];
            Assert.AreEqual(4, firstScenario.Steps.Count);
            AssertStep(firstScenario.Steps[0], StepType.Given, "there are 12 cucumbers");
            AssertStep(firstScenario.Steps[1], StepType.Given, "the following users exist:");
            AssertStep(firstScenario.Steps[2], StepType.When, "I eat 5 cucumbers");
            AssertStep(firstScenario.Steps[3], StepType.Then, "I should have 7 cucumbers");
            
            var secondScenario = scenarios[1];
            Assert.AreEqual(4, secondScenario.Steps.Count);
            AssertStep(secondScenario.Steps[0], StepType.Given, "there are 20 cucumbers");
            AssertStep(secondScenario.Steps[1], StepType.Given, "the following users exist:");
            AssertStep(secondScenario.Steps[2], StepType.When, "I eat 5 cucumbers");
            AssertStep(secondScenario.Steps[3], StepType.Then, "I should have 15 cucumbers");
        }

        void AssertStep(Step step, StepType expectedStepType, string expectedText)
        {
            Assert.AreEqual(expectedStepType, step.StepType);
            Assert.AreEqual(expectedText, step.Text);
        }
    }
}