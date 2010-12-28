using System;
using System.Collections.Generic;
using System.Linq;

namespace DillPickle.Framework.Infrastructure
{
    public class MiniContainer
    {
        readonly Dictionary<Type, List<Type>> typeMappings = new Dictionary<Type, List<Type>>();

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

            var list = typeMappings.ContainsKey(serviceType)
                           ? typeMappings[serviceType]
                           : (typeMappings[serviceType] = new List<Type>());

            list.Add(implementationType);

            return this;
        }

        object Create(Type serviceTypeToCreate, ResolutionContext context)
        {
            var concreteTypeToCreate = GetConcreteType(serviceTypeToCreate, context);

            using (context.EnterResolutionContextOf(concreteTypeToCreate))
            {
                var parameters = concreteTypeToCreate
                    .GetConstructors().First()
                    .GetParameters()
                    .Select(p => Create(p.ParameterType, context))
                    .ToArray();

                var instance = Activator.CreateInstance(concreteTypeToCreate, parameters);

                return instance;
            }
        }

        Type GetConcreteType(Type serviceTypeToCreate, ResolutionContext context)
        {
            if (typeMappings.ContainsKey(serviceTypeToCreate))
            {
                var availableTypes = typeMappings[serviceTypeToCreate];

                return availableTypes
                    .SkipWhile(context.CurrentlyResolving)
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

            public bool CurrentlyResolving(Type type)
            {
                return resolving.Contains(type);
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
    }
}