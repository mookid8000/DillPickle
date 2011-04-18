using System;
using System.Collections.Generic;
using System.Linq;
using DillPickle.Framework.Extensions;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;
using DillPickle.Framework.Types;

namespace DillPickle.Framework.Listeners
{
    public abstract class DefaultTextOutputEventListener : DefaultListener
    {
        readonly List<string> errorMessages = new List<string>();

        public virtual bool ShowCurrentTimes { get; set; }

        public override void BeforeFeature(Feature feature)
        {
            WriteLine();

            if (feature.Tags.Any())
            {
                WriteLine(ConsoleColor.Cyan, 0, feature.Tags.Select(t => string.Format(@"@{0}", t)).JoinToString(" "));
            }

            WriteLine(ConsoleColor.Cyan, 0, @"Feature: {0}", feature.Headline);

            if (feature.Description.IsSet())
            {
                WriteLine(ConsoleColor.Cyan, 1, feature.Description);
            }
        }

        public override void BeforeScenario(Feature feature, Scenario scenario)
        {
            WriteLine();

            if (scenario.Tags.Any())
            {
                WriteLine(ConsoleColor.Cyan, 1, scenario.Tags.Select(t => string.Format(@"@{0}", t)).JoinToString(" "));
            }

            WriteLine(ConsoleColor.Cyan, 1, @"Scenario: {0}", scenario.Headline);
        }

        public override void AfterStep(Feature feature, Scenario scenario, Step step, StepResult result)
        {
            WriteLine(Color(result.Result), 2, "{0} {1}{2}{3}",
                      step.Prefix,
                      step.Text,
                      PossiblyTimes(),
                      PossiblyResult(result));

            if (step.Parameters.Any())
            {
                WriteParameters(result.Result, step.Parameters);
            }

            if (result.Result == Result.Failed)
            {
                errorMessages.Add(result.ErrorMessage);
            }
        }

        public override void AfterFeature(Feature feature, FeatureResult result)
        {
            if (!errorMessages.Any()) return;

            WriteLine();

            errorMessages.ForEach(msg => WriteLineRaw(ConsoleColor.Red, 2, msg));
            errorMessages.Clear();
        }

        public string PossiblyTimes()
        {
            return ShowCurrentTimes ? string.Format(" [{0}]", Time.Now().ToLocalTime().ToString("HH:mm:ss")) : "";
        }

        protected abstract void WriteLineRaw(ConsoleColor color, int tabs, string text);

        protected abstract void WriteLineRaw();

        string PossiblyResult(StepResult result)
        {
            return result.Result != Result.Success
                       ? string.Format(" - {0}", result.Result)
                       : "";
        }

        void WriteParameters(Result result, IEnumerable<Dictionary<string, string>> parameters)
        {
            var keys = parameters.First().Keys;
            var color = Color(result);
            var tabs = 3;

            WriteLine(color, tabs, "| " + string.Join(" | ", keys.Select(k => k.PadRight(MaxWidth(parameters, k))).ToArray()) + " |");
            parameters.ForEach(r => WriteLine(color, tabs, "| " + string.Join(" | ", keys.Select(key => r[key].PadRight(MaxWidth(parameters, key))).ToArray()) + " |"));
        }

        int MaxWidth(IEnumerable<Dictionary<string, string>> parameters, string key)
        {
            return Math.Max(parameters.Max(p => p[key].Length), key.Length);
        }

        void WriteLine(ConsoleColor color, int tabs, string text, params object[] objs)
        {
            WriteLineRaw(color, tabs, string.Format(text, objs));
        }

        void WriteLine()
        {
            WriteLineRaw();
        }

        ConsoleColor Color(Result result)
        {
            switch (result)
            {
                case Result.Failed:
                    return ConsoleColor.Red;
                case Result.Pending:
                    return ConsoleColor.Yellow;
                case Result.Success:
                    return ConsoleColor.Green;
                default:
                    throw new ArgumentOutOfRangeException("result");
            }
        }
    }
}