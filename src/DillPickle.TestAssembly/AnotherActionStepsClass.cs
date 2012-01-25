using System;
using DillPickle.Framework.Executor.Attributes;

namespace DillPickle.TestAssembly
{
    [ActionSteps]
    public class AnotherActionStepsClass
    {
        [When("I dream of $what")]
        public void DreamOf(string what)
        {
            Console.WriteLine("Dreaming of {0}", what);
        }
    }
}