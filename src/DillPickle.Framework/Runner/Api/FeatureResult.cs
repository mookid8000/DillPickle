using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Runner.Api
{
    public class FeatureResult
    {
        public FeatureResult()
        {
            ScenarioResults = new List<ScenarioResult>();
        }

        public string Headline { get; set; }
        public string Description { get; set; }
        public List<ScenarioResult> ScenarioResults { get; set; }

        public Result Result
        {
            get
            {
                if (ScenarioResults.Any(r => r.Result == Result.Failed))
                    return Result.Failed;

                if (ScenarioResults.Any(r => r.Result == Result.Pending))
                    return Result.Pending;

                return Result.Success;
            }
        }
    }
}