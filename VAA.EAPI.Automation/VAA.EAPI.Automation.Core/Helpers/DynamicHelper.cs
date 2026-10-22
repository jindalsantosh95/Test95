using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public static class DynamicHelper
    {
        /// <summary>
        /// Recursively checks if a dynamic object contains a specific property.
        /// </summary>
        /// <param name="obj">The dynamic object to check, which will be converted to a <see cref="JObject"/>.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <returns><c>true</c> if the property exists at any level within the object; otherwise, <c>false</c>.</returns>
        public static bool HasProperty(dynamic obj, string propertyName)
        {
            dynamic jObject = JObject.FromObject(obj);

            return HasProperty(jObject, propertyName);
        }

        public static bool HasProperty(JObject jObject, string propertyName)
        {
            return jObject.SelectTokens($"..{propertyName}").Any();
        }

        public static bool HasPropertyWithValue(dynamic obj, string propertyName, string textToFind)
        {
            dynamic jObject = JObject.FromObject(obj);

            return HasPropertyWithValue(jObject, propertyName, textToFind);
        }

        public static bool HasPropertyWithValue(JObject jObject, string propertyName, string textToFind)
        {
            // Select all matching property tokens in the object tree
            IEnumerable<JToken> tokens = jObject.SelectTokens($"..{propertyName}");

            // Return true if any property value contains the text
            return tokens.Any(token =>
                token.Type == JTokenType.String &&
                token.Value<string>()?.Contains(textToFind, StringComparison.OrdinalIgnoreCase) == true
            );
        }

    }
}
