using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        public void AddError(string path, string message)
        {
            Errors.Add($"{path}: {message}");
        }

        public void AddWarning(string path, string message)
        {
            Warnings.Add($"{path}: {message}");
        }
    }

    public class OpenApiSchemaValidator
    {
        private readonly OpenApiSchemaResolver _resolver;

        public OpenApiSchemaValidator(string openApiJson)
        {
            _resolver = new OpenApiSchemaResolver(openApiJson);
        }

        /// <summary>
        /// Validates a JSON response against a schema
        /// </summary>
        /// <param name="responseJson">The JSON response to validate</param>
        /// <param name="schemaName">Name of the schema to validate against</param>
        /// <returns>ValidationResult with details about validation success/failure</returns>
        public ValidationResult ValidateResponse(string responseJson, string schemaName)
        {
            try
            {
                JToken responseData = JToken.Parse(responseJson);

                if (responseData.Type == JTokenType.Array)
                    responseData = responseData.First;

                JObject schema = _resolver.ResolveSchema(schemaName);

                VAA.EAPI.Automation.Core.Helpers.ValidationResult result = new ValidationResult { IsValid = true };
                ValidateToken(responseData, schema, "$", result);

                result.IsValid = result.Errors.Count == 0;
                return result;
            }
            catch (JsonException ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = { $"Invalid JSON: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = { $"Validation error: {ex.Message}" }
                };
            }
        }

        /// <summary>
        /// Validates a JToken against a schema object
        /// </summary>
        private void ValidateToken(JToken data, JObject schema, string path, ValidationResult result)
        {
            // Get schema properties
            string schemaType = schema["type"]?.ToString();
            JObject schemaProperties = schema["properties"] as JObject;
            JArray requiredProperties = schema["required"] as JArray;
            JObject schemaItems = schema["items"] as JObject;
            JArray schemaEnum = schema["enum"] as JArray;
            string schemaFormat = schema["format"]?.ToString();
            string schemaPattern = schema["pattern"]?.ToString();
            int? schemaMinLength = schema["minLength"]?.ToObject<int?>();
            int? schemaMaxLength = schema["maxLength"]?.ToObject<int?>();
            double? schemaMinimum = schema["minimum"]?.ToObject<double?>();
            double? schemaMaximum = schema["maximum"]?.ToObject<double?>();

            // Handle nullable values
            bool nullable = schema["nullable"]?.ToObject<bool>() ?? false;
            if (data.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddError(path, "Value cannot be null");
                }
                return;
            }

            // Validate type
            if (!string.IsNullOrEmpty(schemaType))
            {
                ValidateType(data, schemaType, path, result);
            }

            // Validate enum values
            if (schemaEnum != null && schemaEnum.Count > 0)
            {
                ValidateEnum(data, schemaEnum, path, result);
            }

            // Validate format
            if (!string.IsNullOrEmpty(schemaFormat))
            {
                ValidateFormat(data, schemaFormat, path, result);
            }

            // Validate pattern (for strings)
            if (!string.IsNullOrEmpty(schemaPattern) && data.Type == JTokenType.String)
            {
                ValidatePattern(data.ToString(), schemaPattern, path, result);
            }

            // Validate string length
            if (data.Type == JTokenType.String)
            {
                ValidateStringLength(data.ToString(), schemaMinLength, schemaMaxLength, path, result);
            }

            // Validate numeric ranges
            if (data.Type == JTokenType.Integer || data.Type == JTokenType.Float)
            {
                ValidateNumericRange(data.ToObject<double>(), schemaMinimum, schemaMaximum, path, result);
            }

            // Validate object properties
            if (data.Type == JTokenType.Object && schemaProperties != null)
            {
                ValidateObject(data as JObject, schemaProperties, requiredProperties, path, result);
            }

            // Validate array items
            if (data.Type == JTokenType.Array && schemaItems != null)
            {
                ValidateArray(data as JArray, schemaItems, path, result);
            }
        }

        private void ValidateType(JToken data, string expectedType, string path, ValidationResult result)
        {
            string actualType = GetJsonSchemaType(data.Type);

            // Handle integer vs number type flexibility
            if (expectedType == "number" && actualType == "integer")
            {
                return; // integers are valid numbers
            }

            if (expectedType == "dateOnly" && RegexCompiled.IsMatch(data.ToString(), @"\d{4}-\d{2}-\d{2}"))
            {
                return;
            }

            if (expectedType == "timeOnly" && RegexCompiled.IsMatch(data.ToString(), @"\d{2}:\d{2}:\d{2}"))
            {
                return;
            }

            if (actualType != expectedType)
            {
                result.AddError(path, $"Expected type '{expectedType}' but got '{actualType}'");
            }
        }

        private void ValidateEnum(JToken data, JArray enumValues, string path, ValidationResult result)
        {
            string dataValue = data.ToString();
            List<string> validValues = enumValues.Select(v => v.ToString()).ToList();

            if (!validValues.Contains(dataValue))
            {
                result.AddError(path, $"Value '{dataValue}' is not one of the allowed values: [{string.Join(", ", validValues)}]");
            }
        }

        private void ValidateFormat(JToken data, string format, string path, ValidationResult result)
        {
            string value = data.ToString();

            switch (format.ToLower())
            {
                case "date-time":
                    if (!DateTime.TryParse(value, out _))
                    {
                        result.AddError(path, $"Invalid date-time format: '{value}'");
                    }
                    break;

                case "date":
                    if (!DateTime.TryParseExact(value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
                    {
                        result.AddError(path, $"Invalid date format (expected yyyy-MM-dd): '{value}'");
                    }
                    break;

                case "email":
                    if (!IsValidEmail(value))
                    {
                        result.AddError(path, $"Invalid email format: '{value}'");
                    }
                    break;

                case "uri":
                    if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    {
                        result.AddError(path, $"Invalid URI format: '{value}'");
                    }
                    break;
            }
        }

        private void ValidatePattern(string value, string pattern, string path, ValidationResult result)
        {
            try
            {
                if (!Regex.IsMatch(value, pattern))
                {
                    result.AddError(path, $"Value '{value}' does not match required pattern '{pattern}'");
                }
            }
            catch (ArgumentException)
            {
                result.AddWarning(path, $"Invalid regex pattern in schema: '{pattern}'");
            }
        }

        private void ValidateStringLength(string value, int? minLength, int? maxLength, string path, ValidationResult result)
        {
            if (minLength.HasValue && value.Length < minLength.Value)
            {
                result.AddError(path, $"String length {value.Length} is less than minimum {minLength.Value}");
            }

            if (maxLength.HasValue && value.Length > maxLength.Value)
            {
                result.AddError(path, $"String length {value.Length} exceeds maximum {maxLength.Value}");
            }
        }

        private void ValidateNumericRange(double value, double? minimum, double? maximum, string path, ValidationResult result)
        {
            if (minimum.HasValue && value < minimum.Value)
            {
                result.AddError(path, $"Value {value} is less than minimum {minimum.Value}");
            }

            if (maximum.HasValue && value > maximum.Value)
            {
                result.AddError(path, $"Value {value} exceeds maximum {maximum.Value}");
            }
        }

        private void ValidateObject(JObject data, JObject schemaProperties, JArray requiredProperties, string path, ValidationResult result)
        {
            // Check required properties
            if (requiredProperties != null)
            {
                foreach (JToken requiredProp in requiredProperties)
                {
                    string propName = requiredProp.ToString();
                    if (data[propName] == null)
                    {
                        result.AddError($"{path}.{propName}", "Required property is missing");
                    }
                }
            }

            // Validate each property in the data
            foreach (JProperty property in data.Properties())
            {
                JObject propSchema = schemaProperties[property.Name] as JObject;
                if (propSchema != null)
                {
                    ValidateToken(property.Value, propSchema, $"{path}.{property.Name}", result);
                }
                else
                {
                    result.AddError($"{path}.{property.Name}", "Property not defined in schema");
                }
            }
        }

        private void ValidateArray(JArray data, JObject itemSchema, string path, ValidationResult result)
        {
            for (int i = 0; i < data.Count; i++)
            {
                ValidateToken(data[i], itemSchema, $"{path}[{i}]", result);
            }
        }

        private string GetJsonSchemaType(JTokenType tokenType)
        {
            return tokenType switch
            {
                JTokenType.String or JTokenType.Date => "string",
                JTokenType.Integer => "integer",
                JTokenType.Float => "number",
                JTokenType.Boolean => "boolean",
                JTokenType.Array => "array",
                JTokenType.Object => "object",
                JTokenType.Null => "null",
                _ => "unknown"
            };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                System.Net.Mail.MailAddress addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
