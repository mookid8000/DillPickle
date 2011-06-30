using System;
using System.Collections.Generic;
using DillPickle.Framework.Runner;

namespace DillPickle.Framework.Parser.Api
{
    public abstract class Scenario
    {
        readonly string headline;
        readonly List<Step> steps = new List<Step>();
        readonly List<string> tags = new List<string>();

        protected Scenario(string headline, IEnumerable<string> scenarioTags)
        {
            this.headline = headline;
            tags.AddRange(scenarioTags);
        }

        public string Headline
        {
            get { return headline; }
        }

        public List<Step> Steps
        {
            get { return steps; }
        }

        public List<string> Tags
        {
            get { return tags; }
        }

        public abstract List<ExecutableScenario> GetExecutableScenarios();

        public bool ShouldBeIncluded(TagFilter filter)
        {
            return filter.Includes(Tags)
                   && !filter.Excludes(Tags);
        }
    }
}