using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Common;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand
{
    public class GenerateSynchronyTokenServiceCommandHandler : IRequestHandler<GenerateSynchronyTokenServiceCommand, Result<Response<SynchronyTokenResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ISynchronyAccessTokenGenerator _synchronyToken;
        private readonly ISynchronyPayloadCryptoService _payloadCryptoService;
        private readonly ILogger<GenerateSynchronyTokenServiceCommandHandler> _logger;

        public GenerateSynchronyTokenServiceCommandHandler(IHttpConnector httpConnector, ISynchronyAccessTokenGenerator synchronyAccessToken, ISynchronyPayloadCryptoService payloadCryptoService, ILogger<GenerateSynchronyTokenServiceCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _synchronyToken = synchronyAccessToken ?? throw new ArgumentNullException(nameof(synchronyAccessToken));
            _payloadCryptoService = payloadCryptoService ?? throw new ArgumentNullException(nameof(payloadCryptoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<SynchronyTokenResponse>>> Handle(GenerateSynchronyTokenServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var headers = new Dictionary<string, string>
                    {
                         {"X-SYF-Request-TrackingId", AsyncContext.Current.CorrelationId.ToString()}
                    };

                var synchronyGenerateTokenRequest = (SynchronyGenerateTokenRequest)request;

                var token = await _synchronyToken.GetToken();

                var genTokenUrl = string.Concat(ConfigReader.Get(Constants.Environment.SynchronyDataShareAPIUrl).TrimEnd('/'), Constants.Application.SynchronyTokenGenerateUrl);
                var synchronyGenerateTokenResponse = await _httpConnector.SendAsync<SynchronyGenerateTokenResponse>(genTokenUrl, headers, token, verb: HttpMethod.Post, content: Json.Serialize((SynchronyPayload)_payloadCryptoService.EncryptData(Json.Serialize(synchronyGenerateTokenRequest))));

                if (synchronyGenerateTokenResponse != null)
                {
                    return Result.Success(new Response<SynchronyTokenResponse>
                    {
                        Status = Status.OK,
                        Value = new SynchronyTokenResponse { SynchronyToken = synchronyGenerateTokenResponse.Token },
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} Error in GenerateSynchronyTokenServiceCommandHandler with Request: {ex.Data["Request"]?.ToString()} and Response: {ex.Data["Response"]?.ToString()}");
                return Result.Failure<Response<SynchronyTokenResponse>>(ex.Message);
            }
           

            return null; // Error Handling TO do
            
        }
       
    }
}
