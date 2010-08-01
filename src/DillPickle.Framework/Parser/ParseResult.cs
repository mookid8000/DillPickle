using System.Collections.Generic;

namespace DillPickle.Framework.Parser
{
    public class ParseResult
    {
        readonly List<Feature> features = new List<Feature>();

        public ParseResult(IEnumerable<Feature> features)
        {
            this.features.AddRange(features);
        }

        public List<Feature> Features
        {
            get { return features; }
        }
    }
}