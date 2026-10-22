using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using Segment = VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange.Segment;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class SegmentV4CancelAndRebookHandler : IRequestHandler<SegmentV4CancelAndRebookCommand, List<Segment>>
    {
        private readonly ILogger<SegmentV4CancelAndRebookHandler> _logger;
        private readonly IAir4SegmentV4 _segmentV4RequestOperation;
        public SegmentV4CancelAndRebookHandler(IAir4SegmentV4 SegmentV4RequestOperation, ILogger<SegmentV4CancelAndRebookHandler> logger)
        {
            _segmentV4RequestOperation = SegmentV4RequestOperation;
            _logger = logger;
        }
        public async Task<List<Segment>> Handle(SegmentV4CancelAndRebookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("SegmentV4 Repo Command Handler for Cancel and Rebook, Session id:" + request.SessionId);
                CancelAndRebookResponseType cancelAndRebookResponseType = _segmentV4RequestOperation.CancelAndRebookSegments(SegmentV4CancelRebookReqResMapper.CancelRebookReqMapper(request.SessionId, request.NumberInParty, request.CancelSegments, request.RebookSegments, request.ChannelInfo));
                _logger.LogDebug("Getting CancelAndRebook Response");
                return SegmentV4CancelRebookReqResMapper.CancelRebookResMapper(cancelAndRebookResponseType).ToList();
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.SegmentV4Exception(e, ErrorCodes.DeltaCancelRebookErrorCode);
            }
        }
    }
}
