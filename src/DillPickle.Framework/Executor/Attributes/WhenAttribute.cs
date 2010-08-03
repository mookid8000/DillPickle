using DillPickle.Framework.Executor.Attributes.Base;

namespace DillPickle.Framework.Executor.Attributes
{
    public class WhenAttribute : StepAttribute
    {
        public WhenAttribute(string text)
            : base(text)
        {
        }
    }
}