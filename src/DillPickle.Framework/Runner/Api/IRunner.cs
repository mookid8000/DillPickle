using System;
using System.Collections.Generic;
using DillPickle.Framework.Parser.Api;

namespace DillPickle.Framework.Runner.Api
{
    public interface IRunner
    {
        List<FeatureResult> Run(Feature[] features, Type[] types);
    }
}