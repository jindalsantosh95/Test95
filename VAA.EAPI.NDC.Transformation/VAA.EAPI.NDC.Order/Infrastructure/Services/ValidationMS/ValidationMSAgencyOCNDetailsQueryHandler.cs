using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS
{
    public class ValidationMSAgencyOCNDetailsQueryHandler : IRequestHandler<ValidationMSAgencyOCNDetailsQuery, IEnumerable<AgencyOCNDetails>>
    {
        private readonly IHttpConnector _httpConnector;

        public ValidationMSAgencyOCNDetailsQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<AgencyOCNDetails>> Handle(ValidationMSAgencyOCNDetailsQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ValidationMSSubscriptionKey)},
                { "environment" , request.Environment }
            };
            var uri = ConfigReader.Get(Constants.Environment.ValidationMSAgencyOCNDetailsUrl) + Uri.EscapeDataString(request.AgencyId);

            if (!string.IsNullOrWhiteSpace(request.AggregatorId))
                uri += "?aggregatorId=" + Uri.EscapeDataString(request.AggregatorId);

            var httpResponseMessage = await _httpConnector.SendAsync(uri, headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<IEnumerable<AgencyOCNDetails>>(response);
            else
                return null;
        }
    }
}
