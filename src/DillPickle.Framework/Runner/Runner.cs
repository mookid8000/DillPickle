using System;
using System.Collections.Generic;
using System.Linq;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class Runner
    {
        readonly IFeatureRunner featureRunner;

        public Runner(IFeatureRunner featureRunner)
        {
            this.featureRunner = featureRunner;
        }

        public List<FeatureResult> Run(Feature[] features, Type[] types)
        {
            var results = new List<FeatureResult>();

            foreach (Feature feature in features)
            {
                results.Add(featureRunner.Run(feature, types));
            }

            return results;
        }
    }

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

    public class StepResult
    {
        public StepResult(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }
        public Result Result { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum Result
    {
        Failed,
        Pending,
        Success,
    }
}