using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class FunctionContextExtensions
    {
        public static void SetHttpResponseData(this FunctionContext source, HttpStatusCode statusCode, string message = null)
        {
            HttpResponseData response = source.GetHttpRequestDataAsync().GetAwaiter().GetResult().CreateResponse();

            if (message != null)
                response.Body = new MemoryStream(Encoding.UTF8.GetBytes(message));

            response.StatusCode = statusCode;

            source.SetHttpResponseData(response);
        }

        public static void SetHttpResponseData(this FunctionContext source, HttpResponseData response)
        {
            object functionBindingsFeature = source.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
            PropertyInfo invocationResult = functionBindingsFeature.GetType().GetProperties().Single(p => p.Name == "InvocationResult");

            invocationResult.SetValue(functionBindingsFeature, response);
        }

        public static void SetHttpRequestData(this FunctionContext source, HttpRequestData request)
        {
            object functionBindingsFeature = source.Features.Single(f => f.Key.Name == "IFunctionBindingsFeature").Value;
            PropertyInfo invocationResult = functionBindingsFeature.GetType().GetProperties().Single(p => p.Name == "InputData");
            System.Collections.ObjectModel.ReadOnlyDictionary<string, object> inputData = (System.Collections.ObjectModel.ReadOnlyDictionary<string, object>)invocationResult.GetValue(functionBindingsFeature);
            KeyValuePair<string, object> req = inputData.Single(x => x.Key.Is("req"));
            FieldInfo fieldInfo = req.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name.Is("value"));

            fieldInfo?.SetValue(req, request);
        }

        public static void SetHttpResponseStatusCode(this FunctionContext source, HttpStatusCode statusCode)
        {
            HttpResponseData response = source.GetHttpRequestDataAsync().GetAwaiter().GetResult().CreateResponse();

            response.StatusCode = statusCode;

            source.SetHttpResponseData(response);
        }

        public static MethodInfo GetTargetFunctionMethod(this FunctionContext source)
        {
            string entryPoint = source.FunctionDefinition.EntryPoint;

            string assemblyPath = source.FunctionDefinition.PathToAssembly;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            string typeName = entryPoint.Substring(0, entryPoint.LastIndexOf('.'));
            Type type = assembly.GetType(typeName);
            string methodName = entryPoint.Substring(entryPoint.LastIndexOf('.') + 1);
            MethodInfo method = type.GetMethod(methodName);

            return method;
        }

        public static T GetBindingData<T>(this FunctionContext source, string key)
        {
            key = source.BindingContext.BindingData.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (key.IsNullOrWhiteSpace())
                return default;

            return (T)source.BindingContext.BindingData[key];
        }

        public static string GetOperationName(this FunctionContext source)
        {
            string operationName = source.GetBindingData<string>("operationName");

            if (operationName.IsNullOrWhiteSpace())
                operationName = source.GetBindingData<string>("pqId");

            if (operationName.IsNullOrWhiteSpace())
                operationName = RegexCompiled.Match(source.GetBindingData<string>("query") ?? string.Empty, "(?<=query[^{]*{)\\s*([^(]*)", RegexOptions.CultureInvariant | RegexOptions.Multiline)?.Groups[1].Value;

            return operationName ?? string.Empty;
        }

        public static T GetHeader<T>(this FunctionContext source, string name) => source.GetHeader(name).TryParse(out T result) ? result : default;

        public static string GetHeader(this FunctionContext source, string name)
        {
            HttpRequestData request = source.GetHttpRequestDataAsync().GetAwaiter().GetResult();

            if (!(request?.Headers?.Contains(name) ?? false))
                return default;
            if (!request.Headers.TryGetValues(name, out IEnumerable<string> values))
                return default;

            string value = string.Join(string.Empty, values);

            return value == string.Empty ? null : value;
        }
    }
}
