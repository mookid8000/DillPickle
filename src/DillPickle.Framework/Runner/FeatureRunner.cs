using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Executor;
using DillPickle.Framework.Matcher;
using DillPickle.Framework.Parser;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class FeatureRunner : IFeatureRunner
    {
        readonly List<IListener> listeners = new List<IListener>();

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

            var steps = (from scenarioBase in feature.Scenarios
                         from scenario in scenarioBase.GetExecutableScenarios()
                         from step in scenario.Steps
                         select step)
                .Concat(feature.BackgroundSteps)
                .Distinct();

            var actionStepsClasses = finder.Find(types);

            var matches = (from step in steps
                           from actionStepClass in actionStepsClasses
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

            var executionObjects = matches
                .Where(m => m.Match.IsMatch)
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
                foreach (var scenario in feature.Scenarios.SelectMany(s => s.GetExecutableScenarios()))
                {
                    var currentScenario = scenario;
                    listeners.ForEach(l => l.BeforeScenario(feature, currentScenario));

                    var scenarioResult = new ScenarioResult(scenario.Headline);
                    featureResult.ScenarioResults.Add(scenarioResult);

                    foreach (var step in feature.BackgroundSteps.Concat(scenario.Steps))
                    {
                        var currentStep = step;
                        listeners.ForEach(l => l.BeforeStep(feature, currentScenario, currentStep));

                        var stepResult = new StepResult(step.Text);
                        scenarioResult.StepResults.Add(stepResult);

                        var stepToMatch = step;
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
                                var targetObject = executionObjects[method.Type];
                                var parameters = GenerateParameterList(method.ActionStepMethod, method.Match);
                                var methodInfo = method.ActionStepMethod.MethodInfo;

                                methodInfo.Invoke(targetObject, parameters);

                                stepResult.Result = Result.Success;
                            }
                            catch (FeatureExecutionException)
                            {
                                throw;
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
                                if (p.Type == typeof(List<Dictionary<string,string>>))
                                {
                                    return match.Step.Parameters;
                                }
                                
                                if (p.Type.IsArray && p.Type.GetArrayRank() == 1)
                                {
                                    return Deserialize(p.Type.GetElementType(), match.Step.Parameters);
                                }

                                var token = match.Tokens
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

                                var value = Convert.ChangeType(token.Value, p.Type);

                                return value;
                            })
                .ToArray();
        }

        object Deserialize(Type type, IEnumerable<Dictionary<string, string>> parameters)
        {
            var array = Array.CreateInstance(type, parameters.Count());

            var objects = parameters
                .Select(row => PopulateInstance(Activator.CreateInstance(type), row))
                .ToArray();

            Array.Copy(objects, array, objects.Length);

            return array;
        }

        const BindingFlags DeserializationPropertyBindingFlags = BindingFlags.IgnoreCase
                                                                 | BindingFlags.Public
                                                                 | BindingFlags.Instance;

        object PopulateInstance(object instance, Dictionary<string, string> dictionary)
        {
            foreach(var kvp in dictionary)
            {
                var type = instance.GetType();
                var propertyName = kvp.Key;
                var property = type.GetProperty(propertyName, DeserializationPropertyBindingFlags);

                if (property == null)
                {
                    throw new FeatureExecutionException(
                        "Property corresponding to table column '{0}' was not found on object of type {1}",
                        propertyName,
                        type.FullName);
                }

                var value = kvp.Value;

                try
                {
                    property.SetValue(instance, Convert.ChangeType(value, property.PropertyType), null);
                }
                catch(Exception e)
                {
                    throw new FeatureExecutionException(
                        "Error converting '{0}' to value of type {1} (property named '{2}' on object of type {3}): {4}",
                        value,
                        property.PropertyType.Name,
                        property.Name,
                        type.FullName,
                        e);
                }
            }

            return instance;
        }

        public void AddListener(IListener listener)
        {
            listeners.Add(listener);
        }
    }
}