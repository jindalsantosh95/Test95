using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal sealed class ResolverContext<T> : IResolverContext
    {
        private static readonly ConcurrentDictionary<Type, OperationType> resolverTypes = [];
        private readonly Router _router;
        private readonly ILogger<T> _logger;

        private readonly VariableValueCollection _variables = new();
        private readonly IReadOnlyList<VariableDefinitionNode> _variableDefinitions;
        private readonly SelectionSetNode _selectionSet = new([]);
        private readonly ConcurrentDictionary<string, object> _contextData = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _typeCache = [];

        public ResolverContext(IServiceProvider serviceProvider, Router router, ILogger<T> logger)
        {
            _contextData.TryAdd("Errors", new ConcurrentBag<IError>());

            _router = router ?? throw new ArgumentNullException(nameof(router));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _variableDefinitions = _variables.Select(v => new VariableDefinitionNode(null, new VariableNode(null, new NameNode(v.Name)), new NamedTypeNode(new NameNode(v.Value.Kind.ToString())), null, []))
                                             .ToList();

            var mapping = _router.Routes.Values.SelectMany(x => x).FirstOrDefault(x => x.ServiceType == typeof(T));

            if (mapping is not null)
            {
                var type = mapping.MethodInfo.ReturnType;

                if (type.BaseType == typeof(Task) && type.IsGenericType)
                    type = type.GenericTypeArguments[0];

                if (!_typeCache.TryGetValue(type, out PropertyInfo[] properties))
                {
                    properties = type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);

                    _typeCache.TryAdd(type, properties);
                }

                Path = Path.New(mapping.Name);
                ResponseName = new NameString(mapping.Name);
                ResponseIndex = 0;

                var parsedSelectionSet = Utf8GraphQLParser.Syntax.ParseSelectionSet($"{{ {string.Join(" ", properties.Select(x => x.Name))} }}");

                _selectionSet = _selectionSet.WithSelections([.. _selectionSet.Selections, .. parsedSelectionSet.Selections]);

                var selections = parsedSelectionSet.Selections.OfType<FieldNode>().ToArray();

                if (selections.Length > 0)
                {
                    var objectType = new ObjectType(a =>
                    {
                        a.Name(type.Name);
                        a.Description($"Auto-generated type for {type.Name} returned by {typeof(T).Name}");

                        foreach (var selection in selections)
                            a.Field(selection.Name.Value);
                    });

                    Selection = new FieldSelection(
                        responseName: type.Name,
                        field: new ObjectField(0, selections[0].Name.Value, $"Auto-generated type for {type.Name} returned by {typeof(T).Name}", objectType, type), // You may want to resolve the actual IObjectField if available
                        type: objectType,
                        typeKind: TypeKind.Object,
                        syntaxNode: selections[0],
                        syntaxNodes: selections
                    );
                }
            }

            if (!resolverTypes.TryGetValue(typeof(T), out var operation))
            {
                var extendObjectTypeAttribute = typeof(T).GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false).FirstOrDefault() as ExtendObjectTypeAttribute ?? throw new InvalidOperationException($"The type {typeof(T).Name} does not have an ExtendObjectTypeAttribute.");

                operation = extendObjectTypeAttribute.Name switch
                {
                    OperationTypeNames.Mutation => OperationType.Mutation,
                    OperationTypeNames.Query => OperationType.Query,
                    _ => throw new InvalidOperationException($"Unsupported operation type: {extendObjectTypeAttribute.Name}")
                };

                resolverTypes.TryAdd(typeof(T), operation);
            }

            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Operation = new OperationDefinitionNode(null, new NameNode("Operation"), operation, _variableDefinitions, [], _selectionSet);
        }

        public IServiceProvider Services { get; set; }

        public IObjectField Field { get; }

        public DocumentNode Document { get; }

        public OperationDefinitionNode Operation { get; }

        public FieldNode FieldSelection { get; }

        public NameString ResponseName { get; } = default!;

        public int ResponseIndex { get; }

        public bool HasErrors => ContextData.TryGetValue("Errors", out object errors) && !((ConcurrentBag<IError>)errors).IsEmpty;

        public IImmutableDictionary<string, object> ScopedContextData { set; get; }
        public IImmutableDictionary<string, object> LocalContextData { set; get; }

        public CancellationToken RequestAborted => CancellationToken.None;

        public Path Path { get; } = default!;

        public ISchema Schema { get; }

        public IObjectType RootType { get; }

        public IObjectType ObjectType { get; }

        public IFieldSelection Selection { get; } = default!;

        public IVariableValueCollection Variables => _variables;

        public IDictionary<string, object> ContextData => _contextData;

        public T? Argument<T>(NameString name) => throw new NotImplementedException();

        public ValueKind ArgumentKind(NameString name) => throw new NotImplementedException();

        public TValueNode ArgumentLiteral<TValueNode>(NameString name) where TValueNode : IValueNode => throw new NotImplementedException();

        public Optional<T> ArgumentOptional<T>(NameString name) => throw new NotImplementedException();

        public T ArgumentValue<T>(NameString name) => throw new NotImplementedException();

        public T GetQueryRoot<T>() => throw new NotImplementedException();

        public IReadOnlyList<IFieldSelection> GetSelections(ObjectType typeContext, SelectionSetNode selectionSet = null, bool allowInternals = false)
        {
            RouteMapping mapping = null;

            if (_router.Routes.TryGetValue(HttpMethod.Post, out var postMappings))
                mapping = postMappings.FirstOrDefault(x => x.ServiceType == typeof(T));

            if (mapping is null && _router.Routes.TryGetValue(HttpMethod.Get, out var getMappings))
                mapping = getMappings.FirstOrDefault(x => x.ServiceType == typeof(T));

            if (mapping is not null)
            {
                var type = mapping.MethodInfo.ReturnType;

                if (type.BaseType == typeof(Task) && type.IsGenericType)
                    type = type.GenericTypeArguments[0];

                if (_typeCache.TryGetValue(type, out PropertyInfo[] properties))
                {
                    var parsedSelectionSet = Utf8GraphQLParser.Syntax.ParseSelectionSet($"{{ {string.Join(" ", properties.Select(x => x.Name))} }}");

                    var selections = parsedSelectionSet.Selections.OfType<FieldNode>();

                    IObjectType declaringType = new ObjectType((a) =>
                    {
                        a.Name(type.Name);
                        a.Description($"Auto-generated type for {type.Name} returned by {typeof(T).Name}");

                        foreach (var selection in selections)
                            a.Field(selection.Name.Value);

                    });

                    return [.. selections.Select((x, i) => new FieldSelection(x.Name.Value
                                                                         , new ObjectField(0, $"{char.ToLowerInvariant(x.Name.Value[0])}{x.Name.Value.Substring(1)}", $"Auto-generated type for {x.Name.Value} returned by {typeof(T).Name}", declaringType, type)
                                                                         , declaringType
                                                                         , TypeKind.Object
                                                                         , x
                                                                         , [x]))];
                }
            }

            return [];
        }

        public T Parent<T>() => ContextData.TryGetValue($"Parent:{typeof(T).FullName}", out object value) ? (T)value : default(T);

        public void ReportError(string errorMessage)
        {
            var error = ErrorBuilder.New().SetMessage(errorMessage).Build();

            ReportError(error);
        }

        public void ReportError(IError error)
        {
            if (ContextData.TryGetValue("Errors", out object errors))
                ((ConcurrentBag<IError>)errors).Add(error);

            _logger.LogError(error.Exception, error.Message);
        }

        public void ReportError(Exception exception, Action<IErrorBuilder> configure = null)
        {
            var errorBuilder = ErrorBuilder.New();

            if (configure is not null)
                configure(errorBuilder);
            else
                errorBuilder.SetException(exception);

            ReportError(errorBuilder.Build());
        }

        public T Resolver<T>() => throw new NotImplementedException();

        public object Service(Type service) => Services.GetService(service) ?? throw new InvalidOperationException($"Service of type {service.Name} is not registered.");

        public T Service<T>() => (T)Service(typeof(T));
    }
}
