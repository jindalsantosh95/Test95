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
    public class RetrieveCountryDetailsRepoQueryHandler : IRequestHandler<RetrieveCountryDetailsRepoQuery, CountryDetails>
    {
        private readonly ILogger<RetrievePaymentDetailsRepoQueryHandler> _logger;
        private readonly IHttpConnector _apiConnector;

        public RetrieveCountryDetailsRepoQueryHandler(IHttpConnector apiConnector, ILogger<RetrievePaymentDetailsRepoQueryHandler> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CountryDetails> Handle(RetrieveCountryDetailsRepoQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Retrieve Country Details Repo handler");
            var bearerToken = await CRMUtility.GetCRMToken();
            var headers = new Dictionary<string, string>();
            string url;
            if (ConfigReader.Get(Constants.Environment.CallCRMAPIMUrl) == "N")
            {
                url = $"{CRMUtility.GetVAAOrgCRMURI()}/{Constants.Application.CRMCountryEntity}?{ConfigReader.Get(Constants.Environment.CRMWebApiCountryFilterParams)}".Replace("{countryName}", request.CountryName.Replace("'", "''"));
            }
            else
            {

                headers = await CRMUtility.GetHeaders();
                url = $"{CRMUtility.CRMWebAPIGETAPIMURL(Constants.Application.CRMCountryEntity)}?{ConfigReader.Get(Constants.Environment.CRMWebApiCountryFilterParams)}".Replace("{countryName}", request.CountryName.Replace("'", "''"));
            }

            try
            {
                return await _apiConnector.GetAsync<CountryDetails>(uri: url, headers: headers, bearerToken: bearerToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in RetrieveCountryDetailsRepoQueryHandler |{ex.Message} ");
                throw new DomainException(ConfigReader.Get<bool>(Constants.Environment.CRMWebApiResponsiveFeature) ? Constants.ErrorCodes.CountryFetchError : Constants.ErrorCodes.PaymentUpdateError);
            }
        }

    }
}
