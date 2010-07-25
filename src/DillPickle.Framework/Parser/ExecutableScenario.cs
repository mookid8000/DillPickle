using System.Collections.Generic;

namespace DillPickle.Framework.Parser
{
    public class ExecutableScenario : Scenario
    {
        public ExecutableScenario(string headline, IEnumerable<string> accumulatedTags) : base(headline, accumulatedTags)
        {
        }

        public override List<ExecutableScenario> GetExecutableScenarios()
        {
            return new List<ExecutableScenario> {this};
        }
    }
}