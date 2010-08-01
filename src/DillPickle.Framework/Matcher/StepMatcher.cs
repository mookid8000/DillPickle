using System;
using System.Collections.Generic;
using System.Linq;
using DillPickle.Framework.Executor;
using DillPickle.Framework.Parser;

namespace DillPickle.Framework.Matcher
{
    public class StepMatcher
    {
        public StepMatch GetMatch(Step step, ActionStepMethod stepMethod)
        {
            if (step.StepType != stepMethod.StepType) return StepMatch.NoMatch(step);

            var stepTokens = Tokenize(step.Text);
            var methodTokens = Tokenize(stepMethod.Text);

            if (stepTokens.Length != methodTokens.Length) return StepMatch.NoMatch(step);

            var tokens = new List<Token>();

            for (var index = 0; index < stepTokens.Length; index++)
            {
                var tok1 = stepTokens[index];
                var tok2 = methodTokens[index];

                if (tok2 is VariableToken)
                {
                    ((VariableToken) tok2).Value = tok1.Text;
                    tokens.Add(tok2);
                    continue;
                }

                if (tok1.Text != tok2.Text)
                {
                    return StepMatch.NoMatch(step);
                }

                tokens.Add(tok1);
            }

            return new StepMatch(step) {IsMatch = true, Tokens = tokens};
        }

        Token[] Tokenize(string text)
        {
            var sections = text.Split('"');
            var tokens = new List<Token>();

            for (var index = 0; index < sections.Length; index++)
            {
                var section = sections[index];

                if (index%2 == 0)
                {
                    tokens.AddRange(section.Split(" ,;.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => CreateToken(s)));
                }
                else
                {
                    tokens.Add(CreateToken(section));
                }
            }

            return tokens.ToArray();
        }

        Token CreateToken(string s)
        {
            if (s.StartsWith("$"))
            {
                return new VariableToken(s);
            }

            return new StringToken(s);
        }
    }
}