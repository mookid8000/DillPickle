using System;

namespace DillPickle.Framework.Executor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IncludeActionStepsAttribute : Attribute
    {
        readonly Type[] actionStepsTypesToInclude;

        public IncludeActionStepsAttribute(params Type[] actionStepsTypesToInclude)
        {
            this.actionStepsTypesToInclude = actionStepsTypesToInclude;
        }

        public Type[] ActionStepsTypesToInclude
        {
            get { return actionStepsTypesToInclude; }
        }
    }
}