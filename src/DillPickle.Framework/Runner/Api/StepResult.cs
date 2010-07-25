namespace DillPickle.Framework.Runner.Api
{
    public class StepResult
    {
        public StepResult(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }
        public Result Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}