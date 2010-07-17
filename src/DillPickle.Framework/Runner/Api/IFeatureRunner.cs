using System;
using DillPickle.Framework.Parser;

namespace DillPickle.Framework.Runner.Api
{
    public interface IFeatureRunner
    {
        FeatureResult Run(Feature feature, Type[] types);
    }
}