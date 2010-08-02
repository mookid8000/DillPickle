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

        class FoundMatch
        {
            public Type RequiredType { get; set; }
            public ActionStepMethod ActionStepMethod { get; set; }
            public StepMatch StepMatch { get; set; }
        }

        class ActionStepsObjectHolder
        {
            public Type Type { get; set; }
            public object Instance { get; set; }
            
            public void CleanUp()
            {
                if (Instance is IDisposable)
                {
                    ((IDisposable) Instance).Dispose();
                }
            }

            public object GetInstance()
            {
                return Instance;
            }
        }

        public FeatureResult Run(Feature feature, Type[] types)
        {
            BeforeFeature(feature);
            var result = ExecuteFeature(feature, types);
            AfterFeature(feature, result);
            return result;
        }

        FeatureResult ExecuteFeature(Feature feature, Type[] types)
        {
            var featureResult = new FeatureResult
                                    {
                                        Headline = feature.Headline,
                                        Description = feature.Description
                                    };

            var matcher = new StepMatcher();
            var finder = new ActionStepFinder();

            var steps = GetUniqueSteps(feature);
            var actionStepsClasses = finder.Find(types);
            var matches = MatchStepsToActionMethods(steps, actionStepsClasses, matcher);
            var executionObjects = GetExecutionObjects(matches);

            try
            {
                foreach (var scenario in feature.Scenarios.SelectMany(s => s.GetExecutableScenarios()))
                {
                    BeforeScenario(feature, scenario);
                    var result = ExecuteScenario(scenario, feature, matches, executionObjects);
                    featureResult.AddScenarioResult(result);
                    AfterScenario(feature, scenario, result);
                }
            }
            finally
            {
                CleanUpExecutionObjects(executionObjects);
            }

            return featureResult;
        }

        void CleanUpExecutionObjects(IDictionary<Type, ActionStepsObjectHolder> executionObjects)
        {
            executionObjects.Values.ToList()
                .ForEach(d => d.CleanUp());
        }

        Dictionary<Type, ActionStepsObjectHolder> GetExecutionObjects(IEnumerable<FoundMatch> matches)
        {
            return matches
                .Select(m => m.RequiredType)
                .Distinct()
                .Select(t => new ActionStepsObjectHolder
                                 {
                                     Type = t,
                                     Instance = Activator.CreateInstance(t)
                                 })
                .ToDictionary(t => t.Type);
        }

        List<FoundMatch> MatchStepsToActionMethods(IEnumerable<Step> steps, IEnumerable<ActionStepsClass> actionStepsClasses, StepMatcher matcher)
        {
            return (from step in steps
                    from actionStepClass in actionStepsClasses
                    let requiredType = actionStepClass.Type
                    from actionStepMethod in actionStepClass.ActionStepMethods
                    let match = matcher.GetMatch(step, actionStepMethod)
                    where match.IsMatch
                    select new FoundMatch
                               {
                                   RequiredType = requiredType,
                                   ActionStepMethod = actionStepMethod,
                                   StepMatch = match,
                               })
                .ToList();
        }

        IEnumerable<Step> GetUniqueSteps(Feature feature)
        {
            return (from scenarioBase in feature.Scenarios
                    from scenario in scenarioBase.GetExecutableScenarios()
                    from step in scenario.Steps
                    select step)
                .Concat(feature.BackgroundSteps)
                .Distinct();
        }

        ScenarioResult ExecuteScenario(Scenario scenario, Feature feature, IEnumerable<FoundMatch> matches, Dictionary<Type, ActionStepsObjectHolder> executionObjects)
        {
            var scenarioResult = new ScenarioResult(scenario.Headline);

            foreach (var step in feature.BackgroundSteps.Concat(scenario.Steps))
            {
                BeforeStep(scenario, feature, step);
                var result = ExecuteStep(step, matches, executionObjects);
                scenarioResult.AddStepResult(result);
                AfterStep(scenario, feature, step, result);
            }

            return scenarioResult;
        }

        StepResult ExecuteStep(Step step, IEnumerable<FoundMatch> matches, IDictionary<Type, ActionStepsObjectHolder> executionObjects)
        {
            var stepResult = new StepResult(step.Text);

            var stepToMatch = step;
            var method = matches.FirstOrDefault(m => m.StepMatch.Step == stepToMatch);

            if (method == null)
            {
                stepResult.Result = Result.Pending;
            }
            else
            {
                try
                {
                    var targetObject = executionObjects[method.RequiredType].GetInstance();
                    var parameters = GenerateParameterList(method.ActionStepMethod, method.StepMatch);
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

            return stepResult;
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
                                if (p.Type == typeof(List<Dictionary<string, string>>))
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
            foreach (var kvp in dictionary)
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
                catch (Exception e)
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

        void BeforeFeature(Feature feature)
        {
            listeners.ForEach(l => l.BeforeFeature(feature));
        }

        void AfterFeature(Feature feature, FeatureResult featureResult)
        {
            listeners.ForEach(l => l.AfterFeature(feature, featureResult));
        }

        void AfterScenario(Feature feature, Scenario scenario, ScenarioResult result)
        {
            listeners.ForEach(l => l.AfterScenario(feature, scenario, result));
        }

        void BeforeScenario(Feature feature, Scenario scenario)
        {
            listeners.ForEach(l => l.BeforeScenario(feature, scenario));
        }

        void BeforeStep(Scenario scenario, Feature feature, Step step)
        {
            listeners.ForEach(l => l.BeforeStep(feature, scenario, step));
        }

        void AfterStep(Scenario scenario, Feature feature, Step step, StepResult result)
        {
            listeners.ForEach(l => l.AfterStep(feature, scenario, step, result));
        }
    }
}