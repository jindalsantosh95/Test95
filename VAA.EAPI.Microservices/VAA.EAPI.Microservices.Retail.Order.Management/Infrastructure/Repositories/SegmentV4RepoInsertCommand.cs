using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories
{
    public class SegmentV4RepoInsertCommand : IRequest<bool>
    {
        public string SessionId { get; set; }
        public string TransactionId { get; set; }
        public string Location { get; set; }
        public int NumberInParty { get; set; }
        public string ApplicationId { get; set; }
        public List<Segment> Segments { get; set; }
        public Channel ChannelEntity { get; set; }
    }
}
