using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Domain;
using VAA.EAPI.Microservices.CRM.Infrastructure.Helpers;

namespace VAA.EAPI.Microservices.CRM.Infrastructure.Repository
{
    public class RetrievePaymentDetailsRepoQueryHandler : IRequestHandler<RetrievePaymentDetailsRepoQuery, PaymentDetails>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<RetrievePaymentDetailsRepoQueryHandler> _logger;

        public RetrievePaymentDetailsRepoQueryHandler(IHttpConnector apiConnector, ILogger<RetrievePaymentDetailsRepoQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaymentDetails> Handle(RetrievePaymentDetailsRepoQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Retrieve Payment Details Repo handler");
            var bearerToken = await CRMUtility.GetCRMToken();
            var headers = new Dictionary<string, string>();
            string url;
            if (ConfigReader.Get(Constants.Environment.CallCRMAPIMUrl) == "N")
            {
                url = $"{CRMUtility.GetVAAOrgCRMURI()}/{Constants.Application.CRMPaymentEntity}?{ConfigReader.Get(Constants.Environment.CRMWebApiPaymentFilterParams)}".Replace("{paymentNumber}", request.PaymentNumber);
            }
            else
            {
                headers = await CRMUtility.GetHeaders();
                url = $"{CRMUtility.CRMWebAPIGETAPIMURL(Constants.Application.CRMPaymentEntity)}?{ConfigReader.Get(Constants.Environment.CRMWebApiPaymentFilterParams)}".Replace("{paymentNumber}", request.PaymentNumber);
            }
            try
            {
                return await _apiConnector.GetAsync<PaymentDetails>(uri: url, headers: headers, bearerToken: bearerToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in RetrievePaymentDetailsRepoQueryHandler |{ex.Message} ");
                throw new DomainException(ConfigReader.Get<bool>(Constants.Environment.CRMWebApiResponsiveFeature) ? Constants.ErrorCodes.PaymentFetchError : Constants.ErrorCodes.PaymentUpdateError);

            }

        }
    }
}
