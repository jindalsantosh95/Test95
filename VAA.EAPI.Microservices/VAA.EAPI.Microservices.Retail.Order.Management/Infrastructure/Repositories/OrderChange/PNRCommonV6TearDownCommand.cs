using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class PNRCommonV6TearDownCommand : IRequest<TeardownContextResponseType>
    {
        public string SessionId { get; set; }
        public string Location { get; set; }
        public string TransactionId { get; set; }
        public Channel ChannelInfo { get; set; }
    }
}
