using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.LoungeManagement.Clues.Helper
{
    public class MediaTypeHelper
    {
        public static async Task<HttpResponseData> CreateResponseForMediaType<T>(HttpRequestData req, string mediaType, Response<T> result)
        {
            var response = req.CreateResponse();

            response.Headers.Remove("Content-Type");

            response.Headers.Add("Content-Type", "application/json");

            if (result.Value != null)
                await response.WriteStringAsync(Json.Serialize(result.Value));

            if (result.Status.Equals(Status.BadRequest))
                response.StatusCode = HttpStatusCode.BadRequest;
            if (result.Status.Equals(Status.Error))
                response.StatusCode = HttpStatusCode.InternalServerError;

            return response;
        }

        public static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.LoungeOcpKey) }
            };
        }
    }
}
