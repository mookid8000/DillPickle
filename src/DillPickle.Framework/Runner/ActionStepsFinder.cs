using System;
using System.Linq;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class ActionStepsFinder : IActionStepsFinder
    {
        readonly IAssemblyLoader assemblyLoader;

        public ActionStepsFinder(IAssemblyLoader assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader;
        }

        public Type[] FindTypesWithActionSteps(string assemblyPath)
        {
            var assembly = assemblyLoader.LoadAssemblyWithApplicationConfigurationIfPossible(assemblyPath);

            var actionStepsTypes = assembly.GetTypes()
                .Where(HasActionStepsAttribute)
                .ToArray();

            return actionStepsTypes;
        }

        bool HasActionStepsAttribute(Type t)
        {
            return t.GetCustomAttributes(typeof(ActionStepsAttribute), false).Any();
        }
    }
}