using DillPickle.Framework.Executor.Attributes.Base;

namespace DillPickle.Framework.Executor.Attributes
{
    public class ThenAttribute : StepAttribute
    {
        public ThenAttribute(string text)
            : base(text)
        {
        }
    }
}