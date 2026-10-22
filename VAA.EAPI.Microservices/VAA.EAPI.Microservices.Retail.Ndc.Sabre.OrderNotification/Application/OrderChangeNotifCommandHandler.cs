using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Application
{
    public class OrderChangeNotifCommandHandler : IOrderChangeNotifCommand
    {
        private readonly IAgentOCNEndpointService _agentOCNEndpointService;
        private readonly IPortalOCNEndpointService _portalOCNEndpointService;
        private readonly ILogger<OrderChangeNotifCommandHandler> _logger;
        public OrderChangeNotifCommandHandler(IAgentOCNEndpointService agentOCNEndpointService, IPortalOCNEndpointService portalOCNEndpointService, ILogger<OrderChangeNotifCommandHandler> logger)
        {
            _agentOCNEndpointService = agentOCNEndpointService;
            _portalOCNEndpointService = portalOCNEndpointService;
            _logger = logger;
        }

        public async Task<Result<Response<OrderChangeNotifResponse>>> Handle(OrderChangeNotifRequest request)
        {
            try
            {
                var agentResponses = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                var ocnReq = request?.IsDuplicatePNR == true ? OrderChangeNotifRQMapper.MapDuplicatePnrRequest(request.OrderDocument) :
                             request?.TTLExpiryType == Constants.Application.ProactiveTtlExpiry ? OrderChangeNotifRQMapper.MapProactiveTTLRequest(request.OrderDocument)
                           : request?.TTLExpiryType == Constants.Application.ReactiveTtlExpiry ? OrderChangeNotifRQMapper.MapReactiveTTLRequest(request.OrderDocument)
                           : request?.OrderDocument?.ChangeType == ChangeTypeEnum.DocumentRefunded || request?.OrderDocument?.ChangeType == ChangeTypeEnum.DocumentVoided ? OrderChangeNotifRQMapper.MapCancelledRequest(request.OrderDocument)
                           : OrderChangeNotifRQMapper.MapRequest(request.OrderDocument);
                try
                {
                    foreach (var endpoint in request.AgentEndpointDetails)
                    {                        
                        endpoint.EndpointDetails.ClientKey = CredentialsDecryption.Decrypt(endpoint.EndpointDetails.ClientKey);
                        endpoint.EndpointDetails.ClientSecret = CredentialsDecryption.Decrypt(endpoint.EndpointDetails.ClientSecret);

                        var response = await _agentOCNEndpointService.Send(ocnReq, endpoint.EndpointDetails);
                        var agencyId = endpoint.AgencyDetails.RequestAgencyId;
                        var acknowledgementReceived = response?.Notification?.StatusCode.Is("OK") == true;
                        
                        agentResponses[agencyId] = agentResponses.TryGetValue(agencyId, out var previousAcknowledgement)
                            ? previousAcknowledgement && acknowledgementReceived
                            : acknowledgementReceived;
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(exception, "Agent OCN endpoint request failed. {Operation}", "AgentOrderChangeNotification");
                }

                var portalResponse = await _portalOCNEndpointService.Send(ocnReq);
                var portalAcknowledgementReceived = portalResponse.Notification.StatusCode.Is("OK");

                return Result.Success(new Response<OrderChangeNotifResponse>()
                {
                    Status = Status.OK,
                    Value = new()
                    {
                        AgentResponse = agentResponses.Select(x => new AgentResponse
                        {
                            AgentId = x.Key,
                            AcknowledgementRcvd = x.Value
                        }).Append(new AgentResponse
                        {
                            AgentId = "portal",
                            AcknowledgementRcvd = portalAcknowledgementReceived
                        })
                    }
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Order change notification processing was cancelled. {Operation}", "OrderChangeNotification");
                return Result.Failure<Response<OrderChangeNotifResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Order change notification processing failed. {Operation}", "OrderChangeNotification");
                return Result.Failure<Response<OrderChangeNotifResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
