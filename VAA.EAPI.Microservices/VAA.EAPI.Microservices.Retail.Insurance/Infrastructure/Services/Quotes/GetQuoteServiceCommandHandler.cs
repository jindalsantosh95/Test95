using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Insurance.Domain;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Services.OrderCreate
{
    public class GetQuoteServiceCommandHandler : IRequestHandler<GetQuoteServieCommand, InsuranceQuoteResponse>
    {
        private readonly ILogger<GetQuoteServiceCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        
        public GetQuoteServiceCommandHandler(ILogger<GetQuoteServiceCommandHandler> logger, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));          
        }

        public async Task<InsuranceQuoteResponse> Handle(GetQuoteServieCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var anzQuoteRequest = AllianzQuoteRequestMapper.MapQuoteRequest(request.Request);

                _logger.LogInformation(Json.Serialize(anzQuoteRequest));
                                
                var quoteResponse = await _apiConnector.SendAsync<QuoteResponse>( ConfigReader.Try(Insurance.Constants.Environment.AllianzQuoteCreateUri), headers: null, null, verb: HttpMethod.Post, content: Json.Serialize(anzQuoteRequest, false));

                
                return AllianzQuoteResponseMapper.MapInsuranceQuoteResponse(quoteResponse, request.Request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation id - {AsyncContext.Current.CorrelationId} Request Processing Terminated for GetQuoteServiceCommandHandler with Allianz Request: - {ex.Data["Response"]?.ToString()} - Allianz Response {ex.Data["ResponseData"]?.ToString()}");
                throw;
            }
        }
    }
}
