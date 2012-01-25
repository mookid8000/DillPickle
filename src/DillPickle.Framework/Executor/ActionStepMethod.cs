using System.Reflection;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Executor.Attributes.Base;
using DillPickle.Framework.Parser.Api;

namespace DillPickle.Framework.Executor
{
    public class ActionStepMethod
    {
        readonly MethodInfo methodInfo;
        readonly StepType stepType;
        readonly string text;

        public ActionStepMethod(MethodInfo info, StepAttribute attribute)
        {
            if (attribute is GivenAttribute)
            {
                stepType = StepType.Given;
            }

            if (attribute is WhenAttribute)
            {
                stepType = StepType.When;
            }

            if (attribute is ThenAttribute)
            {
                stepType = StepType.Then;
            }

            text = attribute.Text;
            methodInfo = info;
        }

        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        public StepType StepType
        {
            get { return stepType; }
        }

        public string Text
        {
            get { return text; }
        }
    }
}