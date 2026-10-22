using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories
{
    public class Air4CreateSessionCommandHandler : IRequestHandler<Air4CreateSessionCommand, PnrSessionDetails>
    {
        private readonly IAir4PnrCommonV6 _iAir4PnrCommonV6;
        private readonly ILogger<Air4CreateSessionCommandHandler> _logger;
        public Air4CreateSessionCommandHandler(IAir4PnrCommonV6 iAir4PNRCommonV6, ILogger<Air4CreateSessionCommandHandler> logger)
        {
            _iAir4PnrCommonV6 = iAir4PNRCommonV6;
            _logger = logger;
        }
        public async Task<PnrSessionDetails> Handle(Air4CreateSessionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var pnrCommonRequest = PNRCommonV6ReqResMapper.PNRCommonV6Req(request.transactionId, request.location, request.pnrNumber, request.ChannelInfo);
                var serviceResponse = _iAir4PnrCommonV6.Air4SetupContext(pnrCommonRequest);

                //Check for Married Segments
                if (serviceResponse.Pnr.SegmentDl is not null)
                {
                    if (serviceResponse.Pnr.SegmentDl.Any(x => !string.IsNullOrEmpty(x.OandDTripNumber)))
                    {
                        if (CheckForMarriedSegmentsError(serviceResponse, request))
                        {
                            throw new ExternalServiceIntegrationException(ErrorCodes.MarriedSegmentErrorCode, new Exception($"{ErrorCodes.MarriedSegmentErrorMessage}"));
                        }
                    }
                }
                return PNRCommonV6ReqResMapper.PNRCommonV6Res(serviceResponse);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.PNRCommonV6Exception(e, ErrorCodes.DeltaSetupContextErrorCode);
            }
        }
        public static bool CheckForMarriedSegmentsError(SetupContextResponseType serviceResponse, Air4CreateSessionCommand request)
        {
            bool errorFlag = false;
            var marriageGroupList = serviceResponse.Pnr.SegmentDl.GroupBy(x => x.OandDTripNumber).Distinct();
            var requestList = request.CancelSegments?.Select(x => x.MarketingFlightNumber.PadLeft(4, '0'));
            if (requestList is null)
                return errorFlag;

            foreach (var group in marriageGroupList.Where(x => x.Key != null))
            {
                var marraigeGroup = group.Where(x => x.OandDTripNumber == group.Key).Select(y => y.FlightNumber);
                if (marraigeGroup.Intersect(requestList).Any())
                {
                    var marriedSegInReq = marraigeGroup.Intersect(requestList);
                    errorFlag = !marraigeGroup.SequenceEqual(marriedSegInReq);
                    if (errorFlag)
                        break;
                }
                else
                {
                    errorFlag = false;
                }
            }
            return errorFlag;
        }

    }
}
