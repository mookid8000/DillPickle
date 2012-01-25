namespace DillPickle.Framework.Parser.Api
{
    public interface IGherkinParser
    {
        ParseResult Parse(string text);
        ParseResult Parse(string fileName, string text);
    }
}