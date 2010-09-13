using DillPickle.Framework.Parser.Api;
using NUnit.Framework;
using DillPickle.Framework.Executor;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Matcher;

namespace DillPickle.Tests.Matcher
{
    [TestFixture]
    public class TestStepMatcher : FixtureBase
    {
        StepMatcher matcher;

        public override void DoSetUp()
        {
            matcher = new StepMatcher();
        }

        void AssertVar(Token token, string expectedVariableName, string expectedValue)
        {
            var varToken = (VariableToken) token;
            Assert.AreEqual(expectedVariableName, varToken.Name);
            Assert.AreEqual(expectedValue, varToken.Value);
        }

        [Test]
        public void CanMatchStepsAndStuff()
        {
            var step = Step.Given("i am logged in as an administrator");
            var method = new ActionStepMethod(null, new GivenAttribute("i am logged in as an administrator"));

            Assert.IsTrue(matcher.GetMatch(step, method).IsMatch);
        }

        [Test]
        public void CanMatchWhenMultipleParametersAreInvolved()
        {
            var step = Step.Given("i click BigButton 10 times frantically");
            var method = new ActionStepMethod(null, new GivenAttribute("i click $controlName $count times $how"));

            var match = matcher.GetMatch(step, method);

            Assert.IsTrue(match.IsMatch);

            AssertVar(match.Tokens[2], "controlName", "BigButton");
            AssertVar(match.Tokens[3], "count", "10");
            AssertVar(match.Tokens[5], "how", "frantically");
        }

        [Test]
        public void CanMatchWhenSimpleParameterIsInvolved()
        {
            var step = Step.Given("i click 5 times");
            var method = new ActionStepMethod(null, new GivenAttribute("i click $count times"));

            var match = matcher.GetMatch(step, method);

            Assert.IsTrue(match.IsMatch);
            var variableToken = (VariableToken) match.Tokens[2];
            Assert.AreEqual("count", variableToken.Name);
            Assert.AreEqual("5", variableToken.Value);
        }

        [Test]
        public void DoesNotMatchWhenTextIsDifferent()
        {
            var step = Step.Given("i am logged in as an administrator");
            var method = new ActionStepMethod(null, new GivenAttribute("i am logged in as a administrator"));

            Assert.IsFalse(matcher.GetMatch(step, method).IsMatch);
        }

        [Test]
        public void DoesNotMatchStepsOfDifferentTypes()
        {
            var step = Step.Given("i am logged in as an administrator");
            var method = new ActionStepMethod(null, new ThenAttribute("i am logged in as an administrator"));

            Assert.IsFalse(matcher.GetMatch(step, method).IsMatch);
        }

        [Test]
        public void WorksWithMultipleQuotedAndNonQuotedStringIsAnExtremelyComplexSetUpThatReallyShouldNeverHappen()
        {
            var step = Step.Given(@"i am punching ""hello there, my name is Joe!"""
                                  + @" into Word2, which actually was better than"
                                  + @" WordPerfect - mostly because of ""WYSIWYG"", though");

            var attr = new GivenAttribute(@"i am punching ""$g"""
                                          + @" into $app, which actually was better than"
                                          + @" $app2 - mostly because of ""$why"", though");

            var method = new ActionStepMethod(null, attr);

            var match = matcher.GetMatch(step, method);

            Assert.IsTrue(match.IsMatch);
            AssertVar(match.Tokens[3], "g", "hello there, my name is Joe!");
            AssertVar(match.Tokens[5], "app", "Word2");
            AssertVar(match.Tokens[11], "app2", "WordPerfect");
            AssertVar(match.Tokens[16], "why", "WYSIWYG");
        }

        [Test]
        public void WorksWithQuotedVariablesAndValuesAsWell()
        {
            var step = Step.Given(@"i type ""hello there"" in TextBoxGreeting");
            var method = new ActionStepMethod(null, new GivenAttribute(@"i type ""$secretText"" in $textBoxName"));

            var match = matcher.GetMatch(step, method);

            Assert.IsTrue(match.IsMatch);
            AssertVar(match.Tokens[2], "secretText", "hello there");
            AssertVar(match.Tokens[4], "textBoxName", "TextBoxGreeting");
        }
    }
}