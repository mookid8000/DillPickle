using System;
using DillPickle.Framework.Parser.Api;

namespace DillPickle.Framework.Runner.Api
{
    public interface IFeatureRunner
    {
        FeatureResult Run(Feature feature, Type[] types);
        void AddListener(IListener listener);
    }
}