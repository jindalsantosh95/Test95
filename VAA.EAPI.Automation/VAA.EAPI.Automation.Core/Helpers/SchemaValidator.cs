using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public static class SchemaValidator
    {
        private static readonly Regex schemaRegex = new("(schema|((type|interface|input|enum)\\s+([aA-zZ0-9]+)))([^{]+){([^}]+)}", RegexOptions.Compiled);

        /// <summary>
        /// Method to carry out the schema validations as per the passed in XSDs and request
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="schemas"></param>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static bool XsdSchemaValidator(string xml, string[] schemas, out string[] exceptions)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentNullException(nameof(xml), "Cannot be of null or empty value.");

            foreach (string[] schema in schemas.Select(x => x.Split('=')))
                if (!File.Exists(schema[1]))
                    throw new FileNotFoundException(schema[1]);

            List<string> messages = new();

            // configure document settings
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;

            foreach (string[] schema in schemas.Select(x => x.Split('=')))
                settings.Schemas.Add(schema[0], schema[1]);

            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += (sender, args) => messages.Add($"{args.Severity}: {args.Exception.Message}");

            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;

            using (XmlReader xmlReader = XmlReader.Create(XmlReader.Create(new StringReader(xml)), settings))
                while (xmlReader.Read())
                { }

            exceptions = messages.ToArray();

            return !exceptions.Any();
        }

        ///// <summary>
        ///// Method to compare the schema stored in the solution vs the schema returned from GQL Get call
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="currentSchema"></param>
        ///// <param name="exceptions"></param>
        ///// <returns></returns>
        //public static bool GraphQlSchemaValidator(string source, string currentSchema, out string[] exceptions)
        //{
        //    var current = schemaRegex.Matches(source).OrderBy(x => x.Value).ToArray();
        //    var @new = schemaRegex.Matches(currentSchema).OrderBy(x => x.Value).ToArray();

        //    // find all types that extist in both schemas but have a property/implementation change
        //    var changed = current.Where(x => @new.Any(y => y.Groups[4].Value.Equals(x.Groups[4].Value) && (!y.Groups[5].Value.Equals(x.Groups[5].Value) || !y.Groups[6].Value.Equals(x.Groups[6].Value))));
        //    var newChanges = @new.Where(x => changed.Any(y => y.Groups[4].Value.Equals(x.Groups[4].Value)));
        //    var differneces = changed.Zip(newChanges)
        //                             .Select(x => new
        //                             {
        //                                 Name = x.First.Groups[4].Value,
        //                                 ImplementationChanged = !x.First.Groups[5].Value.Equals(x.Second.Groups[5].Value),
        //                                 CurrentImplementation = x.First.Groups[5].Value,
        //                                 NewImplementation = x.Second.Groups[5].Value,
        //                                 RemovedProperties = x.First.Groups[6].Value.Split(Environment.NewLine).Except(x.Second.Groups[6].Value.Split(Environment.NewLine)).Select(x => x.Trim()).ToArray(),
        //                                 NewProperties = x.Second.Groups[6].Value.Split(Environment.NewLine).Except(x.First.Groups[6].Value.Split(Environment.NewLine)).Select(x => x.Trim()).ToArray()
        //                             })
        //                             .Select(x => $"{x.Name}{(x.ImplementationChanged ? $", Implementation changed from '{x.CurrentImplementation}' to '{x.NewImplementation}'" : string.Empty)}{(x.RemovedProperties.Any() ? $", Properties [{string.Join(',', x.RemovedProperties)}] removed" : string.Empty)}{(x.RemovedProperties.Any() ? $", Properties [{string.Join(',', x.NewProperties)}] introduced" : string.Empty)}")
        //                             .ToArray();

        //    // here we check if the types in production have been removed in the new schema
        //    var removed = current.Where(x => !@new.Any(y => y.Groups[4].Value.Equals(x.Groups[4].Value)))
        //                         .Select(x => $"{x.Groups[4].Value} removed.")
        //                         .ToArray();

        //    exceptions = differneces.Concat(removed).ToArray();

        //    return !exceptions.Any();
        //}

        public static List<string> CompareJson(JToken previous, JToken current, string path = "Root")
        {
            List<string> differences = new List<string>();

            if (previous == null && current == null)
                return differences;

            if (previous == null || current == null)
            {
                differences.Add($"{path} was changed from {previous ?? "null"} to {current ?? "null"}");
                return differences;
            }

            if (previous.Type != current.Type)
            {
                differences.Add($"{path} type changed from {previous.Type} to {current.Type}");
                return differences;
            }

            switch (previous.Type)
            {
                case JTokenType.Object:
                    JObject obj1 = (JObject)previous;
                    JObject obj2 = (JObject)current;
                    HashSet<string> allKeys = new HashSet<string>(obj1.Properties().Select(p => p.Name));
                    allKeys.UnionWith(obj2.Properties().Select(p => p.Name));

                    foreach (string key in allKeys)
                    {
                        JToken child1 = obj1.ContainsKey(key) ? obj1[key] : null;
                        JToken child2 = obj2.ContainsKey(key) ? obj2[key] : null;
                        differences.AddRange(CompareJson(child1, child2, $"{path} > {key}"));
                    }
                    break;

                case JTokenType.Array:
                    JArray arr1 = (JArray)previous;
                    JArray arr2 = (JArray)current;
                    int maxLength = Math.Max(arr1.Count, arr2.Count);

                    for (int i = 0; i < maxLength; i++)
                    {
                        JToken item1 = i < arr1.Count ? arr1[i] : null;
                        JToken item2 = i < arr2.Count ? arr2[i] : null;

                        string identifier = $"[{i}]"; // Default to index

                        // Try to use "name" property from either item
                        if (item1 is JObject itemObj1 && itemObj1["name"] != null)
                        {
                            identifier = itemObj1["name"].ToString();
                        }
                        else if (item2 is JObject itemObj2 && itemObj2["name"] != null)
                        {
                            identifier = itemObj2["name"].ToString();
                        }

                        differences.AddRange(CompareJson(item1, item2, $"{path} > {identifier}"));
                    }
                    break;

                default:
                    if (!JToken.DeepEquals(previous, current))
                    {
                        differences.Add($"{path} was changed from {previous} to {current}");
                    }
                    break;
            }

            return differences;
        }



        public static void JsonSchemaValidator(string payload, string schema, string schemaName, string responseCode)
        {
            JToken offersSchemaJtoken = JsonConvert.DeserializeObject<JToken>(schema);

            string schemaRef = offersSchemaJtoken.SelectToken($"$.paths.{schemaName}.post.responses.{responseCode}.content.application/json.schema.items.$ref")?.ToString();
            schemaRef ??= offersSchemaJtoken.SelectToken($"$.paths.{schemaName}.post.responses.{responseCode}.content.application/json.schema.$ref")?.ToString();

            if (schemaRef == null)
                throw new ArgumentException($"Could not find the schema name {schemaName} and / or  response code {responseCode} in the schema");

            string componentSchemaName = schemaRef.Split('/').Last();

            VAA.EAPI.Automation.Core.Helpers.OpenApiSchemaValidator validator = new OpenApiSchemaValidator(schema);

            validator.AssertValid(payload, componentSchemaName);
        }

        public static void AssertValid(this OpenApiSchemaValidator validator, string responseJson, string schemaName)
        {
            VAA.EAPI.Automation.Core.Helpers.ValidationResult result = validator.ValidateResponse(responseJson, schemaName);
            if (!result.IsValid)
            {
                string errorMessage = string.Join(Environment.NewLine, result.Errors);
                throw new ArgumentException($"Schema validation failed:{Environment.NewLine}{errorMessage}");
            }
        }

        public static string[] GetAllPropertiesFromJsonResponse(string jsonResponse)
        {
            List<string> properties = new List<string>();
            JToken jsonToken = JToken.Parse(jsonResponse);
            JObject jsonObject = null;

            if (jsonToken is JArray jsonArray)
                jsonObject = jsonArray.FirstOrDefault() as JObject;
            else if (jsonToken is JObject)
                jsonObject = jsonToken as JObject;

            if (jsonObject is null)
                return [];

            foreach (JProperty property in jsonObject.Descendants().OfType<JProperty>())
            {
                string path = property.Path;

                path = RemoveArrayIndicies(path);
                path = path.TrimStart('.');

                if (!properties.Contains(path))
                {
                    properties.Add(path);
                }
            }

            return properties.OrderBy(x => x).ToArray();
        }

        private static string RemoveArrayIndicies(string path)
        {
            return string.Join(".", path.Split('.').Select(part => part.Split('[')[0]));
        }
    }
}
