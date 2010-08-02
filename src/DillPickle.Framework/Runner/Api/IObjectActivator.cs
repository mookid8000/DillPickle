using System;

namespace DillPickle.Framework.Runner.Api
{
    public interface IObjectActivator
    {
        object GetInstance(Type type);
    }
}