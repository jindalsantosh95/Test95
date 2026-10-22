using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class Air4TicketingProcessV3RepriceCommandHandler : IRequestHandler<Air4TicketingProV3RepriceCommand, List<string>>
    {
        private readonly IAir4TicketingProcessV3 _iAir4TicketingProcessV3;
        private readonly ILogger<Air4TicketingProcessV3RepriceCommandHandler> _logger;
        public Air4TicketingProcessV3RepriceCommandHandler(IAir4TicketingProcessV3 iAir4TicketingProcessV3, ILogger<Air4TicketingProcessV3RepriceCommandHandler> logger)
        {
            _iAir4TicketingProcessV3 = iAir4TicketingProcessV3 ?? throw new ArgumentNullException(nameof(iAir4TicketingProcessV3));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<List<string>> Handle(Air4TicketingProV3RepriceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ticketingProcessV3Request = TicketingProcessV3ReqResMapper.TicketingProcessV3RepriceReqMapper(request.SessionId, request.TransactionId, request.Location, request.Trvallers, request.ChannelInfo);
                var serviceResponse = await _iAir4TicketingProcessV3.Reprice(ticketingProcessV3Request);

                if (serviceResponse?.Error?.Length > 0)
                {
                    throw new ExternalServiceIntegrationException(ErrorCodes.DeltaErrorCode, new Exception($"{Constants.Application.ErrorCode} : {serviceResponse.Error[0].Number}  {Constants.Application.ErrorMessage} : {serviceResponse.Error[0].Description}"));
                }

                return TicketingProcessV3ReqResMapper.RepriceResponseTypeMapper(serviceResponse);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.TicketingProcessV3Exception(e, ErrorCodes.DeltaRePriceErrorCode);
            }
        }
    }
}
