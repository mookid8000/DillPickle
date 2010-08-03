using DillPickle.Framework.Executor.Attributes.Base;

namespace DillPickle.Framework.Executor.Attributes
{
    public class GivenAttribute : StepAttribute
    {
        public GivenAttribute(string text)
            : base(text)
        {
        }
    }
}