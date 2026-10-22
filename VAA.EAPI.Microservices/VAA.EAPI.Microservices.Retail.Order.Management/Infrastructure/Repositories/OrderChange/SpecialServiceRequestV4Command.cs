using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class SpecialServiceRequestV4Command : IRequest<AddResponseType>
    {
        public string SessionId { get; set; }
        public string Location { get; set; }
        public string TransactionId { get; set; }
        public SpecialServiceRequestInfo specialServiceRequestInfo { get; set; }
        public Channel ChannelInfo { get; set; }
    }
}
