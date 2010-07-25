using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Runner.Api
{
    public class ScenarioResult
    {
        public ScenarioResult(string headline)
        {
            Headline = headline;
            StepResults = new List<StepResult>();
        }

        public string Headline { get; set; }
        public List<StepResult> StepResults { get; set; }

        public Result Result
        {
            get
            {
                if (StepResults.Any(r => r.Result == Result.Failed))
                    return Result.Failed;

                if (StepResults.Any(r => r.Result == Result.Pending))
                    return Result.Pending;

                return Result.Success;
            }
        }
    }
}