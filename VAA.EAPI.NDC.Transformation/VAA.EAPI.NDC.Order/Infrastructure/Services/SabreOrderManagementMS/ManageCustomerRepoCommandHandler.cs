using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS
{
    public class ManageCustomerRepoCommandHandler : IRequestHandler<ManageCustomerRepoCommand, ChangeOrderRS>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<ManageCustomerRepoCommandHandler> _logger;

        public ManageCustomerRepoCommandHandler(IHttpConnector httpConnector, ILogger<ManageCustomerRepoCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChangeOrderRS> Handle(ManageCustomerRepoCommand request, CancellationToken cancellationToken)
        {
            StringContent content = new(Json.Serialize(request.ManageCustomerRequest));
            var patchrequest = new HttpRequestMessage(HttpMethod.Patch, ConfigReader.Get(Constants.Environment.ManageCustomerMSUrl))
            {
                Content = content
            };

            patchrequest.Headers.Add("Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderChangeMSSubscriptionKey));

            var httpResponseMessage = await _httpConnector.SendAsync(patchrequest, cancellationToken);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new ChangeOrderRS() { orderDocument = Json.Deserialize<OrderDocument>(response) };
            else
            {
                var mgmtResponse = Json.Deserialize<Result<Response<OrderDocument>>>(response);

                return new ChangeOrderRS() { error = Json.Deserialize<List<ErrorDetails>>(mgmtResponse.Error) };
            }
        }
    }
}

