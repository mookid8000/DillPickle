using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Executor.Attributes.Base;

namespace DillPickle.Framework.Executor
{
    public class ActionStepFinder
    {
        public List<ActionStepsClass> Find(Assembly assemblyToScan)
        {
            return Find(assemblyToScan.GetTypes());
        }

        public List<ActionStepsClass> Find(params Type[] typesToScan)
        {
            return typesToScan
                .Where(HasAttribute<ActionStepsAttribute>)
                .Select(t => CreateActionStepsClass(t))
                .ToList();
        }

        ActionStepsClass CreateActionStepsClass(Type type)
        {
            var actionStepMethods = type.GetMethods()
                .Where(HasAttribute<StepAttribute>)
                .SelectMany(m => m.GetCustomAttributes(typeof (StepAttribute), false)
                                     .Cast<StepAttribute>()
                                     .Select(a => CreateActionStepMethod(m, a)));

            var stepsClass = new ActionStepsClass(type);
            stepsClass.AddMethods(actionStepMethods);

            return stepsClass;
        }

        ActionStepMethod CreateActionStepMethod(MethodInfo info, StepAttribute stepAttribute)
        {
            return new ActionStepMethod(info, stepAttribute);
        }

        bool HasAttribute<T>(ICustomAttributeProvider type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof (T), false).Any();
        }
    }
}