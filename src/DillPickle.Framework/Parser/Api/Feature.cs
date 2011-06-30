using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DillPickle.Framework.Runner;

namespace DillPickle.Framework.Parser.Api
{
    public class Feature
    {
        readonly string headline;
        readonly List<Scenario> scenarios = new List<Scenario>();
        readonly StringBuilder storyText = new StringBuilder();
        readonly List<string> tags = new List<string>();
        readonly List<Step> backgroundSteps = new List<Step>();

        public Feature(string headline, IEnumerable<string> featureTags)
        {
            this.headline = headline;
            tags.AddRange(featureTags);
        }

        public List<Scenario> Scenarios
        {
            get { return scenarios; }
        }

        public string Headline
        {
            get { return headline; }
        }

        public string Description
        {
            get { return storyText.ToString(); }
        }

        public List<string> Tags
        {
            get { return tags; }
        }

        public List<Step> BackgroundSteps
        {
            get { return backgroundSteps; }
        }

        public static Feature NewAnonymousFeature(List<string> accumulatedTags)
        {
            return new Feature("", accumulatedTags);
        }

        public void AddText(string line)
        {
            storyText.AppendLine(line);
        }

        public override string ToString()
        {
            return string.Format("Feature: {0} ({1})", headline, tags.JoinToString(","));
        }

        public bool ShouldBeIncluded(TagFilter filter)
        {
            return (filter.Includes(Tags) || Scenarios.Any(s => s.ShouldBeIncluded(filter)))
                   && !filter.Excludes(Tags);
        }
    }
}