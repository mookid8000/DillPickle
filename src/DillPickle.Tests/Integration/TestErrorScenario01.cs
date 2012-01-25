using System;
using System.Collections.Generic;
using System.Linq;
using DillPickle.Framework.Executor.Attributes;
using NUnit.Framework;

namespace DillPickle.Tests.Integration
{
    [TestFixture]
    public class TestErrorScenario01 : IntegrationTestBase
    {
        [Test]
        public void ScenarioWithMultilineStepArgumentsAndParameters()
        {
            Run(
                @"
Feature: Show all notes
  As a user
  In order to view contents of entire system
  I want to be able to browse catalog of notes
  
  Scenario: Browse catalog
    Given the following notes exist:
      | artist                | title                   |
      | Josh Rouse            | Winter In The Hamptons  |
      | Bonnie ""Prince"" Billy | The Glory Goes          |
      | Martha Wainwright     | This Life               |
    When I go to /notes
    Then table Notes contains the following rows:
      | artist                | title                   |
      | Bonnie ""Prince"" Billy | The Glory Goes          |
      | Josh Rouse            | Winter In The Hamptons  |
      | Martha Wainwright     | This Life               |
",
                typeof (Steps));

            var expectedCalls =
                new[]
                    {
                        @"Given notes exist: Josh Rouse/Winter In The Hamptons,Bonnie ""Prince"" Billy/The Glory Goes,Martha Wainwright/This Life"
                        ,
                        @"When go to url: /notes",
                        @"Then table Notes contains: Bonnie ""Prince"" Billy/The Glory Goes,Josh Rouse/Winter In The Hamptons,Martha Wainwright/This Life"
                    };

            Assert.IsTrue(Steps.Calls.SequenceEqual(expectedCalls),
                          @"
Expected, but missing:
{0}

Unexpected:
{1}
",
                          expectedCalls.Except(Steps.Calls).JoinToString(Environment.NewLine),
                          Steps.Calls.Except(expectedCalls).JoinToString(Environment.NewLine));
        }

        [ActionSteps]
        class Steps
        {
            public static readonly List<string> Calls = new List<string>();

            void Add(string str)
            {
                Calls.Add(string.Intern(str));
            }

            [Given(@"the following notes exist:")]
            public void GivenNotesExist(NoteSpec[] noteSpecs)
            {
                Add(string.Format("Given notes exist: {0}",
                                        noteSpecs.Select(s => string.Format("{0}/{1}", s.Artist, s.Title)).JoinToString
                                            (",")));
            }

            [When(@"I go to $url")]
            public void WhenGoToUrl(string url)
            {
                Add(string.Format("When go to url: {0}", url));
            }

            [Then(@"table $tableId contains the following rows:")]
            public void ThenTableContains(string tableId, List<Dictionary<string, string>> rows)
            {
                Add(string.Format("Then table {0} contains: {1}", tableId,
                                        rows.Select(d => string.Format("{0}/{1}", d["artist"], d["title"])).
                                            JoinToString(",")));

                throw new ApplicationException("This string contains something, that can be evaluated to be a format string: {INVALID}");
            }
        }

        class NoteSpec
        {
            public string Artist { get; set; }
            public string Title { get; set; }
        }
    }
}