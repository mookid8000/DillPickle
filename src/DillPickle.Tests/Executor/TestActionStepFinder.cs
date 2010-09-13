using System.Collections.Generic;
using DillPickle.Framework.Parser.Api;
using NUnit.Framework;
using DillPickle.Framework.Executor;
using DillPickle.Framework.Executor.Attributes;

namespace DillPickle.Tests.Executor
{
    [TestFixture]
    public class TestActionStepFinder : FixtureBase
    {
        ActionStepFinder finder;

        public override void DoSetUp()
        {
            finder = new ActionStepFinder();
        }

        void AssertStepMethod(ActionStepMethod method, StepType expectedStepType, string expectedText)
        {
            Assert.AreEqual(expectedStepType, method.StepType);
            Assert.AreEqual(expectedText, method.Text);
        }


        [ActionSteps]
        class HasSomeSteps
        {
            [Given("some precondition")]
            public void GivenSomePrecondition()
            {
            }

            [Given("another precondition")]
            public void GivenAnotherPrecondition()
            {
            }

            [When("an actor does some stuff")]
            public void WhenAnActorDoesSomeStuff()
            {
            }

            [When("an actor does some more stuff")]
            public void WhenAnActorDoesSomeMoreStuff()
            {
            }

            [Then("something weird happens")]
            public void ThenSomethingWeirdHappens()
            {
            }

            [Then("something even more weird happens")]
            public void ThenSomethingEvenMoreWeirdHappens()
            {
            }
        }

        [Test]
        public void CanFindActionsAsExpected()
        {
            List<ActionStepsClass> classes = finder.Find(typeof (HasSomeSteps));

            Assert.AreEqual(1, classes.Count);
            ActionStepsClass c = classes[0];

            List<ActionStepMethod> methods = c.ActionStepMethods;

            Assert.AreEqual(6, methods.Count);
            AssertStepMethod(methods[0], StepType.Given, "some precondition");
            AssertStepMethod(methods[1], StepType.Given, "another precondition");
            AssertStepMethod(methods[2], StepType.When, "an actor does some stuff");
            AssertStepMethod(methods[3], StepType.When, "an actor does some more stuff");
            AssertStepMethod(methods[4], StepType.Then, "something weird happens");
            AssertStepMethod(methods[5], StepType.Then, "something even more weird happens");
        }
    }
}