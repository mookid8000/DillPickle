using System;
using System.Linq;
using System.Reflection;
using DillPickle.Framework.Exceptions;
using DillPickle.Framework.Runner.Api;

namespace DillPickle.Framework.Runner
{
    public class IntelligentPropertySetter : IPropertySetter
    {
        readonly IPropertySetter fallbackPropertySetter;
        readonly Assembly assembly;
        readonly IObjectActivator objectActivator;

        public IntelligentPropertySetter(IPropertySetter fallbackPropertySetter, Assembly assembly, IObjectActivator objectActivator)
        {
            this.fallbackPropertySetter = fallbackPropertySetter;
            this.assembly = assembly;
            this.objectActivator = objectActivator;
        }

        public void SetValue(object instance, PropertyInfo property, string value)
        {
            var targetType = property.PropertyType;
            var thing = assembly.GetTypes()
                .Select(t => new
                                 {
                                     Type = t,
                                     Attribute = t.GetCustomAttributes(typeof (TypeConverterAttribute), false)
                                         .Cast<TypeConverterAttribute>()
                                         .SingleOrDefault()
                                 })
                .Where(t => t.Attribute != null && t.Attribute.TargetType == targetType)
                .FirstOrDefault();

            if (thing != null)
            {
                var typeConverterType = thing.Type;

                if (!typeof(ITypeConverter).IsAssignableFrom(typeConverterType))
                {
                    throw new FeatureExecutionException(
                        "{0} must implement ITypeConverter if you want it to convert to {1}",
                        typeConverterType.FullName,
                        targetType.Name);
                }

                var typeConverter = (ITypeConverter) objectActivator.GetInstance(typeConverterType);
                try
                {
                    var convert = typeConverter.Convert(value);
                    property.SetValue(instance, convert, null);
                }
                catch(Exception e)
                {
                    throw new FeatureExecutionException(e,
                                                        "Unhandled exception while attempting to convert {0} to {1} using {2}",
                                                        value,
                                                        targetType.FullName,
                                                        typeConverterType.FullName);
                }
            }
            else
            {
                fallbackPropertySetter.SetValue(instance, property, value);
            }
        }
    }

    public interface ITypeConverter
    {
        object Convert(string value);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TypeConverterAttribute : Attribute
    {
        readonly Type targetType;

        public TypeConverterAttribute(Type targetType)
        {
            this.targetType = targetType;
        }

        public Type TargetType
        {
            get { return targetType; }
        }
    }
}