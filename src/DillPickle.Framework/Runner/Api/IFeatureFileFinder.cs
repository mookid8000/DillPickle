using System.Collections.Generic;

namespace DillPickle.Framework.Runner.Api
{
    public interface IFeatureFileFinder
    {
        IEnumerable<string> Find(string featurePattern);
    }
}