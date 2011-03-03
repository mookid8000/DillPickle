using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Executor;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Executor.Attributes.Base;
using DillPickle.Framework.Matcher;
using DillPickle.Framework.Parser.Api;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class FeatureRunner : IFeatureRunner
    {
        readonly List<IListener> listeners = new List<IListener>();
        readonly IObjectActivator objectActivator;
        readonly IPropertySetter propertySetter;

        public FeatureRunner(IObjectActivator objectActivator, IPropertySetter propertySetter)
        {
            this.objectActivator = objectActivator;
            this.propertySetter = propertySetter;
        }

        public FeatureResult Run(Feature feature, Type[] types, RunnerOptions options)
        {
            return ExecuteFeature(feature, types, options);
        }

        FeatureResult ExecuteFeature(Feature feature, Type[] types, RunnerOptions options)
        {
            var featureResult = new FeatureResult(feature);
            var matcher = new StepMatcher();
            var finder = new ActionStepFinder();

            var steps = GetUniqueSteps(feature);
            var actionStepsClasses = finder.Find(types);
            var matches = MatchStepsToActionMethods(steps, actionStepsClasses, matcher);
            var executionObjects = GetExecutionObjects(matches);
            var filter = options.Filter;

            try
            {
                BeforeFeature(feature, executionObjects);

                foreach (var scenario in feature.Scenarios.SelectMany(s => s.GetExecutableScenarios()).Where(s => filter.IsSatisfiedBy(s.Tags)))
                {
                    BeforeScenario(feature, scenario, executionObjects);
                    var result = ExecuteScenario(scenario, feature, matches, executionObjects, options);
                    featureResult.AddScenarioResult(result);
                    AfterScenario(feature, scenario, result, executionObjects);

                    if (options.SuccessRequired && !result.Success) break;
                }

                AfterFeature(feature, featureResult, executionObjects);
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
                .Select(t => new ActionStepsObjectHolder(objectActivator)
                                 {
                                     Type = t,
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

        ScenarioResult ExecuteScenario(Scenario scenario, Feature feature, IEnumerable<FoundMatch> matches, Dictionary<Type, ActionStepsObjectHolder> executionObjects, RunnerOptions options)
        {
            var scenarioResult = new ScenarioResult(scenario.Headline);

            foreach (var step in feature.BackgroundSteps.Concat(scenario.Steps))
            {
                BeforeStep(scenario, feature, step, executionObjects);
                var result = ExecuteStep(step, matches, executionObjects, options);
                scenarioResult.AddStepResult(result);
                AfterStep(scenario, feature, step, result, executionObjects);

                if (options.SuccessRequired && !result.Success) break;
            }

            return scenarioResult;
        }

        StepResult ExecuteStep(Step step, IEnumerable<FoundMatch> matches, IDictionary<Type, ActionStepsObjectHolder> executionObjects, RunnerOptions options)
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

                    if (!options.DruRun)
                    {
                        methodInfo.Invoke(targetObject, parameters);
                    }

                    stepResult.Result = Result.Success;
                }
                catch (TargetInvocationException ex)
                {
                    var innerException = ex.InnerException;

                    if (innerException is FeatureExecutionException)
                    {
                        throw;
                    }

                    stepResult.Result = Result.Failed;
                    stepResult.ErrorMessage = innerException.ToString();
                }
                catch (Exception ex)
                {
                    stepResult.Result = Result.Failed;
                    stepResult.ErrorMessage = ex.ToString();
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
                    propertySetter.SetValue(instance, property, value);
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

        void ExecuteMethodsDecoratedWith<T>(IEnumerable<ActionStepsObjectHolder> collection) where T : HookAttribute
        {
            foreach (var item in collection)
            {
                var instance = item.GetInstance();

                var methods = instance.GetType()
                    .GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof (T), false).Any())
                    .Where(m => !m.GetParameters().Any())
                    .ToList();

                methods.ForEach(m => m.Invoke(instance, new object[0]));
            }
        }

        void BeforeFeature(Feature feature, Dictionary<Type, ActionStepsObjectHolder> dictionary)
        {
            listeners.ForEach(l => l.BeforeFeature(feature));
            ExecuteMethodsDecoratedWith<BeforeFeatureAttribute>(dictionary.Values);
        }

        void AfterFeature(Feature feature, FeatureResult featureResult, Dictionary<Type, ActionStepsObjectHolder> dictionary)
        {
            listeners.ForEach(l => l.AfterFeature(feature, featureResult));
            ExecuteMethodsDecoratedWith<AfterFeatureAttribute>(dictionary.Values);
        }

        void AfterScenario(Feature feature, Scenario scenario, ScenarioResult result, Dictionary<Type, ActionStepsObjectHolder> dictionary)
        {
            listeners.ForEach(l => l.AfterScenario(feature, scenario, result));
            ExecuteMethodsDecoratedWith<AfterScenarioAttribute>(dictionary.Values);
        }

        void BeforeScenario(Feature feature, Scenario scenario, Dictionary<Type, ActionStepsObjectHolder> dictionary)
        {
            listeners.ForEach(l => l.BeforeScenario(feature, scenario));
            ExecuteMethodsDecoratedWith<BeforeScenarioAttribute>(dictionary.Values);
        }

        void BeforeStep(Scenario scenario, Feature feature, Step step, Dictionary<Type, ActionStepsObjectHolder> dictionary)
        {
            listeners.ForEach(l => l.BeforeStep(feature, scenario, step));
            ExecuteMethodsDecoratedWith<BeforeStepAttribute>(dictionary.Values);
        }

        void AfterStep(Scenario scenario, Feature feature, Step step, StepResult result, Dictionary<Type, ActionStepsObjectHolder> dictionary)
        {
            listeners.ForEach(l => l.AfterStep(feature, scenario, step, result));
            ExecuteMethodsDecoratedWith<AfterStepAttribute>(dictionary.Values);
        }

        class FoundMatch
        {
            public Type RequiredType { get; set; }
            public ActionStepMethod ActionStepMethod { get; set; }
            public StepMatch StepMatch { get; set; }
        }

        class ActionStepsObjectHolder
        {
            readonly IObjectActivator objectActivator;

            object instance;

            public ActionStepsObjectHolder(IObjectActivator objectActivator)
            {
                this.objectActivator = objectActivator;
            }

            public Type Type { get; set; }

            public void CleanUp()
            {
                if (instance is IDisposable)
                {
                    ((IDisposable)instance).Dispose();
                }
            }

            public object GetInstance()
            {
                return instance ?? (instance = CreateInstance());
            }

            object CreateInstance()
            {
                return objectActivator.GetInstance(Type);
            }
        }
    }
}