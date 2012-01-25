using System.Collections.Generic;

namespace DillPickle.Framework.Parser.Api
{
    public class ExecutableScenario : Scenario
    {
        public ExecutableScenario(string headline, IEnumerable<string> scenarioTags) : base(headline, scenarioTags)
        {
        }

        public override List<ExecutableScenario> GetExecutableScenarios()
        {
            return new List<ExecutableScenario> {this};
        }
    }
}