using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories
{
    public class Air4CreateSessionCommand : IRequest<PnrSessionDetails>
    {
        public string pnrNumber { get; set; }
        public string transactionId { get; set; }
        public string location { get; set; }
        public List<Segment> CancelSegments { get; set; }
        public Channel ChannelInfo { get; set; }

    }
}
