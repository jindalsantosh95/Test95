using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate;
using VAA.EAPI.GraphQL.Rest.Middleware.Mcp;
using VAA.EAPI.GraphQL.Rest.Middleware;
using Error = VAA.EAPI.GraphQL.Rest.Middleware.Mcp.Error;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost
{
    public sealed class Router
    {
        private const string VaaTypeMatchPattern = @"^VAA.*$";
        private const string QueryMutationMatchPattern = @"[^.]+(Query|Mutation)[^\]]*";
        private const string NotFoundPattern = @"\b(?:not found|unable to find|couldn['’]t find|no (?:data found|bookings found|meal options available|seat\s*offers found|results|flights are available)|nothing matches|requested (?:item|resource) does not exist)\b";
        private static readonly Lazy<IReadOnlySet<string>> NotFoundCodes = new(LoadNotFoundCodes);
        private readonly IServiceCollection _serviceCollection;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<HttpMethod, RouteMapping[]> _routes = [];
        private readonly IEnumerable<Tool> _tools = [];
        private readonly IEnumerable<ResolverType> _resolvers = [];
        private readonly string _capabilityStatement;


        public Tool[] Tools => [.. _tools];
        public IReadOnlyDictionary<HttpMethod, RouteMapping[]> Routes => _routes;


        public Router(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var types = _serviceCollection.Where(x => x.ServiceType.IsClass && !x.ServiceType.IsInterface && RegexCompiled.IsMatch(x.ServiceType.FullName, VaaTypeMatchPattern))
                                          .ToArray();

            var queriesAndMutations = types.Where(x => RegexCompiled.IsMatch(x.ServiceType.Name, QueryMutationMatchPattern))
                                           .ToArray();

            _routes = queriesAndMutations.GroupBy(k => k.ServiceType.FullName)
                                         .Select(x => new
                                         {
                                             x.Key,
                                             Value = x.SelectMany(v => v.ServiceType.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance)
                                                                                    .Where(m => m.GetCustomAttribute<GraphQLIgnoreAttribute>() == null)
                                                                                    .Select(x => (RouteMapping)x)
                                                                                    .ToArray())
                                         .DistinctBy(v => v.Name)
                                         .ToArray()
                                         })
                                         .SelectMany(x => x.Value)
                                         .GroupBy(x => x.Verb)
                                         .ToDictionary(k => k.Key, v => v.ToArray());

            _tools = _routes.Values.SelectMany(x => x)
                                   .Select(route => new Tool()
                                   {
                                       Name = route.Name,
                                       Description = route.Description,
                                       Parameters = route.Parameters.Where(x => x.ParameterType.GetCustomAttribute<ServiceAttribute>() is null && (!x.ParameterType.IsInterface || (x.ParameterType.IsInterface && x.ParameterType.Name.StartsWith("IEnumerable`"))) && x.ParameterType != typeof(CancellationToken))
                                                                    .Select(p => new Parameter()
                                                                    {
                                                                        Name = p.Name,
                                                                        Type = p.ParameterType.Name,
                                                                        Description = (p.GetCustomAttributes<GraphQLDescriptionAttribute>().FirstOrDefault() as GraphQLDescriptionAttribute)?.Description ?? string.Empty
                                                                    })
                                                                    .ToArray(),
                                       InputSchema = JToken.Parse(route.RequestSchema),
                                       AuthorizedRoles = route.AuthorizeAttribute?.Roles ?? [],
                                       RequiresAuthentication = route.AuthorizeAttribute is not null,
                                       RequiresMcpCallerContext = route.RequiresMcpCallerContext
                                   })
                                   .ToArray();

            _resolvers = types.Where(x => x.ServiceType.BaseType.FullName.StartsWith("HotChocolate.Types.ObjectType`1[[", StringComparison.OrdinalIgnoreCase))
                              .Select(x =>
                              {
                                  var targetType = x.ServiceType.BaseType.GenericTypeArguments[0];
                                  var mutationOrQueryType = _routes.Values.SelectMany(x => x).FirstOrDefault(x => x.ReturnType == targetType || (x.ReturnType.IsGenericType && x.ReturnType.GenericTypeArguments[0] == targetType)).ServiceType;
                                  var configuredMethod = x.ServiceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name.Is("Configure") && x.IsFamily && x.IsVirtual);

                                  return new ResolverType
                                  {
                                      QueryMutationType = mutationOrQueryType,
                                      ServiceType = x.ServiceType,
                                      For = targetType,
                                      ConfigureMethod = configuredMethod
                                  };
                              })
                              .ToArray();

            var stringBuilder = new StringBuilder();

            foreach (var item in _routes.Values.SelectMany(x => x).OrderBy(x => x.Name).ThenBy(x => x.Verb.Method))
                stringBuilder.AppendLine($"| [/api/v1/{item.Name}](/api/v1/{item.Name}) | [OPTIONS, {item.Verb}] |");

            _capabilityStatement = Properties.Resources.CapabilityTemplate.Replace("{capability}", stringBuilder.ToString());

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Routes:");

            foreach (var route in _routes.Values.SelectMany(x => x.Select(r => $"[OPTIONS, {r.Verb}] /api/v1/{r.Name}")).Order().ToArray())
                Console.WriteLine(route);
#endif
        }


        public async Task<HttpResponseData> Route(HttpRequestData request, string? name)
        {
            var verb = HttpMethod.Parse(request.Method);
            HttpResponseData response;

            if (verb == HttpMethod.Options)
            {
                if (name.IsNullOrWhiteSpace())
                {
                    response = request.CreateResponse(System.Net.HttpStatusCode.OK);

                    response.Headers.AddOrUpdate("content-type", "text/markdown");

                    response.WriteString(_capabilityStatement);

                    return response;
                }

                var mapping = _routes.Values.SelectMany(x => x).FirstOrDefault(x => x.Name.Is(name));

                if (mapping != null)
                {
                    response = request.CreateResponse(System.Net.HttpStatusCode.OK);

                    response.Headers.AddOrUpdate("content-type", "text/markdown");

                    var template = mapping.Documentation.Replace("{endpoint}", request.Url.ToString());

                    response.WriteString(template);

                    return response;
                }
            }
            else if (_routes.TryGetValue(verb, out var mappings))
            {
                var mapping = mappings.FirstOrDefault(x => x.Name.Is(name));

                if (mapping is null)
                {
                    response = request.CreateResponse(System.Net.HttpStatusCode.NotFound);

                    response.Headers.AddOrUpdate("content-type", "text/plain");

                    response.WriteString($"No route found for '{name}'.");

                    return response;
                }

                var requestBody = await request.ReadAsStringAsync();

                try
                {
                    var result = await Invoke(requestBody, mapping);

                    if (result is null)
                        return CreateNotFoundResponse(request, "No result was returned.", ((int)System.Net.HttpStatusCode.NotFound).ToString());

                    response = request.CreateResponse(System.Net.HttpStatusCode.OK);

                    response.Headers.AddOrUpdate("content-type", "application/json");

                    response.WriteString(Json.Serialize(result));

                    return response;
                }
                catch (UnauthorizedAccessException ex) when (ex.Message.Is(nameof(System.Net.HttpStatusCode.Unauthorized)) || ex.Message.Is(nameof(System.Net.HttpStatusCode.Forbidden)))
                {
                    response = request.CreateResponse(ex.Message.Is(nameof(System.Net.HttpStatusCode.Unauthorized)) ? System.Net.HttpStatusCode.Unauthorized : System.Net.HttpStatusCode.Forbidden);

                    response.Headers.AddOrUpdate("content-type", "text/plain");

                    response.WriteString(ex.Message);

                    return response;
                }
                catch (ResolverReportedErrorException ex) when (IsNotFound(ex.Errors))
                {
                    return CreateNotFoundResponse(request, ex.Errors);
                }
                catch (Exception ex) when (TryGetGraphQLHttpStatusCode(ex, out var errors, out var statusCode))
                {
                    return CreateGraphQLErrorResponse(request, errors, statusCode);
                }
                catch (Exception ex) when (IsNotFound(ex))
                {
                    return CreateNotFoundResponse(request, "No result was found.", ((int)System.Net.HttpStatusCode.NotFound).ToString());
                }
                catch (InvalidOperationException ex)
                {
                    response = request.CreateResponse(System.Net.HttpStatusCode.BadRequest);

                    response.Headers.AddOrUpdate("content-type", "text/plain");

                    response.WriteString(ex.Message);

                    return response;
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(System.Net.HttpStatusCode.InternalServerError);

                    response.Headers.AddOrUpdate("content-type", "text/plain");

                    response.WriteString(ex.Message);

                    return response;
                }
            }

            response = request.CreateResponse(System.Net.HttpStatusCode.NotFound);

            response.Headers.AddOrUpdate("content-type", "text/plain");

            response.WriteString($"No route found for verb '{verb}' and name '{name}'.");

            return response;
        }

        public async Task<Response> Mcp(HttpRequestData request)
        {
            var requestBody = await request.ReadAsStringAsync();

            Request mcpRequest;

            try
            {
                if (requestBody.IsNullOrWhiteSpace()) throw new JsonException("Request body is empty.");

                mcpRequest = Json.Deserialize<Request>(requestBody);
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Jsonrpc = "2.0",
                    Id = 0,
                    Error = new Error
                    {
                        Code = -32700,
                        Message = $"Parse error: Invalid JSON was received by the server. {ex.Message}"
                    }
                };
            }

            if (mcpRequest.Params is null)
                return new Response
                {
                    Id = mcpRequest.Id,
                    Error = new Error
                    {
                        Code = -32602,
                        Message = "Missing 'params' property."
                    }
                };

            var name = mcpRequest.Params["name"]?.ToString();
            var arguments = mcpRequest.Params["arguments"]?.ToString();

            if (name.IsNullOrWhiteSpace())
                return new Response
                {
                    Id = mcpRequest.Id,
                    Error = new Error
                    {
                        Code = -32602,
                        Message = "Missing 'name' parameter."
                    }
                };

            var mapping = _routes.Values.SelectMany(x => x)
                                        .FirstOrDefault(x => x.Name.Is(name));

            if (mapping is null) return new Response
            {
                Id = mcpRequest.Id,
                Error = new Error
                {
                    Code = -32601,
                    Message = $"Method '{name}' not found."
                }
            };

            try
            {
                var result = await Invoke(arguments, mapping);

                if (result is HttpResponseData httpResponseData)
                {
                    using var reader = new StreamReader(httpResponseData.Body);

                    var responseContent = await reader.ReadToEndAsync();

                    if (httpResponseData.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return new Response
                        {
                            Id = mcpRequest.Id,
                            Result = Json.Deserialize<JToken>(responseContent)
                        };
                    }
                    else
                    {
                        return new Response
                        {
                            Id = mcpRequest.Id,
                            Error = new Error
                            {
                                Code = (int)httpResponseData.StatusCode,
                                Message = responseContent
                            }
                        };
                    }
                }

                return new Response
                {
                    Id = mcpRequest.Id,
                    Result = Json.Deserialize<JToken>(Json.Serialize(result))
                };
            }
            catch (Exception ex) when (TryGetExplicitTransportStatus(ex, out var errors, out var statusCode))
            {
                return new Response
                {
                    Id = mcpRequest.Id,
                    Error = new Error
                    {
                        Code = (int)statusCode,
                        Message = errors[0].Message,
                        Data = Json.Deserialize<JToken>(Json.Serialize(new { errors = errors.Select(error => new { error.Message, error.Extensions }) }))
                    }
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                return new Response
                {
                    Id = mcpRequest.Id,
                    Error = new Error
                    {
                        Code = ex.Message.Is(nameof(System.Net.HttpStatusCode.Unauthorized)) ? (int)System.Net.HttpStatusCode.Unauthorized : (int)System.Net.HttpStatusCode.Forbidden,
                        Message = ex.Message
                    }
                };
            }
            catch (InvalidOperationException ex)
            {
                return new Response
                {
                    Id = mcpRequest.Id,
                    Error = new Error
                    {
                        Code = (int)System.Net.HttpStatusCode.BadRequest,
                        Message = ex.Message
                    }
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Id = mcpRequest.Id,
                    Error = new Error
                    {
                        Code = (int)System.Net.HttpStatusCode.InternalServerError,
                        Message = ex.Message
                    }
                };
            }
        }

        public async Task<object> Invoke(string requestBody, RouteMapping mapping)
        {
            if (mapping.RequiresMcpCallerContext)
            {
                var context = AsyncContext.Current?.Context;
                var tool = new Tool
                {
                    RequiresMcpCallerContext = true,
                    RequiresAuthentication = mapping.AuthorizeAttribute is not null,
                    AuthorizedRoles = mapping.AuthorizeAttribute?.Roles ?? []
                };
                if (!tool.HasRequiredCallerContext(context?.GetJwtPrincipalFeature()?.Principal,
                    context?.Features.Get<McpCallerContextFeature>()?.Principal))
                    throw new UnauthorizedAccessException("Unauthorized");
            }

            var service = _serviceProvider.GetService(mapping.ServiceType);

            if (service is null) throw new InvalidOperationException($"Service of type '{mapping.ServiceType.FullName}' not found.");

            // Before invoking the method, we need to check if the user is authorized to access it. This is done by checking
            // if the method has an AuthorizeAttribute, and if so, we check if the user is authenticated and if they have the
            // required roles (if any). If the user is not authenticated or does not have the required roles, we throw an
            // UnauthorizedAccessException with a message indicating whether it's an unauthorized or forbidden error. This
            // ensures that only authorized users can access the method, and provides clear error messages for unauthorized
            // access attempts.

            if (mapping.AuthorizeAttribute != null)
            {
                var principal = AsyncContext.Current.Context.GetJwtPrincipalFeature()?.Principal;

                if (!principal?.Identity?.IsAuthenticated ?? true) throw new UnauthorizedAccessException("Unauthorized");

                if (!mapping.AuthorizeAttribute.Roles.IsNullOrEmpty() && !mapping.AuthorizeAttribute.Roles.Any(role => principal.IsInRole(role.Trim())))
                    throw new UnauthorizedAccessException("Forbidden");
            }

            object[] args = mapping.Parameters.Length == 1 && mapping.Parameters[0].ParameterType == typeof(CancellationToken) ? [new CancellationToken()] : [];

            JObject deserialized = [];

            try
            {
                if (!requestBody.IsNullOrWhiteSpace())
                    deserialized = Json.Deserialize<JObject>(requestBody);
            }
            catch
            {
                if (mapping.Parameters.Any(p => p.ParameterType != typeof(CancellationToken)
                                             && p.ParameterType != typeof(IResolverContext)
                                             && p.GetCustomAttribute<ServiceAttribute>() is null
                                             && (!p.ParameterType.IsInterface || (p.ParameterType.IsInterface && p.ParameterType.Name.StartsWith("IEnumerable`")))
                                             && !p.HasDefaultValue))
                    throw new InvalidOperationException("Invalid request body. Expected a valid JSON object matching the method parameters.");

                // If the body is not a valid JSON object, but the method has parameters that can be populated from dependency injection or resolver context,
                // we can ignore the invalid body and attempt to invoke the method with default values for the parameters. This allows for methods that rely
                // solely on dependency injection or resolver context to be invoked without requiring a valid JSON body.
            }

            // We need to populate the arguments for the method invocation. This is done by iterating over the parameters
            // of the method and checking if they can be populated from the request body, from dependency injection,
            // or from the resolver context. If a parameter is of type CancellationToken, we provide a new instance of
            // CancellationToken. If a parameter is marked with the ServiceAttribute or is an interface (excluding IEnumerable<>),
            // we attempt to resolve it from the service provider. If a parameter is of type IResolverContext, we create an instance
            // of the resolver context and pass it to the method. For parameters that are not populated from these sources, we
            // attempt to populate them from the deserialized JSON body, using the parameter name as the key.
            args = [.. mapping.Parameters.Select(p =>
                                             {
                                                 if (p.ParameterType == typeof(CancellationToken)) return new CancellationToken();

                                                 var isDependencyInjection = p.GetCustomAttribute<ServiceAttribute>() is not null || (p.ParameterType.IsInterface && !p.ParameterType.Name.StartsWith("IEnumerable`"));
                                                 var isResolverContext =  p.ParameterType == typeof(IResolverContext);

                                                 if (isDependencyInjection && !isResolverContext) return _serviceProvider.GetService(p.ParameterType);

                                                 if (isResolverContext)
                                                 {
                                                     var instanceType = typeof(ResolverContext<>).MakeGenericType(p.Member.DeclaringType);
                                                     var loggerType = typeof(ILogger<>).MakeGenericType(p.Member.DeclaringType);

                                                     var logger = _serviceProvider.GetService(loggerType);

                                                     var instance = Activator.CreateInstance(instanceType, _serviceProvider, this, logger);

                                                     return instance;
                                                 }

                                                 var token = deserialized[p.Name];

                                                 return token != null ? token.ToObject(p.ParameterType) : p.HasDefaultValue ? p.DefaultValue : null;
                                             })];

            // Validate that all required parameters are provided (i.e., non-nullable parameters without default values)
            // We skip parameters that are populated via dependency injection or resolver context, as well as CancellationToken
            // parameters, since they can be optional and have a default value. This validation ensures that the method is
            // not invoked with missing required parameters, which would lead to runtime errors. By checking for null
            // values and default values, we can provide clear error messages indicating which parameters are missing,
            // improving the robustness of the API.
            for (int i = 0; i < mapping.Parameters.Length; i++)
            {
                var param = mapping.Parameters[i];
                var arg = args[i];

                var isDependencyInjection = param.GetCustomAttribute<ServiceAttribute>() is not null || (param.ParameterType.IsInterface && !param.ParameterType.Name.StartsWith("IEnumerable`"));
                var isResolverContext = param.ParameterType == typeof(IResolverContext);

                if (isDependencyInjection || isResolverContext || param.ParameterType == typeof(CancellationToken))
                    continue;

                // Value types (except Nullable<T>) cannot be null
                if (arg == null && param.ParameterType.IsValueType && Nullable.GetUnderlyingType(param.ParameterType) == null)
                    throw new InvalidOperationException($"Missing required parameter '{param.Name}' of type '{param.ParameterType.Name}'.");

                // Reference types without default value
                if (arg == null && !param.HasDefaultValue && !param.ParameterType.IsValueType)
                    throw new InvalidOperationException($"Missing required parameter '{param.Name}' of type '{param.ParameterType.Name}'.");
            }

            var result = mapping.MethodInfo.Invoke(service, args);
            object response;

            if (result is Task task)
            {
                await task;

                response = task.GetType().GetProperty("Result")?.GetValue(task);
            }
            else
                response = result;

            var resolverContextInstance = args.FirstOrDefault(x => x is IResolverContext) as IResolverContext;

            if (resolverContextInstance != null && resolverContextInstance.HasErrors && resolverContextInstance.ContextData.TryGetValue("Errors", out object errors))
                throw new ResolverReportedErrorException(((IEnumerable<IError>)errors).ToArray());

            if (response is not null)
            {
                var resolver = _resolvers.FirstOrDefault(x => x.For == response.GetType());

                if (resolver is not null)
                {
                    // If there is a resolver for the response type, we need to invoke it to populate the fields of the response object.
                    // This is done by creating an instance of the resolver context and the object type descriptor, and then invoking the
                    // Configure method of the resolver to populate the fields of the response object. This is necessary because the
                    // response object may have fields that are not populated by the initial method invocation, and the resolver is
                    // responsible for populating those fields.

                    var instanceType = typeof(ResolverContext<>).MakeGenericType(resolver.QueryMutationType);
                    var loggerType = typeof(ILogger<>).MakeGenericType(resolver.QueryMutationType);

                    var logger = _serviceProvider.GetService(loggerType);

                    var resolverContext = (IResolverContext)Activator.CreateInstance(instanceType, _serviceProvider, this, logger);

                    resolverContext.ContextData.Add($"Parent:{response.GetType().FullName}", response);

                    var objectFieldDescriptor = Activator.CreateInstance(typeof(Emulate.ObjectTypeDescriptor<>).MakeGenericType(resolver.For), resolverContext);

                    var configureMethod = resolver.ServiceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name.Is("Configure") && x.IsFamily && x.IsVirtual);

                    var instance = _serviceProvider.GetRequiredService(resolver.ServiceType);

                    configureMethod.Invoke(instance, [objectFieldDescriptor]);

                    var fields = ((IObjectTypeDescriptorFields)objectFieldDescriptor).FieldDescriptors;

                    foreach (var field in fields)
                    {
                        var fieldType = field.GetType();
                        var expression = fieldType.GetProperty("PropertyOrMethod", BindingFlags.Instance | BindingFlags.Public).GetValue(field);
                        var body = expression.GetType().GetProperty("Body").GetValue(expression);
                        var member = body.GetType().GetProperty("Member").GetValue(body);
                        var name = member.GetType().GetProperty("Name").GetValue(member)?.ToString() ?? string.Empty;
                        var targetProperty = resolver.For.GetProperty(name);
                        var authorizeAttribute = targetProperty.GetCustomAttribute<AuthorizeAttribute>();

                        if (authorizeAttribute != null)
                        {
                            var principal = AsyncContext.Current.Context.GetJwtPrincipalFeature()?.Principal;

                            var canAccess = (principal?.Identity?.IsAuthenticated ?? false) && (authorizeAttribute.Roles.IsNullOrEmpty() || authorizeAttribute.Roles.Any(role => principal.IsInRole(role.Trim())));

                            if (!canAccess) continue;
                        }

                        var applyMethod = fieldType.GetMethod("Apply");
                        var applyMethodResult = applyMethod.Invoke(field, [targetProperty.PropertyType]);

                        object? awaitedResult = null;
                        var resultType = applyMethodResult?.GetType();

                        if (resultType != null)
                        {
                            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                            {
                                var resultProperty = resultType.GetProperty("Result");

                                if (resultProperty != null)
                                    awaitedResult = resultProperty.GetValue(applyMethodResult);
                                else
                                {
                                    var asTaskResult = (Task)resultType.GetMethod("AsTask")
                                                                       .Invoke(applyMethodResult, null);

                                    await asTaskResult.ConfigureAwait(false);

                                    var taskResultProperty = asTaskResult.GetType()
                                                                         .GetProperty("Result");

                                    awaitedResult = taskResultProperty.GetValue(asTaskResult);
                                }

                                targetProperty.SetValue(response, awaitedResult);
                            }
                            else if (applyMethodResult is Task applyMethodResultTask)
                            {
                                await applyMethodResultTask.ConfigureAwait(false);

                                awaitedResult = applyMethodResultTask.GetType()
                                                                     .GetProperty("Result")?
                                                                     .GetValue(applyMethodResultTask);

                                targetProperty.SetValue(response, awaitedResult);
                            }
                            else if (applyMethodResult is ValueTask valueTask)
                            {
                                await valueTask.ConfigureAwait(false);

                                targetProperty.SetValue(response, null);
                            }
                            else
                                targetProperty.SetValue(response, applyMethodResult);

                        }
                    }
                }
            }

            return response;
        }

        private static bool IsNotFound(IReadOnlyList<IError> errors)
        {
            var httpStatusCodes = errors.SelectMany(GetErrorCodes)
                                        .Select(value => int.TryParse(value, out var statusCode) ? statusCode : 0)
                                        .Where(statusCode => statusCode is >= 400 and <= 599)
                                        .ToArray();

            if (httpStatusCodes.Any(statusCode => statusCode != (int)System.Net.HttpStatusCode.NotFound))
                return false;

            var notFoundCodes = NotFoundCodes.Value;

            return errors.Any(error => IsNotFound(error, notFoundCodes));
        }

        private static bool IsNotFound(IError error, IReadOnlySet<string> notFoundCodes)
        {
            var codes = GetErrorCodes(error).ToArray();

            if (codes.Any(code => code.Is(((int)System.Net.HttpStatusCode.NotFound).ToString()) || notFoundCodes.Contains(code)))
                return true;

            if (codes.Any(code => int.TryParse(code, out var statusCode) && statusCode is >= 400 and <= 599))
                return false;

            return IsNotFoundMessage(error.Message) || error.Exception is not null && IsNotFound(error.Exception, notFoundCodes);
        }

        private static IEnumerable<string> GetErrorCodes(IError error)
        {
            if (!error.Code.IsNullOrWhiteSpace())
                yield return error.Code;

            if (error.Extensions?.TryGetValue("code", out var extensionCode) == true && extensionCode is not null)
                yield return extensionCode.ToString()!;
        }

        private static bool TryGetGraphQLHttpStatusCode(Exception exception, out IReadOnlyList<IError> errors, out System.Net.HttpStatusCode statusCode)
        {
            if (TryGetExplicitTransportStatus(exception, out errors, out statusCode))
                return true;

            for (var current = exception; current is not null; current = current.InnerException)
            {
                errors = current switch
                {
                    ResolverReportedErrorException resolverReportedErrorException => resolverReportedErrorException.Errors,
                    GraphQLException graphQLException => graphQLException.Errors,
                    _ => null
                };

                if (errors is null)
                    continue;

                var codes = errors.SelectMany(GetErrorCodes)
                                  .Select(value => int.TryParse(value, out var parsedCode) ? parsedCode : 0)
                                  .Where(value => value is >= 400 and <= 599)
                                  .ToArray();
                var code = codes.FirstOrDefault(value => value != (int)System.Net.HttpStatusCode.NotFound);

                if (code == 0)
                    code = codes.FirstOrDefault();

                if (code != 0)
                {
                    statusCode = (System.Net.HttpStatusCode)code;
                    return true;
                }
            }

            errors = [];
            statusCode = System.Net.HttpStatusCode.InternalServerError;

            return false;
        }

        // Explicit opt-in: existing resolver error contracts keep their legacy transport behavior.
        private static bool TryGetExplicitTransportStatus(Exception exception, out IReadOnlyList<IError> errors, out System.Net.HttpStatusCode statusCode)
        {
            for (var current = exception; current is not null; current = current.InnerException)
            {
                errors = current switch
                {
                    GraphQLException graphQLException => graphQLException.Errors,
                    ResolverReportedErrorException reported => reported.Errors,
                    _ => null
                };
                if (errors is null) continue;
                foreach (var error in errors)
                {
                    if (error.Extensions?.TryGetValue("transportStatusCode", out var value) == true &&
                        value is int code && code is >= 400 and <= 599)
                    {
                        statusCode = (System.Net.HttpStatusCode)code;
                        return true;
                    }
                }
            }
            errors = [];
            statusCode = System.Net.HttpStatusCode.InternalServerError;
            return false;
        }

        private static bool IsNotFound(Exception exception) => IsNotFound(exception, NotFoundCodes.Value);

        private static bool IsNotFound(Exception exception, IReadOnlySet<string> notFoundCodes)
        {
            for (var current = exception; current is not null; current = current.InnerException)
            {
                if (current is ResolverReportedErrorException resolverReportedErrorException)
                    return IsNotFound(resolverReportedErrorException.Errors);

                if (current is HttpRequestException { StatusCode: System.Net.HttpStatusCode.NotFound } ||
                    current is Common.Exceptions.HttpResponseException { Response.StatusCode: System.Net.HttpStatusCode.NotFound } ||
                    current.Message.Is(((int)System.Net.HttpStatusCode.NotFound).ToString()) ||
                    notFoundCodes.Contains(current.Message) ||
                    IsNotFoundMessage(current.Message))
                    return true;
            }

            return false;
        }

        private static IReadOnlySet<string> LoadNotFoundCodes()
        {
            try
            {
                var errorFileData = ConfigReader.ReadAllText(Common.Constants.Application.FunctionAppDirPath, Common.Constants.Application.ErrorFilePath);
                var configuredErrors = Json.Deserialize<List<Common.Entities.Error>>(errorFileData) ?? [];

                return configuredErrors.Where(error => IsNotFoundMessage($"{error.Message} {error.DeveloperMessage}"))
                                       .Select(error => error.Code)
                                       .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is IOException or JsonException or ArgumentException or UnauthorizedAccessException)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static bool IsNotFoundMessage(string message) => RegexCompiled.IsMatch(message, NotFoundPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static HttpResponseData CreateNotFoundResponse(HttpRequestData request, IReadOnlyList<IError> errors) => CreateGraphQLErrorResponse(request, errors, System.Net.HttpStatusCode.NotFound);

        private static HttpResponseData CreateGraphQLErrorResponse(HttpRequestData request, IReadOnlyList<IError> errors, System.Net.HttpStatusCode statusCode)
        {
            var response = request.CreateResponse(statusCode);

            response.Headers.AddOrUpdate("content-type", "application/json");

            response.WriteString(Json.Serialize(new
            {
                Errors = errors.Select(error => new
                {
                    error.Message,
                    Extensions = error.Extensions?.ContainsKey("transportStatusCode") == true
                        ? (object)error.Extensions
                        : new { Code = GetErrorCodes(error).FirstOrDefault() }
                })
            }));

            return response;
        }

        private static HttpResponseData CreateNotFoundResponse(HttpRequestData request, string message, string code)
        {
            var response = request.CreateResponse(System.Net.HttpStatusCode.NotFound);

            response.Headers.AddOrUpdate("content-type", "application/json");

            response.WriteString(Json.Serialize(new
            {
                Errors = new[]
                {
                    new
                    {
                        Message = message,
                        Extensions = new { Code = code }
                    }
                }
            }));

            return response;
        }
    }
}
