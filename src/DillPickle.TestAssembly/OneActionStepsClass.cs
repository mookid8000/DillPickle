using System;
using DillPickle.Framework.Executor.Attributes;

namespace DillPickle.TestAssembly
{
    [ActionSteps]
    public class OneActionStepsClass : IDisposable
    {
        #region IDisposable Members

        public void Dispose()
        {
            Console.WriteLine("DISPOSE!");
        }

        #endregion

        [Given("I am pretty $condition")]
        public void GivenPrettyTired(string condition)
        {
            Console.WriteLine("My condition is: {0}", condition);
        }

        [When("I go to bed")]
        public void GoToBed()
        {
            Console.WriteLine("I go to bed");
        }

        [Then("I will $action almost immediately")]
        public void ActionImmediately(string action)
        {
            Console.WriteLine("{0}ing immediately", action);
        }

        [Then("I will sleet even more")]
        public void WillSleepMore()
        {
            throw new InvalidOperationException("something bad just happened!");
        }
    }
}