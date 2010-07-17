using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DillPickle.Framework.Parser
{
    public class GherkinParser
    {
        const StringComparison Comparison = StringComparison.CurrentCultureIgnoreCase;
        const string FeatureIntroduction = "feature:";
        const string ScenarioIntroduction = "scenario:";

        public ParseResult Parse(string text)
        {
            return Parse("n/a", text);
        }

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

                int lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    lineNumber++;

                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    if (line.StartsWith("@"))
                    {
                        string[] tokens = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string invalidTag = tokens.FirstOrDefault(t => !t.StartsWith("@"));
                        if (invalidTag != null)
                        {
                            throw new GherkinParseException(fileName, lineNumber, line,
                                                            "'{0}' is not a valid tag - tags must begin with '@', e.g. '@important'",
                                                            invalidTag);
                        }

                        IEnumerable<string> tags = tokens.Select(t => t.TrimStart('@'));

                        accumulatedTags.AddRange(tags);

                        continue;
                    }

                    if (line.StartsWith(FeatureIntroduction, Comparison))
                    {
                        string featureText = line.Substring(FeatureIntroduction.Length).Trim();
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

                    if (line.StartsWith(ScenarioIntroduction, Comparison))
                    {
                        string scenarioText = line.Substring(ScenarioIntroduction.Length).Trim();
                        currentScenario = new Scenario(scenarioText, accumulatedTags.Concat(currentFeature.Tags));
                        accumulatedTags.Clear();
                        currentFeature.Scenarios.Add(currentScenario);
                        mostRecentStepType = null;
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
                                                                @"Lines can only be introduced with ""and"" when it's preceded by either ""given"", ""when"", or ""then"".");
                            }

                            currentScenario.Steps.Add(Step.And(line.Substring("and".Length).Trim(),
                                                               mostRecentStepType.Value));
                        }
                        else if (line.StartsWith("given", Comparison))
                        {
                            currentScenario.Steps.Add(Step.Given(line.Substring("given".Length).Trim()));
                            mostRecentStepType = StepType.Given;
                        }
                        else if (line.StartsWith("when", Comparison))
                        {
                            currentScenario.Steps.Add(Step.When(line.Substring("when".Length).Trim()));
                            mostRecentStepType = StepType.When;
                        }
                        else if (line.StartsWith("then", Comparison))
                        {
                            currentScenario.Steps.Add(Step.Then(line.Substring("then".Length).Trim()));
                            mostRecentStepType = StepType.Then;
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

            return new ParseResult(features);
        }
    }
}