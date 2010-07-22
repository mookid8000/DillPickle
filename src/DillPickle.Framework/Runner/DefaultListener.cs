using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class DefaultListener : IListener
    {
        public virtual void BeforeFeature(Feature feature)
        {
        }

        public virtual void BeforeScenario(Feature feature, Scenario scenario)
        {
        }

        public virtual void BeforeStep(Feature feature, Scenario scenario, Step step)
        {
        }

        public virtual void AfterStep(Feature feature, Scenario scenario, Step step, StepResult result)
        {
        }

        public virtual void AfterScenario(Feature feature, Scenario scenario, ScenarioResult result)
        {
        }

        public virtual void AfterFeature(Feature feature, FeatureResult result)
        {
        }
    }
}