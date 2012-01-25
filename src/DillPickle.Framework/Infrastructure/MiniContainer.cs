using System;
using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Infrastructure
{
    public class MiniContainer
    {
        readonly Dictionary<Type, List<Component>> typeMappings = new Dictionary<Type, List<Component>>();
        readonly Dictionary<Type, Component> components = new Dictionary<Type, Component>();

        class Component
        {
            public Component(Type implementationType, Type serviceType)
            {
                ImplementationType = implementationType;
                ServiceType = serviceType;
                CommisionActions = new List<Delegate>();
            }

            public Type ServiceType { get; private set; }
            public Type ImplementationType { get; private set; }
            public List<Delegate> CommisionActions { get; private set; }
        }

        public T Resolve<T>()
        {
            var typeToCreate = typeof(T);

            return (T) Create(typeToCreate, new ResolutionContext());
        }

        public MiniContainer MapType<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof (TService);
            var implementationType = typeof (TImplementation);

            if (implementationType.IsInterface || implementationType.IsAbstract)
            {
                throw new InvalidOperationException(
                    string.Format("{0} cannot be registered as an implementation of {1} because it is not concrete.",
                                  implementationType,
                                  serviceType));
            }

            var component = new Component(implementationType, serviceType);

            AddComponent(component);

            return this;
        }

        void AddComponent(Component component)
        {
            var serviceType = component.ServiceType;
            var implementationType = component.ImplementationType;

            var list = typeMappings.ContainsKey(serviceType)
                           ? typeMappings[serviceType]
                           : (typeMappings[serviceType] = new List<Component>());

            list.Add(component);
            components[implementationType] = component;
        }

        object Create(Type serviceTypeToCreate, ResolutionContext context)
        {
            var concreteTypeToCreate = GetConcreteType(serviceTypeToCreate, context);

            using (context.EnterResolutionContextOf(concreteTypeToCreate))
            {
                var constructors = concreteTypeToCreate
                    .GetConstructors().FirstOrDefault();

                if (constructors == null)
                {
                    throw new InvalidOperationException(string.Format("Could not find valid constructor on {0}.", concreteTypeToCreate));
                }

                var parameters = constructors
                    .GetParameters()
                    .Select(p => Create(p.ParameterType, context))
                    .ToArray();

                var instance = Activator.CreateInstance(concreteTypeToCreate, parameters);

                PerformCommissionTasks(instance);

                return instance;
            }
        }

        void PerformCommissionTasks(object instance)
        {
            var type = instance.GetType();

            if (!components.ContainsKey(type)) return;

            var component = components[type];

            foreach(var deli in component.CommisionActions)
            {
                deli.Method.Invoke(deli.Target, new[] {instance});
            }
        }

        Type GetConcreteType(Type serviceTypeToCreate, ResolutionContext context)
        {
            if (typeMappings.ContainsKey(serviceTypeToCreate))
            {
                var availableTypes = typeMappings[serviceTypeToCreate];

                return availableTypes
                    .SkipWhile(context.CurrentlyResolving)
                    .Select(c => c.ImplementationType)
                    .First();
            }

            if (!serviceTypeToCreate.IsInterface 
                && !serviceTypeToCreate.IsAbstract)
            {
                return serviceTypeToCreate;
            }

            throw new InvalidOperationException(string.Format("Don't know how to create instance of {0}. Did you forget a type mapping?", serviceTypeToCreate));
        }

        class ResolutionContext
        {
            readonly Stack<Type> resolving;

            public ResolutionContext()
            {
                resolving = new Stack<Type>();
            }
            
            public IDisposable EnterResolutionContextOf(Type type)
            {
                return new DisposableTypeResolutionContext(this, type);
            }

            public bool CurrentlyResolving(Component component)
            {
                return resolving.Contains(component.ImplementationType);
            }

            class DisposableTypeResolutionContext : IDisposable
            {
                readonly ResolutionContext context;
                bool disposed;

                public DisposableTypeResolutionContext(ResolutionContext context, Type type)
                {
                    this.context = context;
                    context.resolving.Push(type);
                }

                public void Dispose()
                {
                    if (disposed) return;
                    context.resolving.Pop();
                    disposed = true;
                }
            }
        }

        public MiniContainer Configure<T>(Action<T> doConfigure)
        {
            var concreteType = typeof(T);

            if (!components.ContainsKey(concreteType))
                throw new InvalidOperationException(string.Format("Could not set up configuration for {0} - it does not seem to have been mapped.", concreteType));

            var component = components[concreteType];

            component.CommisionActions.Add(doConfigure);

            return this;
        }
    }
}