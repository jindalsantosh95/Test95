using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Core.Functions
{
    public static class AzureWorkerHelper
    {
        private static readonly Type[] AzureFubnctionTypes = typeof(BindingContext).Assembly.GetTypes();
        private static readonly Type[] MicrosoftExtensionTypes = typeof(Options).Assembly.GetTypes();
        private static readonly Type[] Types = AzureFubnctionTypes.Concat(MicrosoftExtensionTypes).ToArray();

        public static Type? Find(string name)
        {
            IEnumerable<Type> types = Enumerable.Empty<Type>();

            if (name.Contains('.'))
                types = Types.Where(x => x.FullName.Is(name) || x.IsGenericType && x.FullName.Contains($"{name}`") || x.FullName.Contains($"{name}"));
            else
                types = Types.Where(x => x.Name.Is(name) || x.IsGenericType && x.Name.StartsWith($"{name}`"));

            return types.Count() == 1 ? types.First() : null;
        }

        public static T CreateInstance<T>(string typeDeclaration, params object?[]? args) => (T)CreateInstance(typeDeclaration, args)!;

        public static object CreateInstance(string typeDeclaration, params object?[]? args)
        {
            var type = ((ConstructedType)typeDeclaration).MakeType();

            return Activator.CreateInstance(type, args)!;
        }

        public static void AddFunctionParameterPropertyIfPresent(IDictionary<string, object> properties, FunctionParameter param, string key)
        {
            if (param.Properties.TryGetValue(key, out object? val))
                properties.Add(key, val);
        }


        public static class PropertyBagKeys
        {
            internal const string ConverterType = "converterType";
            internal const string BindingAttribute = "bindingAttribute";
            internal const string ConverterFallbackBehavior = "converterFallbackBehavior";
            internal const string BindingAttributeSupportedConverters = "bindingAttributeSupportedConverters";
        }

        public static class ServiceCollection
        {
            public static IServiceCollection AddScoped(IServiceCollection serviceCollection, string interfaceDeclaration, string implementedTypeDeclaration)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();
                var implementedType = ((ConstructedType)implementedTypeDeclaration).MakeType();

                return (serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection))).AddScoped(@interface, implementedType);
            }
            public static IServiceCollection AddTransient(IServiceCollection serviceCollection, string interfaceDeclaration, string implementedTypeDeclaration)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();
                var implementedType = ((ConstructedType)implementedTypeDeclaration).MakeType();

                return (serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection))).AddTransient(@interface, implementedType);
            }
            public static IServiceCollection AddSingleton(IServiceCollection serviceCollection, string interfaceDeclaration, string implementedTypeDeclaration)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();
                var implementedType = ((ConstructedType)implementedTypeDeclaration).MakeType();

                return (serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection))).AddSingleton(@interface, implementedType);
            }

            public static IServiceCollection AddScopedInstance(IServiceCollection serviceCollection, string interfaceDeclaration, string implementedTypeDeclaration, params object?[]? args)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();

                return (serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection))).AddScoped(@interface, (i) => CreateInstance(implementedTypeDeclaration, args)!);
            }
            public static IServiceCollection AddTransientInstance(IServiceCollection serviceCollection, string interfaceDeclaration, string implementedTypeDeclaration, params object?[]? args)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();

                return (serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection))).AddTransient(@interface, (i) => CreateInstance(implementedTypeDeclaration, args)!);
            }
            public static IServiceCollection AddSingletonInstance(IServiceCollection serviceCollection, string interfaceDeclaration, string implementedTypeDeclaration, params object?[]? args)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();

                return (serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection))).AddSingleton(@interface, CreateInstance(implementedTypeDeclaration, args)!);
            }
        }

        public static class ServiceProvider
        {
            public static object? GetService(IServiceProvider serviceProvider, string interfaceDeclaration)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();

                return typeof(IServiceProvider).GetMethod("GetService")!
                                               .Invoke(serviceProvider, new object[] { @interface });
            }
        }

        public static class InvocationFeatures
        {
            public static object? Get(IInvocationFeatures invocationFeatures, string interfaceDeclaration)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();

                return typeof(IInvocationFeatures).GetMethod("Get")!
                                                  .MakeGenericMethod(@interface)
                                                  .Invoke(invocationFeatures ?? throw new ArgumentNullException(nameof(invocationFeatures)), null);
            }

            public static void SetInstance(IInvocationFeatures invocationFeatures, string interfaceDeclaration, string implementedTypeDeclaration, params object?[]? args)
            {
                var @interface = ((ConstructedType)interfaceDeclaration).MakeType();

                typeof(IInvocationFeatures).GetMethod("Set")!
                                           .MakeGenericMethod(@interface)
                                           .Invoke(invocationFeatures ?? throw new ArgumentNullException(nameof(invocationFeatures)), new object[] { CreateInstance(implementedTypeDeclaration, args)! });
            }
        }


        internal sealed class ConstructedType
        {
            public static explicit operator ConstructedType(string source)
            {
                source = RegexCompiled.Replace(source, @"\s+", string.Empty);

                ConstructedType current = new()
                {
                    Parent = new()
                };

                foreach (char @char in source)
                {
                    if (@char is '<' or ',')
                    {
                        var genericParam = new ConstructedType()
                        {
                            Parent = current
                        };

                        current.Parameters = current.Parameters.Append(genericParam).ToArray();

                        current = genericParam;
                    }
                    else if (@char == '>')
                    {
                        current = current.Parent;
                    }
                    else
                        current.Name += @char;
                }

                return current;
            }

            public ConstructedType Parent { get; set; }
            public string Name { get; set; } = string.Empty;
            public ConstructedType[] Parameters { get; set; } = Array.Empty<ConstructedType>();

            public Type MakeType()
            {
                var type = Find(Parameters.Any() ? $"{Name}`{Parameters.Length}" : Name)!;

                if (Parameters.Any())
                    type = type.MakeGenericType(Parameters.Select(x => x.MakeType()).ToArray());

                return type;
            }
        }
    }
}
