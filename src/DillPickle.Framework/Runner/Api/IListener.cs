using DillPickle.Framework.Parser.Api;

namespace DillPickle.Framework.Runner.Api
{
    public interface IListener
    {
        void Initialize();
        void BeforeFeature(Feature feature);
        void BeforeScenario(Feature feature, Scenario scenario);
        void BeforeStep(Feature feature, Scenario scenario, Step step);
        void AfterStep(Feature feature, Scenario scenario, Step step, StepResult result);
        void AfterScenario(Feature feature, Scenario scenario, ScenarioResult result);
        void AfterFeature(Feature feature, FeatureResult result);
        void Finalize();
    }
}