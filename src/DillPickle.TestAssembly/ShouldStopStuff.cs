using System;
using DillPickle.Framework.Executor.Attributes;

namespace DillPickle.TestAssembly
{
    [ActionSteps]
    public class ShouldStopStuff
    {
        [Given("something that works")]
        public void GivenSomethingThatWorks()
        {
            Console.WriteLine("this works");
        }

        [When("something fails")]
        public void WhenSomethingFails()
        {
            throw new InvalidOperationException("this steps fails!!");
        }

        [Then("this should never be executed")]
        public void ThisShouldNeverBeExecuted()
        {
            Console.WriteLine("OMFG!!!");
        }
    }
}