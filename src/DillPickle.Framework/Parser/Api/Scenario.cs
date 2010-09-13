using System.Collections.Generic;

namespace DillPickle.Framework.Parser.Api
{
    public abstract class Scenario
    {
        readonly string headline;
        readonly List<Step> steps = new List<Step>();
        readonly List<string> tags = new List<string>();

        protected Scenario(string headline, IEnumerable<string> accumulatedTags)
        {
            this.headline = headline;
            tags.AddRange(accumulatedTags);
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
    }
}