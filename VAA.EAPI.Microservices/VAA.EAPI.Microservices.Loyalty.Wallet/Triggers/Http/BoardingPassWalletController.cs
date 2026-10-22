using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Triggers.Http
{
    public class BoardingPassWalletController(IBoardingPassWalletService boardingPassService)
    {
        private readonly IBoardingPassWalletService _boardingPassService = boardingPassService ?? throw new System.ArgumentNullException(nameof(boardingPassService));

        [OpenApiOperation(operationId: "WalletLinkGeneration", tags: ["Wallet Microservice"], Summary = "Generate wallet links for a boarding pass for iOS and Android", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(WalletPassRequest), Description = "Details to generate wallet links")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(WalletResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GenerateWalletLinks")]
        public async Task<HttpResponseData> GenerateWalletLinks([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "BoardingPass")] HttpRequestData req)
        {
            var result = await _boardingPassService.GenerateWalletLinksAsync(Json.Deserialize<WalletPassRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<WalletResponse>.ExecuteResultAsync(req, result);
        }
    }
}
