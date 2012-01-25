using System;
using System.Linq;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Runner;

namespace DillPickle.TestAssembly
{
    [ActionSteps]
    public class ThirdActionStepsClass
    {
        Child[] children;

        [Given("something like this:")]
        public void GivenSomethingLikeThis(Child[] children)
        {
            this.children = children;
        }

        [When(@"I say ""$text""")]
        public void WhenSaying(string text)
        {
            Console.WriteLine("!: {0}", text);
        }

        [Then("they go to bed - no questions asked :)")]
        public void ThenTheyGoToBed()
        {
            if (children.Length != 2) throw new ApplicationException();

            if (children.Single(c => c.Name == "Hugo").Age != TimeSpan.FromDays(365*4))
                throw new ApplicationException();

            if (children.Single(c => c.Name == "Niels").Age != TimeSpan.FromDays(30 * 11))
                throw new ApplicationException();
        }

        [TypeConverter]
        class AgeTypeConverter : ITypeConverter<TimeSpan>
        {
            public TimeSpan Convert(string value)
            {
                var tokens = value.Split(' ');

                var amount = tokens[0];
                var measure = tokens[1].ToLowerInvariant();

                switch (measure)
                {
                    case "months":
                        return TimeSpan.FromDays(30 * int.Parse(amount));

                    case "years":
                        return TimeSpan.FromDays(365 * int.Parse(amount));

                    default:
                        throw new FormatException(string.Format("{0} could not be interpreted as a valid age", value));
                }
            }
        }

        public class Child
        {
            public string Name { get; set; }
            public TimeSpan Age { get; set; }
        }
    }
}