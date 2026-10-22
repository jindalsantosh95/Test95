using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost
{
    public sealed class RouteMapping(HttpMethod verb, string request, string requestSchema, string responseSchema, string name, string description, MethodInfo methodInfo, ParameterInfo[] parameters)
    {
        public static explicit operator RouteMapping(MethodInfo source)
        {
            var extendObjectTypeAttribute = source.DeclaringType.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false).FirstOrDefault() as ExtendObjectTypeAttribute;
            var isQuery = extendObjectTypeAttribute?.Name.Is("Query") ?? false;
            var isMutation = extendObjectTypeAttribute?.Name.Is("Mutation") ?? false;
            var name = (source.GetCustomAttributes(typeof(GraphQLNameAttribute), false).FirstOrDefault() as GraphQLNameAttribute)?.Name ?? source.Name;
            var description = (source.GetCustomAttributes(typeof(GraphQLDescriptionAttribute), false).FirstOrDefault() as GraphQLDescriptionAttribute)?.Description ?? string.Empty;
            var parameters = source.GetParameters();
            var parameterDetails = parameters.Select(x =>
                                             {
                                                 var type = x.ParameterType;

                                                 if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                                     type = type.GetGenericArguments()[0].MakeArrayType();

                                                 return new { Name = x.Name, Type = type };
                                             })
                                             .Where(x => x.Type.GetCustomAttribute<ServiceAttribute>() is null && !x.Type.IsInterface && x.Type != typeof(CancellationToken))
                                             .ToArray();
            var verb = isQuery && parameterDetails.Length == 0 ? HttpMethod.Get : HttpMethod.Post;

            var requestSchemaObject = new JsonSchema
            {
                Title = verb == HttpMethod.Post ? "Request payload" : null,
                Type = JsonObjectType.Object
            };

            foreach (var param in parameterDetails)
            {
                var jsonSchema = JsonSchema.FromType(param.Type);

                requestSchemaObject.Properties[param.Name] = new JsonSchemaProperty
                {
                    Type = jsonSchema.Type,
                    Format = jsonSchema.Format
                };

                var jsonSchemaProperty = requestSchemaObject.Properties[param.Name];

                foreach (var property in jsonSchema.Properties)
                    jsonSchemaProperty.Properties.Add(property);

                foreach (var item in jsonSchema.Items)
                    jsonSchemaProperty.Items.Add(item);

                if (!requestSchemaObject.Definitions.ContainsKey(param.Name))
                    requestSchemaObject.Definitions.Add(param.Name, jsonSchema);
            }

            var requestSchema = JToken.Parse(requestSchemaObject.ToJson(Formatting.Indented)).ToString(Formatting.Indented);
            var requestParameters = parameterDetails.Select(x =>
                                                    {
                                                        if (x.Type == typeof(string)) return $"\"{x.Name}\": \"\"";

                                                        return $"\"{x.Name}\": {Json.Serialize(x.Type.IsArray ? Array.CreateInstance(x.Type.GetElementType(), 0) : x.Type.Name.Is("Nullable`1") ? Activator.CreateInstance(x.Type.GenericTypeArguments[0]) : Activator.CreateInstance(x.Type), false)}";
                                                    })
                                                    .ToArray();

            var request = JToken.Parse($"{{ {string.Join(',', requestParameters)} }}").ToString(Formatting.Indented);

            var responseType = source.ReturnType;

            if (responseType.IsGenericType && responseType.Name.Is("Task`1"))
                responseType = responseType.GenericTypeArguments[0];

            var responseSchema = JsonSchema.FromType(responseType).ToJson(Formatting.Indented);

            return new RouteMapping(verb, request, requestSchema, responseSchema, name, description, source, parameters);
        }


        public HttpMethod Verb { get; } = verb;
        public string Request { get; } = request;
        public string RequestSchema { get; } = requestSchema;
        public string ResponseSchema { get; } = responseSchema;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public MethodInfo MethodInfo { get; } = methodInfo;
        public ParameterInfo[] Parameters { get; } = parameters;
        public Type ReturnType { get; } = methodInfo.ReturnType;
        public Type ServiceType { get; } = methodInfo.DeclaringType;
        public AuthorizeAttribute AuthorizeAttribute { get; } = methodInfo.GetCustomAttribute<AuthorizeAttribute>();
        public bool RequiresMcpCallerContext { get; } = methodInfo.GetCustomAttribute<McpCallerContextRequiredAttribute>() is not null;
        public string Documentation { get; } = Properties.Resources.Template.Replace("{method}", verb.ToString())
                                                                            .Replace("{request}", request)
                                                                            .Replace("{requestSchema}", requestSchema)
                                                                            .Replace("{responseSchema}", responseSchema);
    }
}
