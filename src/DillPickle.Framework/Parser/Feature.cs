using System;
using System.Collections.Generic;
using System.Text;

namespace DillPickle.Framework.Parser
{
    public class Feature
    {
        readonly string headline;
        readonly List<Scenario> scenarios = new List<Scenario>();
        readonly StringBuilder storyText = new StringBuilder();
        readonly List<string> tags = new List<string>();

        public Feature(string headline, IEnumerable<string> accumulatedTags)
        {
            this.headline = headline;
            tags.AddRange(accumulatedTags);
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
    }
}