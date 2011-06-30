using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Parser.Api;

namespace DillPickle.Framework.Parser
{
    /// <summary>
    /// Encapsulates the parsing of Gherkin-text into a (pseudo-) AST.
    /// 
    /// OMG this parser is just getting uglier and uglier. 
    /// 
    /// It's a good thing I have unit tests.
    /// 
    /// Some day I'll replace this abominable class with something else, like e.g. an ANTLR-based parser or something.
    /// </summary>
    public class StupidGherkinParser : IGherkinParser
    {
        const StringComparison Comparison = StringComparison.CurrentCultureIgnoreCase;
        const string FeatureIntroduction = "feature:";
        const string ScenarioIntroduction = "scenario:";
        const string ScenarioOutlineIntroduction = "scenario outline:";
        const string ExamplesIntroduction = "examples:";

        ///<summary>
        /// Call this function to parse a piece of text whose file origin is unknown or not available.
        ///</summary>
        ///<param name="text">Gherkin-text to parse</param>
        ///<returns>A parse result including the AST</returns>
        public ParseResult Parse(string text)
        {
            return Parse("n/a", text);
        }

        ///<summary>
        /// Call this function to parse a piece of text, specifying its file origin (which will generate better error messages).
        ///</summary>
        ///<param name="fileName">Name of file origin of the Gherkin-text</param>
        ///<param name="text">Gherkin-text to parse</param>
        ///<returns>A parse result including the AST</returns>
        public ParseResult Parse(string fileName, string text)
        {
            var features = new List<Feature>();

            using (var reader = new StringReader(text))
            {
                var accumulatedTags = new List<string>();

                string line;

                Feature currentFeature = null;
                Scenario currentScenario = null;
                StepType? mostRecentStepType = null;
                var tableColumnNames = new List<string>();
                var parsingExamples = false;
                var parsingBackground = false;

                var lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    lineNumber++;

                    if (ShouldBeIgnored(line))
                    {
                        continue;
                    }

                    if (line.StartsWith("@"))
                    {
                        var tokens = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        var invalidTag = tokens.FirstOrDefault(t => !t.StartsWith("@"));
                        
                        if (invalidTag != null)
                        {
                            throw new GherkinParseException(fileName, lineNumber, line,
                                                            "'{0}' is not a valid tag - tags must begin with '@', e.g. '@important'",
                                                            invalidTag);
                        }

                        var tags = tokens.Select(t => t.TrimStart('@'));

                        accumulatedTags.AddRange(tags);

                        continue;
                    }

                    if (line.StartsWith(FeatureIntroduction, Comparison))
                    {
                        var featureText = line.Substring(FeatureIntroduction.Length).Trim();
                        currentFeature = new Feature(featureText, accumulatedTags);
                        accumulatedTags.Clear();
                        features.Add(currentFeature);
                        currentScenario = null;
                        mostRecentStepType = null;
                        continue;
                    }

                    if (currentFeature == null)
                    {
                        currentFeature = Feature.NewAnonymousFeature(accumulatedTags);
                        accumulatedTags.Clear();
                        features.Add(currentFeature);
                    }

                    if (line.StartsWith("Background:", Comparison) )
                    {
                        parsingBackground = true;
                        continue;
                    }

                    if (line.StartsWith(ScenarioIntroduction, Comparison))
                    {
                        var scenarioText = line.Substring(line.IndexOf(":") + 1).Trim();
                        currentScenario = new ExecutableScenario(scenarioText, accumulatedTags);
                        accumulatedTags.Clear();
                        tableColumnNames.Clear();
                        currentFeature.Scenarios.Add(currentScenario);
                        mostRecentStepType = null;
                        parsingExamples = parsingBackground = false;
                        continue;
                    }

                    if (line.StartsWith(ScenarioOutlineIntroduction, Comparison))
                    {
                        var scenarioText = line.Substring(line.IndexOf(":") + 1).Trim();
                        currentScenario = new ScenarioOutline(scenarioText, accumulatedTags);
                        accumulatedTags.Clear();
                        tableColumnNames.Clear();
                        currentFeature.Scenarios.Add(currentScenario);
                        mostRecentStepType = null;
                        parsingExamples = parsingBackground = false;
                        continue;
                    }

                    if (line.StartsWith(ExamplesIntroduction, Comparison))
                    {
                        if (!(currentScenario is ScenarioOutline))
                        {
                            throw new GherkinParseException(fileName, lineNumber, line, "Cannot specify examples in an ordinary scenario. Please use the 'Scenario outline:' introduction if you mean to specify examples");
                        }

                        parsingExamples = true;
                        
                        continue;
                    }

                    if (parsingBackground)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        if (line.StartsWith("and", Comparison))
                        {
                            if (mostRecentStepType == null)
                            {
                                throw new GherkinParseException(fileName, lineNumber, line,
                                                                @"Lines can only be introduced with ""and"" when it's preceded by either ""given"", ""when"", or ""then"".");
                            }

                            currentFeature.BackgroundSteps.Add(Step.And(line.Substring("and".Length).Trim(),
                                                               mostRecentStepType.Value));
                        }
                        else if (line.StartsWith("given", Comparison))
                        {
                            currentFeature.BackgroundSteps.Add(Step.Given(line.Substring("given".Length).Trim()));
                            mostRecentStepType = StepType.Given;
                            tableColumnNames.Clear();
                        }
                        else if (line.StartsWith("|"))
                        {
                            var tokens = line.Split('|').Select(s => s.Trim()).ToArray();

                            if (!tableColumnNames.Any())
                            {
                                tableColumnNames.AddRange(tokens);
                            }
                            else
                            {
                                var dict = new Dictionary<string, string>();

                                for (var index = 0; index < tokens.Length; index++)
                                {
                                    var key = tableColumnNames[index];
                                    if (string.IsNullOrEmpty(key)) continue;

                                    dict[key] = tokens[index];
                                }

                                currentFeature.BackgroundSteps.Last().Parameters.Add(dict);
                            }
                        }
                        else
                        {
                            throw new GherkinParseException(fileName, lineNumber, line,
                                                            @"Expected line to start with either ""given"", or ""|"". Please note that ""when"" or ""then"" steps may not appear inside the background element.");
                        }

                        continue;
                    }

                    if (currentScenario != null)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        if (line.StartsWith("and", Comparison))
                        {
                            if (mostRecentStepType == null)
                            {
                                throw new GherkinParseException(fileName, lineNumber, line,
                                                                @"Lines can only be introduced with ""and"" when it's preceded by either ""given"", ""when"", ""then"", or ""|"".");
                            }

                            currentScenario.Steps.Add(Step.And(line.Substring("and".Length).Trim(),
                                                               mostRecentStepType.Value));
                            tableColumnNames.Clear();
                        }
                        else if (line.StartsWith("given", Comparison))
                        {
                            currentScenario.Steps.Add(Step.Given(line.Substring("given".Length).Trim()));
                            mostRecentStepType = StepType.Given;
                            tableColumnNames.Clear();
                        }
                        else if (line.StartsWith("when", Comparison))
                        {
                            currentScenario.Steps.Add(Step.When(line.Substring("when".Length).Trim()));
                            mostRecentStepType = StepType.When;
                            tableColumnNames.Clear();
                        }
                        else if (line.StartsWith("then", Comparison))
                        {
                            currentScenario.Steps.Add(Step.Then(line.Substring("then".Length).Trim()));
                            mostRecentStepType = StepType.Then;
                            tableColumnNames.Clear();
                        }
                        else if (line.StartsWith("|"))
                        {
                            var tokens = line.Split('|').Select(s => s.Trim()).ToArray();

                            if (!tableColumnNames.Any())
                            {
                                tableColumnNames.AddRange(tokens);
                            }
                            else
                            {
                                var dict = new Dictionary<string, string>();

                                for(var index = 0; index < tokens.Length; index++)
                                {
                                    var key = tableColumnNames[index];
                                    if (string.IsNullOrEmpty(key)) continue;
                                    
                                    dict[key] = tokens[index];
                                }

                                if (parsingExamples)
                                {
                                    ((ScenarioOutline)currentScenario).Examples.Add(dict);
                                }
                                else
                                {
                                    currentScenario.Steps.Last().Parameters.Add(dict);
                                }
                            }
                        }
                        else
                        {
                            throw new GherkinParseException(fileName, lineNumber, line,
                                                            @"Expected line to start with either ""given"", ""when"", or ""then"".");
                        }

                        continue;
                    }

                    currentFeature.AddText(line);
                    continue;
                }
            }

            features.ForEach(feature => AssertConsistency(feature, fileName));

            return new ParseResult(features);
        }

        private bool ShouldBeIgnored(string line)
        {
            return string.IsNullOrEmpty(line) || line.StartsWith("#");
        }

        void AssertConsistency(Feature feature, string fileName)
        {
            feature.Scenarios.ForEach(scenario => AssertConsistency(scenario, fileName));
        }

        void AssertConsistency(Scenario scenario, string fileName)
        {
            if (scenario is ScenarioOutline)
            {
                var outline = (ScenarioOutline)scenario;

                AssertThatExamplesAreGiven(outline, fileName);
                AssertThatPlaceholdersCanBeResolvedWithThisTable(outline, fileName);
            }
        }

        void AssertThatExamplesAreGiven(ScenarioOutline outline, string fileName)
        {
            if (outline.Examples.Count == 0)
            {
                throw new GherkinParseException(fileName, "In a scenario outline, a table of examples should be given");
            }
        }

        void AssertThatPlaceholdersCanBeResolvedWithThisTable(ScenarioOutline scenario, string fileName)
        {
            var placeholderMatcher = new Regex("<(.)*>");

            var matches = scenario.Steps.SelectMany(s => placeholderMatcher.Matches(s.Text).Cast<Match>());

            foreach(var match in matches.Select(m => m.Value.Substring(1, m.Value.Length - 2)))
            {
                var key = match;
                
                if (!scenario.Examples.Any(d => d.ContainsKey(key)))
                {
                    throw new GherkinParseException(fileName, "The placeholder '{0}' has no matching column in the table of examples", key);
                }
            }
        }
    }
}