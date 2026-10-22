using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class SegmentV4CancelAndRebookCommand : IRequest<List<Segment>>
    {
        public string SessionId { get; set; }
        public int NumberInParty { get; set; }
        public List<Segment> CancelSegments { get; set; }
        public List<Segment> RebookSegments { get; set; }
        public Channel ChannelInfo { get; set; }
    }
}
