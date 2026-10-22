using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Infrastructure.Helpers;

namespace VAA.EAPI.Microservices.CRM.Infrastructure.Repository
{
    public class UpdatePaymentDetailsRepoCommandHandler : IRequestHandler<UpdatePaymentDetailsRepoCommand, HttpResponseHeaders>
    {
        private readonly ILogger<UpdatePaymentDetailsRepoCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;

        public UpdatePaymentDetailsRepoCommandHandler(ILogger<UpdatePaymentDetailsRepoCommandHandler> logger, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<HttpResponseHeaders> Handle(UpdatePaymentDetailsRepoCommand request, CancellationToken cancellationToken)
        {
            var bearerToken = await CRMUtility.GetCRMToken();
            var headers = new Dictionary<string, string>();
            string url;

            if (ConfigReader.Get(Constants.Environment.CallCRMAPIMUrl) == "N")
            {
                url = $"{CRMUtility.GetVAAOrgCRMURI()}/{Constants.Application.CRMPaymentEntity}({request.paymentEntity.VaaPaymentid})";
            }
            else
            {
                headers = await CRMUtility.GetHeaders();
                url = $"{CRMUtility.CRMWebAPIPatchAPIMURL(Constants.Application.CRMPaymentEntity, request.paymentEntity.VaaPaymentid)}";
            }

            try
            {
                var apiResponse = await _apiConnector.SendAsync(url, headers, bearerToken, System.Net.Http.HttpMethod.Patch, content: Json.Serialize(request.paymentEntity));

                return apiResponse.Headers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in UpdatePaymentDetailsRepoCommandHandler |{ex.Message} ");
                throw new DomainException(Constants.ErrorCodes.PaymentUpdateError);
            }
        }
    }
}
