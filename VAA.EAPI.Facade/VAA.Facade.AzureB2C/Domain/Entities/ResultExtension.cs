using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Threading.Tasks;
using VAA.Facade.AzureB2C.Domain.Model;
using VAA.Facade.AzureB2C.Shared;


namespace VAA.Facade.AzureB2C.Domain.Entities
{
    public class ResultExtention<T>
    {
        public static async Task<HttpResponseData> ExecuteResultAsync(HttpRequestData request, Result<Response<T>> result)
        {
            var response = request.CreateResponse();
            response.Headers.Add(Constants.ContentType, Constants.MediaType);
            if (result.IsFailure)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteAsJsonAsync(result.Error);
            }
            else
                {
                if(result.Value != null)
                {
                    await response.WriteAsJsonAsync(result.Value.Value);
                   
                    if(result.Value.Status == Status.OK || result.Value.Status == Status.Accepted || result.Value.Status == Status.Error)
                        response.StatusCode = HttpStatusCode.OK;
                    else if(result.Value.Status == Status.BusinessException)
                        response.StatusCode = HttpStatusCode.BadRequest;                    
                }
                else
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;                    
                }
                
            }
            return response;
        }
    }
}
