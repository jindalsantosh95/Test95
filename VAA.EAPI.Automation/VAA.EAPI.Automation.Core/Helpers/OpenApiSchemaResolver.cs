using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;

public class OpenApiSchemaResolver
{
    private readonly JObject _openApiDocument;
    private readonly JObject _componentsSchemas;
    private readonly HashSet<string> _resolvingRefs; // Prevent circular references

    public OpenApiSchemaResolver(string openApiJson)
    {
        _openApiDocument = JObject.Parse(openApiJson);
        _componentsSchemas = _openApiDocument["components"]?["schemas"] as JObject;
        _resolvingRefs = new HashSet<string>();

        if (_componentsSchemas == null)
        {
            throw new InvalidOperationException("No components/schemas found in OpenAPI document");
        }
    }

    /// <summary>
    /// Resolves all $ref references in a schema and returns a flattened version
    /// </summary>
    /// <param name="schemaPath">Path to the schema (e.g., "tripsDetailsResponse")</param>
    /// <returns>Flattened JObject with all references resolved</returns>
    public JObject ResolveSchema(string schemaPath)
    {
        _resolvingRefs.Clear();
        JObject schema = GetSchemaByPath(schemaPath);

        if (schema == null)
        {
            throw new ArgumentException($"Schema '{schemaPath}' not found");
        }

        return (JObject)ResolveReferences(schema.DeepClone() as JObject);
    }

    /// <summary>
    /// Gets a schema by path from components/schemas
    /// </summary>
    private JObject GetSchemaByPath(string schemaPath)
    {
        return _componentsSchemas[schemaPath] as JObject;
    }

    /// <summary>
    /// Recursively resolves all $ref references in a JToken
    /// </summary>
    private JToken ResolveReferences(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                if (token["$ref"]?.ToString().Split('/').Last() == "dateOnly")
                {
                    JObject property = new JObject();
                    property.Add("type", "dateOnly");
                    return property;
                }
                else if (token["$ref"]?.ToString().Split('/').Last() == "timeOnly")
                {
                    JObject property = new JObject();
                    property.Add("type", "timeOnly");
                    return property;
                }
                else
                    return ResolveObjectReferences(token as JObject);

            case JTokenType.Array:
                return ResolveArrayReferences(token as JArray);

            default:
                return token;
        }
    }

    /// <summary>
    /// Resolves references in a JObject
    /// </summary>
    private JObject ResolveObjectReferences(JObject obj)
    {
        // Check if this object has a $ref property
        JToken refProperty = obj["$ref"];
        if (refProperty != null)
        {
            string refPath = refProperty.ToString();
            return ResolveReference(refPath);
        }

        // Process all properties in the object
        JObject result = new JObject();
        foreach (JProperty property in obj.Properties())
        {
            result[property.Name] = ResolveReferences(property.Value);
        }

        return result;
    }

    /// <summary>
    /// Resolves references in a JArray
    /// </summary>
    private JArray ResolveArrayReferences(JArray array)
    {
        JArray result = new JArray();
        foreach (JToken item in array)
        {
            result.Add(ResolveReferences(item));
        }
        return result;
    }

    /// <summary>
    /// Resolves a single $ref reference
    /// </summary>
    private JObject ResolveReference(string refPath)
    {
        // Check for circular reference
        if (_resolvingRefs.Contains(refPath))
        {
            // Return a placeholder for circular references
            return new JObject
            {
                ["__circular_ref__"] = refPath,
                ["type"] = "object",
                ["description"] = $"Circular reference to {refPath}"
            };
        }

        // Parse the reference path (e.g., "#/components/schemas/booking")
        string[] pathParts = refPath.TrimStart('#', '/').Split('/');

        if (pathParts.Length >= 3 && pathParts[0] == "components" && pathParts[1] == "schemas")
        {
            string schemaName = pathParts[2];
            JObject schema = _componentsSchemas[schemaName] as JObject;

            if (schema == null)
            {
                throw new InvalidOperationException($"Referenced schema '{schemaName}' not found");
            }

            // Add to resolving set to detect circular references
            _resolvingRefs.Add(refPath);

            try
            {
                // Recursively resolve references in the found schema
                JToken resolvedSchema = ResolveReferences(schema.DeepClone());
                return resolvedSchema as JObject;
            }
            finally
            {
                // Remove from resolving set
                _resolvingRefs.Remove(refPath);
            }
        }

        throw new InvalidOperationException($"Invalid reference path: {refPath}");
    }

    /// <summary>
    /// Pretty prints a resolved schema as JSON
    /// </summary>
    public string GetResolvedSchemaJson(string schemaPath, bool indented = true)
    {
        JObject resolvedSchema = ResolveSchema(schemaPath);
        return JsonConvert.SerializeObject(resolvedSchema, indented ? Formatting.Indented : Formatting.None);
    }
}

// Extension methods for easier usage
public static class SchemaResolverExtensions
{
    /// <summary>
    /// Gets a specific property path from a resolved schema
    /// </summary>
    public static JToken GetPropertyByPath(this JObject schema, string propertyPath)
    {
        string[] pathParts = propertyPath.Split('.');
        JToken current = schema;

        foreach (string part in pathParts)
        {
            if (current is JObject obj)
            {
                current = obj[part];
                if (current == null)
                    return null;
            }
            else
            {
                return null;
            }
        }

        return current;
    }
}
