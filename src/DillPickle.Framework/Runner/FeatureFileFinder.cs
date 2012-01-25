using System;
using System.Collections.Generic;
using System.IO;
using DillPickle.Framework.Exceptions;
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

            var directory = Path.GetDirectoryName(featurePattern);
            var filePattern = Path.GetFileName(featurePattern);

            if (!Directory.Exists(directory))
            {
                throw new FeatureExecutionException("Directory {0} does not exist", directory);
            }

            return Directory.GetFiles(directory, filePattern);
        }
    }
}