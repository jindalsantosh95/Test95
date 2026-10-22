using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text;

namespace VAA.EAPI.GraphQL.Stitch.Core.Extensions
{
    public static class FunctionContextExtensions
    {
        private const string DefaultHeaderValueDelimiter = ",";
        private const string XForwardedForHeader = "X-Forwarded-For";

        public static void SetHttpResponseData(this FunctionContext source, HttpStatusCode statusCode, string message = null)
        {
            var response = source.GetHttpRequestDataAsync().GetAwaiter().GetResult().CreateResponse();

            if (message != null)
            {
                response.Headers.Add("Content-Type", "application/json");
                response.Body = new MemoryStream(Encoding.UTF8.GetBytes(message));
            }

            response.StatusCode = statusCode;

            source.SetHttpResponseData(response);
        }
        public static void SetHttpResponseData(this FunctionContext source, HttpResponseData response)
        {
            var functionBindingsFeature = source.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
            var invocationResult = functionBindingsFeature.GetType().GetProperties().Single(p => p.Name == "InvocationResult");

            invocationResult.SetValue(functionBindingsFeature, response);
        }

        public static T GetBindingData<T>(this FunctionContext source, string key)
        {
            key = source.BindingContext.BindingData.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (key.IsNullOrWhiteSpace()) return default;

            return (T)source.BindingContext.BindingData[key];
        }

        public static T GetHeader<T>(this FunctionContext source, string name) => source.GetHeader(name).TryParse(out T result) ? result : default;
        public static string GetHeader(this FunctionContext source, string name)
        {
            var request = source.GetHttpRequestDataAsync().GetAwaiter().GetResult();

            if (!(request?.Headers?.Contains(name) ?? false)) return default;
            if (!request.Headers.TryGetValues(name, out IEnumerable<string> values)) return default;

            var value = string.Join(DefaultHeaderValueDelimiter, values);

            return value == string.Empty ? null : value;
        }

        public static string GetForwardedForIPAddress(this FunctionContext source) => source.GetHeader(XForwardedForHeader)?
                                                                                            .Split(DefaultHeaderValueDelimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                                                                            .FirstOrDefault() ?? string.Empty;
    }
}
