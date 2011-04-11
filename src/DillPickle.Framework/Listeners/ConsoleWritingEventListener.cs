using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DillPickle.Framework.Extensions;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Listeners
{
    public class ConsoleWritingEventListener : DefaultListener
    {
        readonly ConsoleColor defaultColor;
        readonly List<string> errorMessages = new List<string>();

        public ConsoleWritingEventListener()
        {
            defaultColor = Console.ForegroundColor;
        }

        public bool ShowTimestamps { get; set; }

        public override void BeforeFeature(Feature feature)
        {
            WriteLine(ConsoleColor.Cyan, 0, @"{0}
Feature: {1}",
                      feature.Tags.Select(t => string.Format(@"@{0}", t)).JoinToString(" "), feature.Headline);
            WriteLine(ConsoleColor.Cyan, 1, feature.Description);
        }

        public override void BeforeScenario(Feature feature, Scenario scenario)
        {
            WriteLine(ConsoleColor.Cyan, 1, @"
Scenario: {0}", scenario.Headline);
        }

        public override void AfterStep(Feature feature, Scenario scenario, Step step, StepResult result)
        {
            WriteLine(Color(result.Result), 2, "{0} {1}{2}{3}",
                      step.Prefix,
                      step.Text,
                      PossiblyTimestamp(),
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

        string PossiblyTimestamp()
        {
            return ShowTimestamps ? string.Format(" [{0}]", DateTime.Now.ToString("hh:MM")) : "";
        }

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

        public override void AfterFeature(Feature feature, FeatureResult result)
        {
            if (!errorMessages.Any()) return;

            WriteLine();
            errorMessages.ForEach(msg => WriteLineRaw(ConsoleColor.Red, 2, msg));
            errorMessages.Clear();
        }

        void WriteLine()
        {
            Console.WriteLine();
        }

        void WriteLine(ConsoleColor color, int tabs, string text, params object[] objs)
        {
            WriteLineRaw(color, tabs, string.Format(text, objs));
        }

        void WriteLineRaw(ConsoleColor color, int tabs, string text)
        {
            Console.ForegroundColor = color;

            using (var reader = new StringReader(text))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(new string(' ', tabs * 2) + line);
                }
            }

            Console.ForegroundColor = defaultColor;
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