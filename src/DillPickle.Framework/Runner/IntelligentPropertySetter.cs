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
                .Where(t => t.Attribute != null && IsTheRightConverter(t.Type, targetType))
                .FirstOrDefault();

            if (thing != null)
            {
                var typeConverterType = thing.Type;
                var typeConverter = objectActivator.GetInstance(typeConverterType);
                
                try
                {
                    property.SetValue(instance, typeConverterType.GetMethod("Convert")
                                                    .Invoke(typeConverter, new object[] {value}), null);
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

        bool IsTheRightConverter(Type type, Type targetType)
        {
            return typeof (ITypeConverter<>).MakeGenericType(targetType).IsAssignableFrom(type);
        }
    }

    public interface ITypeConverter<TTargetType>
    {
        TTargetType Convert(string value);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TypeConverterAttribute : Attribute
    {
    }
}