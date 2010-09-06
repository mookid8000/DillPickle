using System;
using System.Collections.Generic;
using System.Linq;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class DefaultRunner
    {
        readonly IFeatureRunner featureRunner;

        public DefaultRunner(IFeatureRunner featureRunner)
        {
            this.featureRunner = featureRunner;
        }

        public List<FeatureResult> Run(Feature[] features, Type[] types)
        {
            return features
                .Select(feature => featureRunner.Run(feature, types))
                .ToList();
        }
    }
}