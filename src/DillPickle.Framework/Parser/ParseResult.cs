using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Parser
{
    public class ParseResult
    {
        public ParseResult(IEnumerable<Feature> features)
        {
            Features = features.ToList();
        }

        public List<Feature> Features { get; set; }
    }
}