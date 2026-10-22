using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class JObjectExtensions
    {
        /// <summary>
        /// Resolves a JSONPath under <paramref name="source"/> and throws if either the
        /// path is missing OR the resolved token is a JSON <c>null</c>/undefined. Use
        /// this in tests when the path MUST carry a real value for the scenario to be
        /// meaningful — you get a <see cref="JsonException"/> naming the failing path
        /// instead of a <see cref="NullReferenceException"/> at the indexer that nulled
        /// out, or a silent null comparison further down the assertion chain. Pairs
        /// with the "Body assertions" pattern in docs/TEST_DATA_STRATEGY.md.
        /// </summary>
        /// <param name="source">The root token to search (JObject or any JToken).</param>
        /// <param name="path">JSONPath in Newtonsoft syntax — e.g. <c>"booking.product[0].reference.pnrLocator"</c>.</param>
        /// <returns>The token at <paramref name="path"/>. Never null.</returns>
        /// <exception cref="JsonException">
        ///   Thrown by <see cref="JToken.SelectToken(string, bool)"/> if no token matches
        ///   <paramref name="path"/>; thrown directly if the resolved token is a JSON
        ///   <c>null</c> or undefined value.
        /// </exception>
        public static JToken RequireToken(this JObject source, string path) => ((JToken)source).RequireToken(path);

        /// <inheritdoc cref="RequireToken(JObject, string)"/>
        public static JToken RequireToken(this JToken source, string path)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            // SelectToken(path, errorWhenNoMatch: true) throws JsonException with the
            // path when no token matches — null-coalescing afterwards would be dead
            // code. We do still need to guard against the token being a JSON null /
            // undefined value, which SelectToken returns as-is rather than treating
            // as a no-match.
            JToken token = source.SelectToken(path, errorWhenNoMatch: true)!;
            if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            {
                throw new JsonException(
                    $"JSON path '{path}' resolved to a {token.Type} value (expected a non-null token).");
            }
            return token;
        }

        public static void ApplyMapping(this JObject source, string filePath)
        {
            if (!File.Exists(filePath))
                return;

            using (DataTable dataTable = new DataTable("Mappings"))
            {
                dataTable.FromDelimitedFile(filePath, ",", false, true);
                source.ApplyMapping(dataTable);
            }
        }

        public static void ApplyMapping(this JObject source, DataTable dataTable)
        {
            if (dataTable == null)
                return;

            foreach (DataRow row in dataTable.Rows)
            {
                string path = row["path"].ToString();
                string value = row["value"].ToString();

                if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
                    value = null;

                if (!path.Has("["))
                    source.SelectToken(path)?.Replace(value);
                else
                {
                    JToken array = null;
                    JToken element = source.SelectToken(path);

                    if (element != null)
                        element.Replace(value);
                    else
                    {
                        JToken root = source;

                        // parse the JSONPath to make sure any target arrays & elements exists, if not we create on the fly

                        while (path.Has("]"))
                        {
                            string elementPath = path.Substring(0, path.IndexOf("]") + 1);

                            // if the target element exists go straight to it and save time
                            element = root.SelectToken(elementPath);

                            if (element != null)
                                root = element;
                            else
                            {
                                string[] arrayPath = elementPath.Split("[");
                                string parentArrayPath = arrayPath[0];
                                int targetIndex = int.Parse(arrayPath[1].Replace("]", string.Empty));

                                array = root.SelectToken(parentArrayPath);

                                // if the target array doesn't exist, create it
                                if (array == null)
                                {
                                    string[] segments = parentArrayPath.Split('.');

                                    JToken @ref = root;

                                    for (int i = 0; i < segments.Length - 1; i++)
                                    {
                                        JToken property = @ref.SelectToken(segments[i]);

                                        if (property == null)
                                            ((JObject)@ref).Add(segments[i], JToken.Parse("{}"));

                                        @ref = @ref.SelectToken(segments[i]);
                                    }

                                    ((JObject)@ref).Add(segments.Last(), JToken.Parse("[]"));
                                }

                                JArray arrayParent = (JArray)root.SelectToken(parentArrayPath);

                                if (targetIndex < arrayParent.Count)
                                {
                                    // existing array element

                                    root = arrayParent[targetIndex];
                                }
                                else if (targetIndex >= arrayParent.Count)
                                {
                                    // new element on an existing array

                                    while (targetIndex >= arrayParent.Count)
                                        arrayParent.Add(JToken.Parse("{}"));

                                    root = arrayParent[targetIndex];
                                }
                            }

                            path = path.Substring(path.IndexOf("]") + 1).Trim('.');
                        }

                        if (root != null)
                        {
                            JToken property = root.SelectToken(path);

                            if (property == null)
                                ((JObject)root).Add(path, value);
                            else
                                property.Replace(value);
                        }
                    }
                }
            }
        }
    }
}
