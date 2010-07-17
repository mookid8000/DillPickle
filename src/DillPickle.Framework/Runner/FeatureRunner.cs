using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Executor;
using DillPickle.Framework.Matcher;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class FeatureRunner : IFeatureRunner
    {
        readonly List<IListener> listeners = new List<IListener>();

        #region IFeatureRunner Members

        public FeatureResult Run(Feature feature, Type[] types)
        {
            listeners.ForEach(l => l.BeforeFeature(feature));

            var featureResult = new FeatureResult
                                    {
                                        Headline = feature.Headline,
                                        Description = feature.Description
                                    };

            var matcher = new StepMatcher();
            var finder = new ActionStepFinder();

            var matches = (from scenario in feature.Scenarios
                           from step in scenario.Steps
                           from actionStepClass in finder.Find(types)
                           let requiredType = actionStepClass.Type
                           from actionStepMethod in actionStepClass.ActionStepMethods
                           let match = matcher.GetMatch(step, actionStepMethod)
                           select new
                                      {
                                          Type = requiredType,
                                          ActionStepMethod = actionStepMethod,
                                          Match = match,
                                      })
                .ToList();

            Dictionary<Type, object> executionObjects = matches
                .Select(m => m.Type)
                .Distinct()
                .Select(t => new
                                 {
                                     Type = t,
                                     Instance = Activator.CreateInstance(t)
                                 })
                .ToDictionary(t => t.Type, t => t.Instance);

            try
            {
                foreach (Scenario scenario in feature.Scenarios)
                {
                    Scenario currentScenario = scenario;
                    listeners.ForEach(l => l.BeforeScenario(feature, currentScenario));

                    var scenarioResult = new ScenarioResult(scenario.Headline);
                    featureResult.ScenarioResults.Add(scenarioResult);

                    foreach (Step step in scenario.Steps)
                    {
                        Step currentStep = step;
                        listeners.ForEach(l => l.BeforeStep(feature, currentScenario, currentStep));

                        var stepResult = new StepResult(step.Text);
                        scenarioResult.StepResults.Add(stepResult);

                        Step stepToMatch = step;
                        var method = matches.FirstOrDefault(m => m.Match.Step == stepToMatch
                                                                 && m.Match.IsMatch);

                        if (method == null)
                        {
                            stepResult.Result = Result.Pending;
                        }
                        else
                        {
                            try
                            {
                                object targetObject = executionObjects[method.Type];
                                object[] parameters = GenerateParameterList(method.ActionStepMethod, method.Match);
                                MethodInfo methodInfo = method.ActionStepMethod.MethodInfo;

                                methodInfo.Invoke(targetObject, parameters);

                                stepResult.Result = Result.Success;
                            }
                            catch (Exception e)
                            {
                                stepResult.Result = Result.Failed;
                                stepResult.ErrorMessage = e.ToString();
                            }
                        }

                        listeners.ForEach(l => l.AfterStep(feature, currentScenario, currentStep, stepResult));
                    }

                    listeners.ForEach(l => l.AfterScenario(feature, currentScenario, scenarioResult));
                }
            }
            finally
            {
                executionObjects.Values
                    .Where(obj => obj is IDisposable)
                    .Cast<IDisposable>()
                    .ToList().ForEach(d => d.Dispose());
            }

            listeners.ForEach(l => l.AfterFeature(feature, featureResult));

            return featureResult;
        }

        #endregion

        object[] GenerateParameterList(ActionStepMethod method, StepMatch match)
        {
            var parameters = method.MethodInfo.GetParameters()
                .Select(p => new
                                 {
                                     p.Name,
                                     Type = p.ParameterType
                                 });

            return parameters
                .Select(p =>
                            {
                                VariableToken token = match.Tokens
                                    .Where(t => t is VariableToken)
                                    .Cast<VariableToken>()
                                    .FirstOrDefault(t => t.Name == p.Name);

                                if (token == null)
                                {
                                    throw new FeatureExecutionException(
                                        @"Could not find a variable matching the variable name '{0}' in {1}.{2}: ""({3}) {4}""",
                                        p.Name,
                                        method.MethodInfo.DeclaringType.Name,
                                        method.MethodInfo.Name,
                                        method.StepType,
                                        method.Text);
                                }

                                object value = Convert.ChangeType(token.Value, p.Type);

                                return value;
                            })
                .ToArray();
        }

        public void AddListener(IListener listener)
        {
            listeners.Add(listener);
        }
    }

    public class FeatureExecutionException : Exception
    {
        public FeatureExecutionException(string message, params object[] objs)
            : base(string.Format(message, objs))
        {
        }
    }
}