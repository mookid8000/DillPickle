using System;
using DillPickle.Framework.Parser.Api;

namespace DillPickle.Framework.Runner.Api
{
    ///<summary>
    /// Encapsultates logic to run one single feature
    ///</summary>
    public interface IFeatureRunner
    {
        FeatureResult Run(Feature feature, Type[] types, TagFilter filter);
        void AddListener(IListener listener);
    }
}