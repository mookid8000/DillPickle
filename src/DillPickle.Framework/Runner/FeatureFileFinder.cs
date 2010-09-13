using System;
using System.Collections.Generic;
using System.IO;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class FeatureFileFinder : IFeatureFileFinder
    {
        public IEnumerable<string> Find(string featurePattern)
        {
            if (!Path.IsPathRooted(featurePattern))
            {
                featurePattern = Path.Combine(Environment.CurrentDirectory, featurePattern);
            }

            return Directory.GetFiles(Path.GetDirectoryName(featurePattern), Path.GetFileName(featurePattern));
        }
    }
}