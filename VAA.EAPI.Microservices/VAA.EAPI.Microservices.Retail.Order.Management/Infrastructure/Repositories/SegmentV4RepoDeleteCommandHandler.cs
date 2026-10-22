using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories
{
    public class SegmentV4RepoDeleteCommandHandler : IRequestHandler<SegmentV4RepoDeleteCommand, bool>
    {
        private readonly ILogger<SegmentV4RepoDeleteCommandHandler> _logger;
        private readonly IAir4SegmentV4 _segmentV4RequestOperation;
        public SegmentV4RepoDeleteCommandHandler(IAir4SegmentV4 SegmentV4RequestOperation, ILogger<SegmentV4RepoDeleteCommandHandler> logger)
        {
            _segmentV4RequestOperation = SegmentV4RequestOperation;
            _logger = logger;
        }
        public async Task<bool> Handle(SegmentV4RepoDeleteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("SegmentV4 Repo Command Handler for Delete/Insert request, Session id:" + request.SessionId + "Transaction id:" + request.TransactionId);
            DeleteResponseType deleteResponseType = _segmentV4RequestOperation.DeleteSegments(SegmentV4ReqResMapper.DeleteRequestTypeMapper(request.SessionId, request.TransactionId, request.Location, request.NumberInParty, request.Segments, request.ChannelInfo));
            return true;
        }
    }
}
