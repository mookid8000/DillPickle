using System.Collections.Generic;

namespace DillPickle.Framework.Parser
{
    public class Step
    {
        Step(string text, string prefix)
        {
            Parameters = new List<Dictionary<string, string>>();
            Text = text;
            Prefix = prefix;
        }

        public string Text { get; set; }
        public string Prefix { get; set; }
        public StepType StepType { get; set; }
        public List<Dictionary<string, string>> Parameters { get; set; }

        public static Step And(string text, StepType previousStepType)
        {
            return new Step(text, "And") {StepType = previousStepType};
        }

        public static Step Given(string text)
        {
            return new Step(text, "Given") {StepType = StepType.Given};
        }

        public static Step When(string text)
        {
            return new Step(text, "When") {StepType = StepType.When};
        }

        public static Step Then(string text)
        {
            return new Step(text, "Then") {StepType = StepType.Then};
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", StepType, Text);
        }
    }
}