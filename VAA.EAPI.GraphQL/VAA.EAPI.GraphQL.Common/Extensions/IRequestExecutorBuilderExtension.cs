using HotChocolate;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class IRequestExecutorBuilderExtension
    {
        private static string[] ignoreMethodsNames = { "GetType", "ToString", "Equals", "GetHashCode" };

        /// <summary>
        /// Adds a GraphQL type to the schema if it doe not already exist.
        /// </summary>
        /// <param name="builder">
        /// The GraphQL configuration builder.
        /// </param>
        /// <typeparam name="T">
        /// The GraphQL type.
        /// </typeparam>
        /// <returns>
        /// Returns the GraphQL configuration builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="builder"/> or <paramref name="builder"/> is <c>null</c>
        /// </exception>
        public static IRequestExecutorBuilder TryAddType<T>(this IRequestExecutorBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            if (builder.IsTypeAlreadyRegistrerd<T>()) return builder;

            builder.AddType<T>();

            if (!builder.Services.Any(x => x.ServiceType == typeof(T)))
            {
                Type implementation = typeof(T);
                Type @interface = implementation.GetInterfaces().FirstOrDefault(i => i.Name.Is($"I{implementation.Name}"));

                if (@interface is not null)
                    builder.Services.AddSingleton(@interface, implementation);
                else
                    builder.Services.AddSingleton(implementation);
            }

            return builder;
        }

        private static bool IsTypeAlreadyRegistrerd<T>(this IRequestExecutorBuilder builder) => builder.Services.Where(x => x.ServiceType.FullName.Contains("HotChocolate.Execution.Configuration.RequestExecutorSetup") && x.ImplementationInstance is not null)
                                                                                                                .Any(x =>
                                                                                                                {
                                                                                                                    var requestExecutorSetup = ((Microsoft.Extensions.Options.ConfigureNamedOptions<RequestExecutorSetup>)x.ImplementationInstance).Action.Target;
                                                                                                                    var configureSchemaField = requestExecutorSetup.GetType().GetField("configureSchema");

                                                                                                                    if (configureSchemaField is null) return false;

                                                                                                                    var configureSchema = configureSchemaField.GetValue(requestExecutorSetup);
                                                                                                                    var targetProperty = configureSchema.GetType().GetProperty("Target");

                                                                                                                    if (targetProperty is null) return false;

                                                                                                                    var target = targetProperty.GetValue(configureSchema)?.ToString() ?? string.Empty;

                                                                                                                    if (target.IsNullOrWhiteSpace()) return false;
                                                                                                                    if (target.Contains("VAA.EAPI.GraphQL.FunctionHost.VersionQuery")) return false;

                                                                                                                    var match = RegexCompiled.Match(target, @"VAA.*.[^.]+(Query|Mutation)[^\]]*");

                                                                                                                    if (match.Success)
                                                                                                                    {
                                                                                                                        var fileInfo = new System.IO.FileInfo($@"{RegexCompiled.Match(match.Value, "VAA.EAPI.GraphQL.Domain.[^.]+").Value}.dll");

                                                                                                                        if (fileInfo.Exists)
                                                                                                                        {
                                                                                                                            var assembly = Assembly.LoadFile(fileInfo.FullName);
                                                                                                                            var type = assembly.GetType(match.Value);

                                                                                                                            if (type is null) return false;

                                                                                                                            var targetMethodNames = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Where(x => !ignoreMethodsNames.Contains(x.Name)).Select(m => (m.CustomAttributes.FirstOrDefault(c => c.AttributeType == typeof(GraphQLNameAttribute))?.ConstructorArguments[0].Value ?? m.Name).ToString()).ToArray();
                                                                                                                            var sourceMethodNames = typeof(T).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Where(x => !ignoreMethodsNames.Contains(x.Name)).Select(m => (m.CustomAttributes.FirstOrDefault(c => c.AttributeType == typeof(GraphQLNameAttribute))?.ConstructorArguments[0].Value ?? m.Name).ToString()).ToArray();

                                                                                                                            return targetMethodNames.Any(t => sourceMethodNames.Contains(t));
                                                                                                                        }
                                                                                                                    }

                                                                                                                    return false;
                                                                                                                });
    }
}
