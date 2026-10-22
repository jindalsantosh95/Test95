using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Helper;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Extensions
{
    internal static class FunctionContextExtenstion
    {
        internal static HttpRequestData GetHttpRequestData(this FunctionContext context)
        {
            var keyValuePair = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
            var functionBindingsFeature = keyValuePair.Value;
            var type = functionBindingsFeature.GetType();
            var inputData = type.GetProperties().Single(p => p.Name == "InputData").GetValue(functionBindingsFeature) as IReadOnlyDictionary<string, object>;
            return inputData?.Values.SingleOrDefault(o => o is HttpRequestData) as HttpRequestData;
        }

        internal static async Task InvokeResult(this FunctionContext context, HttpResponseData response)
        {
            var errorResponse = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.Constants.Unauthorized), Guid.NewGuid().ToString(), Constants.Constants.GlobalExceptionFilePath);
            var unthorizedResponse = Result.Success(new Response<object>
            {
                Errors = errorResponse,
                Status = Status.Unauthorized
            });

            response = await ResultExtention<object>.ExecuteResultAsync(context.GetHttpRequestData(), unthorizedResponse);
            var keyValuePair = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
            var functionBindingsFeature = keyValuePair.Value;
            var type = functionBindingsFeature.GetType();
            var result = type.GetProperties().Single(p => p.Name == "InvocationResult");
            result.SetValue(functionBindingsFeature, response);
        }
    }
}
