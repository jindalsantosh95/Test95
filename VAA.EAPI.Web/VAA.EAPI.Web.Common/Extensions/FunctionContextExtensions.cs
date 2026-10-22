using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Common.Extensions
{
    public static class FunctionContextExtensions
    {
        public static void SetHttpResponseData(this FunctionContext context, HttpStatusCode statusCode, string message = null)
        {
            var response = context.GetHttpRequestDataAsync().GetAwaiter().GetResult().CreateResponse();

            if (message != null)
                response.Body = new MemoryStream(Encoding.UTF8.GetBytes(message));

            response.StatusCode = statusCode;

            context.SetHttpResponseData(response);
        }

        public static void SetHttpResponseData(this FunctionContext context, HttpResponseData response)
        {
            var functionBindingsFeature = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
            var invocationResult = functionBindingsFeature.GetType().GetProperties().Single(p => p.Name == "InvocationResult");

            invocationResult.SetValue(functionBindingsFeature, response);
        }

        /// <summary>
        /// Set the Response Status code (for failed Authorizations)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        internal static async Task SetHttpResponseStatusCode(this FunctionContext context, HttpStatusCode statusCode)
        {
            var req = (await context.GetHttpRequestDataAsync());

            var response = req.CreateResponse();

            response.StatusCode = statusCode;

            await context.InvokeResult(response);
        }

        /// <summary>
        /// Get the Method [the actual object] of the currently executing Function
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static MethodInfo GetTargetFunctionMethod(this FunctionContext context)
        {
            var entryPoint = context.FunctionDefinition.EntryPoint;

            var assemblyPath = context.FunctionDefinition.PathToAssembly;
            var assembly = Assembly.LoadFrom(assemblyPath);
            var typeName = entryPoint.Substring(0, entryPoint.LastIndexOf('.'));
            var type = assembly.GetType(typeName);
            var methodName = entryPoint.Substring(entryPoint.LastIndexOf('.') + 1);
            var method = type.GetMethod(methodName);

            return method;
        }

        internal static async Task InvokeResult(this FunctionContext context, HttpResponseData response)
        {
            var log = context.GetLogger("FunctionContextAccessorMiddleware");
            string guid = AsyncContext.Current.CorrelationId.ToString();
            log.LogInformation($"FunctionContextAccessorMiddleware | ErrorID : {guid} | FunctionContextExtensions error");

            switch (response.StatusCode)
            {

                case HttpStatusCode.Forbidden:

                    response = await ResultExtention<object>.ExecuteResultAsync(await context.GetHttpRequestDataAsync(), Result.Success(new Response<object>()
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.Application.Forbidden), guid, Constants.Application.GlobalExceptionFilePath),
                        Status = Status.Forbidden,
                        IgnoreNullValues = false
                    }));
                    break;
                case HttpStatusCode.Unauthorized:
                    response = await ResultExtention<object>.ExecuteResultAsync(await context.GetHttpRequestDataAsync(), Result.Success(new Response<object>()
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.Application.Unauthorized), guid, Constants.Application.GlobalExceptionFilePath),
                        Status = Status.Unauthorized,
                        IgnoreNullValues = false
                    }));
                    break;
                case HttpStatusCode.InternalServerError:
                    response = await ResultExtention<object>.ExecuteResultAsync(await context.GetHttpRequestDataAsync(), Result.Success(new Response<object>()
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.Application.TechnicalError), guid, Constants.Application.GlobalExceptionFilePath),
                        Status = Status.Error,
                        IgnoreNullValues = false
                    }));
                    break;
            }


            var keyValuePair = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
            var functionBindingsFeature = keyValuePair.Value;
            var type = functionBindingsFeature.GetType();
            var result = type.GetProperties().Single(p => p.Name == "InvocationResult");
            result.SetValue(functionBindingsFeature, response);
        }

        public static T GetBindingData<T>(this FunctionContext source, string key)
        {
            key = source.BindingContext.BindingData.Keys.FirstOrDefault(x => x.Equals(key, System.StringComparison.OrdinalIgnoreCase));

            if (key.IsNullOrWhiteSpace()) return default;

            return (T)source.BindingContext.BindingData[key];
        }

        public static string GetOperationName(this FunctionContext source)
        {
            var operationName = source.GetBindingData<string>("operationName");

            if (operationName.IsNullOrWhiteSpace())
                operationName = source.GetBindingData<string>("pqId");

            if (operationName.IsNullOrWhiteSpace())
                operationName = RegexCompiled.Match((source.GetBindingData<string>("query") ?? string.Empty), "(?<=query[^{]*{)\\s*([^(]*)", RegexOptions.CultureInvariant | RegexOptions.Multiline)?.Groups[1].Value;

            return operationName ?? string.Empty;
        }

        public static T GetHeader<T>(this FunctionContext source, string name) => source.GetHeader(name).TryParse(out T result) ? result : default;
        public static string GetHeader(this FunctionContext source, string name)
        {
            var request = source.GetHttpRequestDataAsync().GetAwaiter().GetResult();

            if (!(request?.Headers?.Contains(name) ?? false)) return default;
            if (!request.Headers.TryGetValues(name, out IEnumerable<string> values)) return default;

            var value = string.Join(string.Empty, values);

            return value == string.Empty ? null : value;
        }
    }
}
