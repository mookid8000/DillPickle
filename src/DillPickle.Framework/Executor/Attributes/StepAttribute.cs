using System;

namespace DillPickle.Framework.Executor.Attributes
{
    public class StepAttribute : Attribute
    {
        readonly string text;

        protected StepAttribute(string text)
        {
            this.text = text;
        }

        public string Text
        {
            get { return text; }
        }
    }
}