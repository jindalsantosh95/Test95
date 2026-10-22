using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;

namespace VAA.EAPI.Microservices.Retail.Holidays.Triggers.Http
{
    public sealed class ClaimsController
    {
        private const string TbxUsernameConfig = "Holidays:TbxUsername";
        private const string TbxUserIdConfig = "Holidays:TbxUserId";

        [OpenApiOperation(operationId: "authorisationTokens", tags: ["authorisationTokens"], Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "OK", Description = "OK")]
        [Function("AuthorisationTokens")]
        public async Task<HttpResponseData> AuthorisationTokens([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "authorisationTokens")] HttpRequestData req)
        {
            return await ResultExtention<JObject>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<JObject>()
            {
                Status = Status.OK,
                Value = JObject.Parse(@$"{{
                                ""data"": 
                                {{
                                    ""@odata.type"": ""microsoft.graph.onTokenIssuanceStartResponseData"",
                                    ""actions"": 
                                    [
                                        {{
                                            ""@odata.type"": ""microsoft.graph.tokenIssuanceStart.provideClaimsForToken"",
                                            ""claims"": 
                                            {{
                                                ""tbx-username"": ""{ConfigReader.Get(TbxUsernameConfig)}"",
                                                ""tbx-userid"": ""{ConfigReader.Get(TbxUserIdConfig)}""
                                            }}
                                        }}
                                    ]
                                }}
                            }}"),
            }));
        }
    }
}