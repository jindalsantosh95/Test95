using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
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

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand
{
    public class ApplicationStatusServiceQueryHandler : IRequestHandler<ApplicationStatusServiceQuery, Result<Response<ApplicationStatusResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ISynchronyAccessTokenGenerator _synchronyToken;
        private readonly ILogger<ApplicationStatusServiceQueryHandler> _logger;

        public ApplicationStatusServiceQueryHandler(IHttpConnector httpConnector, ISynchronyAccessTokenGenerator synchronyToken, ILogger<ApplicationStatusServiceQueryHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _synchronyToken = synchronyToken ?? throw new ArgumentNullException(nameof(synchronyToken));
            _logger = logger;
        }

        public async Task<Result<Response<ApplicationStatusResponse>>> Handle(ApplicationStatusServiceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _synchronyToken.GetToken();

                var applicationStatusUrl = string.Concat(ConfigReader.Get(Constants.Environment.SynchronyDataShareAPIUrl).TrimEnd('/'), Constants.Application.SynchronyApplicationStatusUrl.Replace("{token}", request.SynchronyToken));
                var response = await _httpConnector.GetAsync<SynchronyApplicatoinStatusResponse>(applicationStatusUrl, null, token);

                if (response != null)
                {
                    return Result.Success(new Response<ApplicationStatusResponse>
                    {
                        Status = Status.OK,
                        Value = new()
                        {
                            ApplicationDecision = response.ApplicationStatus?.ApplicationDecision,
                            CreditLimit = response.ApplicationStatus?.CreditLimit,
                            TokenStatus = response.TokenStatus,
                            DataShareToken = response.DataShareToken,
                            TokenCreationDate = response.CreatedDate
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Result.Failure<Response<ApplicationStatusResponse>>(ex.Message);
            }
            

            return null; // Error Handling TO do
        }
       
    }
}
