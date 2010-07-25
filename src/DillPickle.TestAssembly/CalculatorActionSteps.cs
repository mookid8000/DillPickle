using System;
using DillPickle.Framework.Executor.Attributes;

namespace DillPickle.TestAssembly
{
    [ActionSteps]
    public class CalculatorActionSteps
    {
        [Given("I type $number")]
        public void GivenTyping(int number)
        {
            Console.WriteLine("{0}", number);
        }

        [When("I press + followed by $number")]
        public void WhenPlussingNumber(int number)
        {
            Console.WriteLine("+ {0}", number);
        }

        [When("I press =")]
        public void WhenPressingEquals()
        {
            Console.WriteLine(" = ");
        }

        [Then("I see $number in the display")]
        public void ThenNumberIsDisplayed(int number)
        {
            Console.WriteLine("{0} !", number);
        }
    }
}