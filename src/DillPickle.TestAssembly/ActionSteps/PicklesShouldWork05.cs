using DillPickle.Framework.Executor.Attributes;

namespace DillPickle.TestAssembly.ActionSteps
{
    [ActionSteps]
    [IncludeActionSteps(typeof(OneActionStepsClass), typeof(AnotherActionStepsClass), typeof(ThirdActionStepsClass), typeof(CalculatorActionSteps))]
    public class PicklesShouldWork05
    {
        
    }
}