using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.Facade.PDMFacade.Helper
{
    public class MediaTypeHelper
    {
        public static async Task<HttpResponseData> CreateResponseForMediaType<T>(HttpRequestData req, string mediaType, Response<T> result)
        {
            var response = req.CreateResponse();

            response.Headers.Remove("Content-Type");

            response.Headers.Add("Content-Type", mediaType.Equals("xml") ? "application/xml" : "application/json");

            if (result.Value != null)
                await response.WriteStringAsync(mediaType.Equals("xml") ? Xml.Serialize(result.Value) : Json.Serialize(result.Value));

            if (result.Status.Equals(Status.BadRequest))
                response.StatusCode = HttpStatusCode.BadRequest;
            if (result.Status.Equals(Status.Error))
                response.StatusCode = HttpStatusCode.InternalServerError;
            return response;
        }

        public static string SetMediaTypeForRequest(HttpRequestData req)
        {
            var mediaType = "json";

            if (req != null && req.Headers != null && req.Headers.Contains("Content-Type"))
            {
                var contentType = req.Headers.FirstOrDefault(h => h.Key.Equals("Content-Type"));

                if (contentType.Value.FirstOrDefault().ToLower().Contains("xml"))
                {
                    mediaType = "xml";
                }
            }

            return mediaType;
        }

        public static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.PdmOcpKey) }
            };
        }
    }
}
