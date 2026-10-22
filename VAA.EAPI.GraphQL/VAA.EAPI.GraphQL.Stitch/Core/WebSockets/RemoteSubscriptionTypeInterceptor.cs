using HotChocolate;
using HotChocolate.Configuration;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Stitching;
using HotChocolate.Stitching.Delegation;
using HotChocolate.Stitching.Utilities;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Stitch.Core.WebSockets
{
    public sealed class RemoteSubscriptionTypeInterceptor : TypeInterceptor
    {
        public override void OnAfterInitialize(ITypeDiscoveryContext context, DefinitionBase definition)
        {
            if (definition is not ObjectTypeDefinition { Name: "Subscription" } subscription)
            {
                return;
            }

            foreach (var field in subscription.Fields)
            {
                var directive = field.Directives.Select(x => x.Value).OfType<DirectiveNode>()
                    .FirstOrDefault(x => x.Name.Value == "delegate");
                var runtimeDirective = field.Directives.Select(x => x.Value).OfType<DelegateDirective>().FirstOrDefault();
                var schema = runtimeDirective?.Schema.ToString() ??
                    (directive?.Arguments.FirstOrDefault(x => x.Name.Value == "schema")?.Value as StringValueNode)?.Value;
                if (schema is null)
                {
                    continue;
                }

                // Only root delegation is supported. Explicit nested delegate paths need their own resolver.
                var path = runtimeDirective?.Path ??
                    (directive?.Arguments.FirstOrDefault(x => x.Name.Value == "path")?.Value as StringValueNode)?.Value;
                var remoteField = ParseRootPath(path);

                var targetSchema = schema;
                field.SubscribeResolver = resolver => SubscribeAsync(resolver, targetSchema, remoteField);
                // Register this interceptor before AddRemoteSchema. Remove only query delegation;
                // the normal field pipeline (including authorization) and child resolvers still run.
                foreach (var delegation in field.Directives.Where(x => x.Value == directive || x.Value == runtimeDirective).ToArray())
                {
                    field.Directives.Remove(delegation);
                }
                field.Resolver = resolver =>
                {
                    var message = resolver.GetEventMessage<RemoteSubscriptionEvent>();
                    foreach (var error in message.Errors)
                    {
                        resolver.ReportError(error.Path is null ? ErrorBuilder.FromError(error).SetPath(resolver.Path).Build() : error);
                    }
                    message.Data.TryGetValue(resolver.ResponseName, out var value);
                    // List/non-null wrappers also implement IInputType around output objects.
                    // Only scalar/enum results should enter the input parser.
                    var result = resolver.Selection.Type.NamedType() is IInputType && resolver.Selection.Type is IInputType input
                        ? resolver.Service<InputParser>().ParseResult(value, input, resolver.Path)
                        : value;
                    return new ValueTask<object>(result);
                };
            }
        }

        private static FieldNode ParseRootPath(string path)
        {
            if (string.IsNullOrEmpty(path)) { return null; }
            // Hot Chocolate generates $arguments:name mappings when it prefixes conflicting root fields.
            // Parse only one root field; never silently reinterpret nested delegation as a root subscription.
            try
            {
                var parsed = Utf8GraphQLParser.Parse("{ " + path.Replace("$arguments:", "$", StringComparison.Ordinal) + " }");
                var selections = parsed.Definitions.OfType<OperationDefinitionNode>().Single().SelectionSet.Selections;
                if (selections.Count == 1 && selections[0] is FieldNode { SelectionSet: null, Alias: null } field)
                {
                    return field;
                }
            }
            catch (SyntaxException) { }
            throw new InvalidOperationException("Remote subscriptions support only root delegation and argument mappings.");
        }

        private static async ValueTask<HotChocolate.Execution.ISourceStream> SubscribeAsync(IResolverContext context, string schema, FieldNode remoteField)
        {
            var rewriter = new ExtractFieldQuerySyntaxRewriter(context.Schema,
                context.Service<IEnumerable<IQueryDelegationRewriter>>());
            var extracted = rewriter.ExtractField(schema, context.Operation.Document, context.Operation.Definition,
                context.Selection, context.ObjectType);
            var fields = extracted.SyntaxNodes.Select(field => remoteField is null ? field :
                field.WithName(remoteField.Name).WithAlias(field.Alias ?? field.Name)
                    .WithArguments(remoteField.Arguments.Select(argument => argument.Value is VariableNode variable
                        ? argument.WithValue(field.Arguments.FirstOrDefault(x => x.Name.Value == variable.Name.Value)?.Value
                            ?? context.ArgumentLiteral<IValueNode>(variable.Name.Value))
                        : argument).ToArray())).ToArray();
            var operation = context.Operation.Definition
                .WithSelectionSet(new SelectionSetNode(fields))
                .WithVariableDefinitions(extracted.Variables.Select(variable => variable.WithType(
                    RemoteType(variable.Type, context.Schema, schema))).ToArray());
            var document = new DocumentNode(new IDefinitionNode[] { operation }.Concat(extracted.Fragments).ToArray());
            var variables = new JObject();
            foreach (var variable in extracted.Variables)
            {
                if (context.Variables.TryGetVariable<IValueNode>(variable.Variable.Name.Value, out var value))
                {
                    var inputType = (IInputType)variable.Type.ToType(context.Schema.GetType<INamedInputType>(variable.Type.NamedType().Name.Value));
                    variables[variable.Variable.Name.Value] = ToJson(rewriter.RewriteValueNode(schema, inputType, value));
                }
            }

            var headers = context.ContextData.TryGetValue(RemoteSubscriptionEndpoints.HeadersProperty, out var captured)
                ? (IReadOnlyDictionary<string, string>)captured
                : new Dictionary<string, string>();
            var payload = new JObject
            {
                ["query"] = document.ToString(),
                ["operationName"] = operation.Name?.Value,
                ["variables"] = variables
            };
            var sse = headers.TryGetValue("Accept", out var accept) && accept == "text/event-stream";
            var endpoint = context.Service<RemoteSubscriptionEndpoints>().Get(schema, sse);
            if (sse) { return await RemoteSseSubscriptionStream.ConnectAsync(endpoint, payload, headers, context.RequestAborted); }
            return await RemoteSubscriptionStream.ConnectAsync(endpoint, payload, headers, context.RequestAborted);
        }

        private static ITypeNode RemoteType(ITypeNode type, ISchema schema, string targetSchema) => type switch
        {
            NonNullTypeNode required => new NonNullTypeNode((INullableTypeNode)RemoteType(required.Type, schema, targetSchema)),
            ListTypeNode list => new ListTypeNode(RemoteType(list.Type, schema, targetSchema)),
            NamedTypeNode named => new NamedTypeNode(((HotChocolate.Types.IHasDirectives)schema.GetType<INamedInputType>(named.Name.Value)).Directives
                .Where(directive => directive.Type.Name == "source").Select(directive => directive.AsValue<SourceDirective>())
                .FirstOrDefault(source => source.Schema == targetSchema)?.Name ?? named.Name.Value),
            _ => throw new InvalidOperationException("Unsupported subscription variable type.")
        };

        private static JToken ToJson(IValueNode value) => value switch
        {
            NullValueNode => JValue.CreateNull(),
            StringValueNode text => new JValue(text.Value),
            EnumValueNode enumeration => new JValue(enumeration.Value),
            BooleanValueNode boolean => new JValue(boolean.Value),
            IntValueNode integer => new JValue(integer.ToInt64()),
            FloatValueNode number => new JValue(number.ToDouble()),
            ListValueNode list => new JArray(list.Items.Select(ToJson)),
            ObjectValueNode obj => new JObject(obj.Fields.Select(x => new JProperty(x.Name.Value, ToJson(x.Value)))),
            _ => throw new InvalidOperationException("Unsupported subscription variable value.")
        };
    }
}
