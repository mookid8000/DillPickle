using System;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class TrivialObjectActivator : IObjectActivator
    {
        public object GetInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}