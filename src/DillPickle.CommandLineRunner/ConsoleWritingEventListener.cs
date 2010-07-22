using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner;

namespace DillPickle.CommandLineRunner
{
    internal class ConsoleWritingEventListener : DefaultListener
    {
        readonly ConsoleColor defaultColor;
        readonly List<string> errorMessages = new List<string>();

        public ConsoleWritingEventListener()
        {
            defaultColor = Console.ForegroundColor;
        }

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
            WriteLine(Color(result.Result), 2, "{0} {1}{2}",
                      step.Prefix,
                      step.Text,
                      result.Result != Result.Success
                          ? string.Format(" - {0}", result.Result)
                          : "");

            if (result.Result == Result.Failed)
            {
                errorMessages.Add(result.ErrorMessage);
            }
        }

        public override void AfterFeature(Feature feature, FeatureResult result)
        {
            if (!errorMessages.Any()) return;

            WriteLine();
            errorMessages.ForEach(msg => WriteLine(ConsoleColor.Red, 2, msg));
            errorMessages.Clear();
        }

        void WriteLine()
        {
            Console.WriteLine();
        }

        void WriteLine(ConsoleColor color, int tabs, string text, params object[] objs)
        {
            Console.ForegroundColor = color;

            using (var reader = new StringReader(string.Format(text, objs)))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(new string(' ', tabs*2) + line);
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