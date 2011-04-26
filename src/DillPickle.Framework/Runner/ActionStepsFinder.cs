using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Executor.Attributes;
using DillPickle.Framework.Extensions;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class ActionStepsFinder : IActionStepsFinder
    {
        readonly IAssemblyLoader assemblyLoader;

        public ActionStepsFinder(IAssemblyLoader assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader;
        }

        public Type[] FindTypesWithActionSteps(string assemblyPath, string featureFileName)
        {
            var assembly = assemblyLoader.LoadConfiguredAssembly(assemblyPath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(featureFileName);
            var possibleClassNames = GetPossibleClassNames(fileNameWithoutExtension);

            var allActionStepsTypes = assembly.GetTypes().Where(HasActionStepsAttribute).ToList();
            
            var matchingTypes = allActionStepsTypes.Where(t => t.Name.In(possibleClassNames)).ToList();

            if (matchingTypes.Count == 0)
            {
                throw new FeatureExecutionException(@"Could not find action steps class matching the feature file {0} - did you forget the [ActionSteps] attribute?

Valid names are: {1}",
                    featureFileName,
                    possibleClassNames.JoinToString(", "));
            }

            if (matchingTypes.Count > 1)
            {
                throw new FeatureExecutionException("Ambiguous action steps class match - {0} was matched by the following classes: {1}",
                    featureFileName,
                    matchingTypes.Select(t => t.Name).JoinToString(", "));
            }

            var rootType = matchingTypes.Single();
            var typesToReturn = new List<Type> {rootType};
            typesToReturn.AddRange(FindIncludes(rootType));

            return typesToReturn.ToArray();
        }

        IEnumerable<Type> FindIncludes(Type rootType)
        {
            return rootType.GetCustomAttributes(typeof (IncludeActionStepsAttribute), false)
                .Cast<IncludeActionStepsAttribute>()
                .SelectMany(a => a.ActionStepsTypesToInclude);
        }

        string[] GetPossibleClassNames(string fileNameWithoutExtension)
        {
            return new[]
                       {
                           CamelCased(fileNameWithoutExtension),
                           Underscored(fileNameWithoutExtension)
                       };
        }

        string Underscored(string fileNameWithoutExtension)
        {
            return string.Join("_", Split(fileNameWithoutExtension));
        }

        string CamelCased(string fileNameWithoutExtension)
        {
            return string.Concat(Split(fileNameWithoutExtension).Select(s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s)).ToArray());
        }

        string[] Split(string str)
        {
            return new string(str.SelectMany(c =>
                                                 {
                                                     if (char.IsUpper(c))
                                                     {
                                                         return new[] {' ', char.ToLower(c)};
                                                     }

                                                     if (c == '_')
                                                     {
                                                         return new[] {' '};
                                                     }

                                                     else return new[] { c };
                                                 }).ToArray())
                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        bool Something(Type type, string[] possibleClassNames)
        {
            return true;
        }

        bool HasActionStepsAttribute(Type t)
        {
            return t.GetCustomAttributes(typeof(ActionStepsAttribute), false).Any();
        }
    }
}