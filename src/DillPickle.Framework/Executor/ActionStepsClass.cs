using System;
using System.Collections.Generic;

namespace DillPickle.Framework.Executor
{
    public class ActionStepsClass
    {
        readonly List<ActionStepMethod> actionStepMethods = new List<ActionStepMethod>();
        readonly Type type;

        public ActionStepsClass(Type type)
        {
            this.type = type;
        }

        public Type Type
        {
            get { return type; }
        }

        public IEnumerable<ActionStepMethod> ActionStepMethods
        {
            get { return actionStepMethods; }
        }

        public void AddMethods(IEnumerable<ActionStepMethod> methodsToAdd)
        {
            actionStepMethods.AddRange(methodsToAdd);
        }
    }
}