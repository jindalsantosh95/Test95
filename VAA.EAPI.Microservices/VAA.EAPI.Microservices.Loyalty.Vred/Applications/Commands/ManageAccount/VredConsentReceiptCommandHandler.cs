using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount
{
    public class VredConsentReceiptCommandHandler : IRequestHandler<VredConsentReceiptCommand, Result<Response<string>>>
    {
        private readonly ILogger<VredConsentReceiptCommandHandler> _logger;
        private readonly IHttpConnector _httpConnector;

        public VredConsentReceiptCommandHandler(ILogger<VredConsentReceiptCommandHandler> logger, IHttpConnector httpConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<string>>> Handle(VredConsentReceiptCommand request, CancellationToken cancellationToken)
        {
            HttpResponseMessage httpResponseMessage;

            var requestObject = VRedRequestMapper.MapToVredMarketingConsentRequest(request);

            var requestContent = Json.Serialize(requestObject, new JsonSerializerSettings());

            string vredUri = ConfigReader.Get(Constants.Environment.VRedConsentEndpointUrl);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format(vredUri, requestContent))
            {
                Content = new StringContent(requestContent, System.Text.Encoding.UTF8, "application/json")
            };

            httpResponseMessage = await _httpConnector.SendAsync(httpRequestMessage, cancellationToken);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var ex = new HttpResponseException(httpResponseMessage);
                ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);
                ex.Data.Add("Request", requestContent);
                return Result.Success(new Response<string>
                {
                    Errors = new List<ErrorDetails>() { new ErrorDetails { Message = ex.Message } },
                    Status = Status.Error
                });
            }

            return Result.Success(new Response<string>
            {
                Errors = null,
                Status = Status.OK
            });

        }
    }
}

