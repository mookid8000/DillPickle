using System.Collections.Generic;

namespace DillPickle.Framework.Parser
{
    public class Scenario
    {
        readonly string headline;
        readonly List<Step> steps = new List<Step>();
        readonly List<string> tags = new List<string>();

        public Scenario(string headline, IEnumerable<string> accumulatedTags)
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
    }

    public class ScenarioOutline : Scenario
    {
        public ScenarioOutline(string headline, IEnumerable<string> accumulatedTags) : base(headline, accumulatedTags)
        {
            Examples = new List<Dictionary<string, string>>();
        }

        public List<Dictionary<string, string>> Examples { get; set; }
    }
}