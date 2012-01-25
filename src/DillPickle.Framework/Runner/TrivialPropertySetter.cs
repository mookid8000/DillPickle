using System;
using System.Reflection;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class TrivialPropertySetter : IPropertySetter
    {
        public void SetValue(object instance, PropertyInfo property, string value)
        {
            var targetType = property.PropertyType;
            object obj;

            try
            {
                obj = Convert.ChangeType(value, targetType);
            }
            catch(FormatException fe)
            {
                throw new FeatureExecutionException(fe, "The value '{0}' could not be automatically converted"
                                                        + " to target type {1} ({2} property of {3})",
                                                    value,
                                                    targetType.Name,
                                                    property.Name,
                                                    property.DeclaringType.Name);
            }

            property.SetValue(instance, obj, null);
        }
    }
}