using System;
using DillPickle.Framework.Parser.Api;

namespace DillPickle.Framework.Runner.Api
{
    ///<summary>
    /// Encapsultates logic to run one single feature
    ///</summary>
    public interface IFeatureRunner
    {
        FeatureResult Run(Feature feature, Type[] types, RunnerOptions filter);
        void AddListener(IListener listener);
        void Commission();
        void Decommission();
    }
}