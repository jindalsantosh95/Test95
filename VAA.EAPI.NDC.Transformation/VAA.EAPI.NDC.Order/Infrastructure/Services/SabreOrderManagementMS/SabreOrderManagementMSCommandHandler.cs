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
    public class SabreOrderManagementMSCommandHandler : IRequestHandler<SabreOrderManagementMSCommand, ChangeOrderRS>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<SabreOrderManagementMSCommandHandler> _logger;

        public SabreOrderManagementMSCommandHandler(IHttpConnector httpConnector, ILogger<SabreOrderManagementMSCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChangeOrderRS> Handle(SabreOrderManagementMSCommand request, CancellationToken cancellationToken)
        {
            StringContent content = new(Json.Serialize(request.OrderChangeRequest));
            var changeOrderServiceuri = ConfigReader.Get(Constants.Environment.OrderChangeMSUrl) + request.OrderChangeRequest.RLOC;

            var patchrequest = new HttpRequestMessage(HttpMethod.Patch, changeOrderServiceuri)
            {
                Content = content
            };

            patchrequest.Headers.Add("Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderChangeMSSubscriptionKey));
            patchrequest.Headers.Add("Channelid", Constants.Application.NDC);

            var httpResponseMessage = await _httpConnector.SendAsync(patchrequest, cancellationToken);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new ChangeOrderRS() { orderDocument = Json.Deserialize<OrderDocument>(response) };
                }
                else
                {
                    var mgmtResponse = Json.Deserialize<Result<Response<OrderDocument>>>(response);

                    return new ChangeOrderRS() { error = Json.Deserialize<List<ErrorDetails>>(mgmtResponse.Error) };
                }
            }
        }
    }

