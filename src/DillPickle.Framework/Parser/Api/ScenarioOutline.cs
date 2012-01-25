using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Parser.Api
{
    public class ScenarioOutline : Scenario
    {
        public ScenarioOutline(string headline, IEnumerable<string> scenarioTags) : base(headline, scenarioTags)
        {
            Examples = new List<Dictionary<string, string>>();
        }

        public List<Dictionary<string, string>> Examples { get; set; }

        public override List<ExecutableScenario> GetExecutableScenarios()
        {
            return Examples.Select(GenerateExecutableScenarioFor).ToList();
        }

        ExecutableScenario GenerateExecutableScenarioFor(Dictionary<string, string> dictionary)
        {
            var scenario = new ExecutableScenario(Headline, Tags);
            scenario.Steps.AddRange(Steps.Select(step => GenerateExecutableStep(step, dictionary)));
            return scenario;
        }

        Step GenerateExecutableStep(Step step, Dictionary<string, string> dictionary)
        {
            return step.SubstituteAndClone(dictionary);
        }
    }
}