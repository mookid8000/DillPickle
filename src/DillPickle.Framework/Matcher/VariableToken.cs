namespace DillPickle.Framework.Matcher
{
    public class VariableToken : Token
    {
        public VariableToken(string text)
        {
            Text = text;
        }

        public string Name
        {
            get { return Text.TrimStart('$'); }
        }

        public string Value { get; set; }
    }
}