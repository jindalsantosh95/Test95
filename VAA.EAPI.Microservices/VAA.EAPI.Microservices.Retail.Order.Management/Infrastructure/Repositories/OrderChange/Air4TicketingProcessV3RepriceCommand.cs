using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class Air4TicketingProV3RepriceCommand : IRequest<List<string>>
    {
        public string SessionId { get; set; }
        public string Location { get; set; }
        public string TransactionId { get; set; }
        public Traveller Trvallers { get; set; }
        public Channel ChannelInfo { get; set; }
    }

}
