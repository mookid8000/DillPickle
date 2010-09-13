using System;

namespace DillPickle.Framework.Runner.Api
{
    public interface IActionStepsFinder
    {
        Type[] FindTypesWithActionSteps(string assemblyPath);
    }
}