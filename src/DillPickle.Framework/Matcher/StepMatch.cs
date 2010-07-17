using System.Collections.Generic;
using DillPickle.Framework.Parser;

namespace DillPickle.Framework.Matcher
{
    public class StepMatch
    {
        public StepMatch(Step step)
        {
            Step = step;
        }

        public List<Token> Tokens { get; set; }
        public bool IsMatch { get; set; }
        public Step Step { get; private set; }

        public static StepMatch NoMatch(Step step)
        {
            return new StepMatch(step)
                       {
                           IsMatch = false,
                       };
        }
    }
}