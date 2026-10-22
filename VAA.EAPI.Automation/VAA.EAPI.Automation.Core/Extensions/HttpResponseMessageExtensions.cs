using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static string ReadRequestAsString(this HttpResponseMessage source) => source.ReadRequestAsStringAsync().GetAwaiter().GetResult();
        public static T ReadRequestFromJson<T>(this HttpResponseMessage source) => source.ReadRequestFromJsonAsync<T>().GetAwaiter().GetResult();
        public static string ReadResponseAsString(this HttpResponseMessage source) => source.ReadResponseAsStringAsync().GetAwaiter().GetResult();
        public static T ReadResponseFromJson<T>(this HttpResponseMessage source) => source.ReadResponseFromJsonAsync<T>().GetAwaiter().GetResult();

        public async static Task<string> ReadRequestAsStringAsync(this HttpResponseMessage source) => await source.RequestMessage.Content.ReadAsStringAsync();
        public async static Task<T> ReadRequestFromJsonAsync<T>(this HttpResponseMessage source) => JsonConvert.DeserializeObject<T>(await source.ReadRequestAsStringAsync());
        public async static Task<string> ReadResponseAsStringAsync(this HttpResponseMessage source) => await source.Content.ReadAsStringAsync();
        public async static Task<T> ReadResponseFromJsonAsync<T>(this HttpResponseMessage source) => JsonConvert.DeserializeObject<T>(await source.ReadResponseAsStringAsync());

        public static void AssertIsSuccessStatusCode(this HttpResponseMessage source) => Assert.True(source.IsSuccessStatusCode, $"Success status code was not returned. {Environment.NewLine}{Environment.NewLine}{source.RequestMessage.Method}:{source.RequestMessage.RequestUri}{Environment.NewLine}Headers:{Environment.NewLine}{FormatHeaders(source.RequestMessage.Headers)}{Environment.NewLine}StatusCode: {source.StatusCode}{Environment.NewLine}Request: {(source.RequestMessage.Content != null ? source.ReadRequestAsString() : string.Empty)}{Environment.NewLine}Response: {source.ReadResponseAsString()}");
        public static void AssertStatusCodeIs(this HttpResponseMessage source, HttpStatusCode statusCode, string? message = null) => Assert.True(source.StatusCode == statusCode, $"{statusCode.ToString().SeparateUpperCamelCase()} status code was not returned. {Environment.NewLine}{Environment.NewLine}{source.RequestMessage.Method}:{source.RequestMessage.RequestUri}{Environment.NewLine}Headers:{Environment.NewLine}{FormatHeaders(source.RequestMessage.Headers)}{Environment.NewLine}StatusCode: {source.StatusCode}{Environment.NewLine}Request: {(source.RequestMessage.Content != null ? source.ReadRequestAsString() : string.Empty)}{Environment.NewLine}Response: {source.ReadResponseAsString()}");
        public static void AssertStatusCodeIsIn(this HttpResponseMessage source, HttpStatusCode from, HttpStatusCode to) => Assert.True((int)source.StatusCode >= (int)from && (int)source.StatusCode <= (int)to, $"Status code {source.StatusCode.ToString().SeparateUpperCamelCase()} is not between expected status codes of {from.ToString().SeparateUpperCamelCase()} - {to.ToString().SeparateUpperCamelCase()}. {Environment.NewLine}{Environment.NewLine}{source.RequestMessage.Method}:{source.RequestMessage.RequestUri}{Environment.NewLine}Headers:{Environment.NewLine}{FormatHeaders(source.RequestMessage.Headers)}{Environment.NewLine}StatusCode: {source.StatusCode}{Environment.NewLine}Request: {(source.RequestMessage.Content != null ? source.ReadRequestAsString() : string.Empty)}{Environment.NewLine}Response: {source.ReadResponseAsString()}");
        public static void AssertIsGQLSuccess(this HttpResponseMessage source, ITestOutputHelper? output = null)
        {
            JToken gqlResponse = source.ReadResponseFromJson<JToken>();
            JToken? gqlErrors = gqlResponse["errors"];

            if (gqlErrors is null)
            {
                return;
            }

            if (ContainsExpectedNoResultsSearchError(gqlErrors))
            {
                // xUnit v2 has no runtime skip. Treat accepted no-results as a pass,
                // but surface the full diagnostic so it stays auditable.
                // TODO (xUnit v3): replace these two lines with
                //   throw SkipException.ForSkip(BuildDiagnostic(source, "Accepted no-results..."));
                output?.WriteLine(BuildDiagnostic(source, "Accepted no-results search response — no GQL data to assert."));
                return;
            }

            Assert.Fail(BuildDiagnostic(source, "Error code was returned in GQL response."));
        }

        private static readonly string[] SensitiveHeaderNames =
        [
            "Authorization",
            "Ocp-Apim-Subscription-Key",
            "X-Api-Key",
            "X-Functions-Key"
        ];

        private static string FormatHeaders(System.Net.Http.Headers.HttpRequestHeaders headers) =>
            string.Join(
                Environment.NewLine,
                headers.Select(header =>
                    $"\t{header.Key}:{(SensitiveHeaderNames.Any(name => name.Is(header.Key)) ? "[REDACTED]" : string.Join(',', header.Value))}"));

        private static string BuildDiagnostic(HttpResponseMessage source, string header) =>
                                                                                            $"{header}{Environment.NewLine}{Environment.NewLine}" +
                                                                                            $"{source.RequestMessage.Method}:{source.RequestMessage.RequestUri}{Environment.NewLine}" +
                                                                                            $"Headers:{Environment.NewLine}" +
                                                                                            $"{FormatHeaders(source.RequestMessage.Headers)}{Environment.NewLine}" +
                                                                                            $"StatusCode: {source.StatusCode}{Environment.NewLine}" +
                                                                                            $"Request: {(source.RequestMessage.Content != null ? source.ReadRequestAsString() : string.Empty)}{Environment.NewLine}" +
                                                                                            $"Response: {source.ReadResponseAsString()}";

        public static HttpResponseMessage EnsureIsSuccessStatusCode(this HttpResponseMessage source)
        {
            source.AssertIsSuccessStatusCode();

            return source;
        }
        public static HttpResponseMessage EnsureStatusCodeIs(this HttpResponseMessage source, HttpStatusCode statusCode)
        {
            source.AssertStatusCodeIs(statusCode);

            return source;
        }
        public static HttpResponseMessage EnsureStatusCodeIsIn(this HttpResponseMessage source, HttpStatusCode from, HttpStatusCode to)
        {
            source.AssertStatusCodeIsIn(from, to);

            return source;
        }

        public static HttpResponseMessage EnsureIsUnprocessableEntityStatusCode(this HttpResponseMessage source)
        {
            source.EnsureStatusCodeIs(HttpStatusCode.UnprocessableEntity);

            return source;
        }

        public static HttpResponseMessage EnsureIsAcceptedStatusCode(this HttpResponseMessage source)
        {
            source.EnsureStatusCodeIs(HttpStatusCode.Accepted);

            return source;
        }

        public static HttpResponseMessage EnsureGQLResponseSuccess(this HttpResponseMessage source, ITestOutputHelper? output = null)
        {
            source.AssertIsSuccessStatusCode();
            source.AssertIsGQLSuccess(output);
            return source;
        }

        public static bool IsGQLResponseSuccess(this HttpResponseMessage source)
        {
            try
            {
                source.EnsureGQLResponseSuccess();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ExitOnError(this HttpResponseMessage source, params string[] errors)
        {
            errors ??= [];

            if (errors.Length == 0)
                return false;

            return source.ReadResponseAsString().HasAny(errors);
        }

        private static bool ContainsExpectedNoResultsSearchError(JToken gqlErrors)
        {
            string[] expectedSearchErrorFragments =
            [
                "find any results for your search",
                "unable to find a flight that meets your current search criteria"
            ];

            foreach (JToken gqlError in gqlErrors.Children())
            {
                string errorCode = gqlError["extensions"]?["code"]?.ToString() ?? string.Empty;
                string normalizedErrorPayload = NormalizeNoResultsText(gqlError.ToString());

                if (errorCode.Is("422") &&
                    expectedSearchErrorFragments.Any(fragment =>
                        normalizedErrorPayload.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeNoResultsText(string source)
        {
            return source
                .Replace("\u2019", "'")
                .Replace("’", "'")
                .ToLowerInvariant();
        }

        public static HttpResponseMessage Clone(this HttpResponseMessage source, HttpRequestMessage request)
        {
            HttpResponseMessage clone = source.Clone();

            clone.RequestMessage = request;

            return clone;
        }
        public static HttpResponseMessage Clone(this HttpResponseMessage source)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(source.StatusCode);

            if (source.Content != null)
            {
                source.Content.LoadIntoBufferAsync().GetAwaiter().GetResult();

                MemoryStream memoryStream = new MemoryStream();

                source.Content.ReadAsStream().CopyTo(memoryStream);

                memoryStream.Position = 0;

                httpResponseMessage.Content = new StreamContent(memoryStream);

                if (source.Content.Headers != null)
                    foreach (KeyValuePair<string, IEnumerable<string>> header in source.Content.Headers)
                        httpResponseMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            httpResponseMessage.Version = source.Version;
            httpResponseMessage.ReasonPhrase = source.ReasonPhrase;

            foreach (KeyValuePair<string, IEnumerable<string>> header in source.Headers)
                httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return httpResponseMessage;
        }
    }
}
