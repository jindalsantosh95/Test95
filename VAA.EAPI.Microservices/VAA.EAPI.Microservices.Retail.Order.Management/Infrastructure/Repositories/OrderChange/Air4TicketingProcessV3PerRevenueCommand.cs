using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class Air4TicketingProcessV3PerRevenueCommand : IRequest<PerformRevenueResponseType>
    {
        public string SessionId { get; set; }
        public string Location { get; set; }
        public string TransactionId { get; set; }
        public string EmailAddress { get; set; }
        public Channel ChannelInfo { get; set; }
    }

}
