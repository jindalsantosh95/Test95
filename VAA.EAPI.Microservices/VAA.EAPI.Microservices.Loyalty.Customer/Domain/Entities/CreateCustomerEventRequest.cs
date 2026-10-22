using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class CreateCustomerEventRequest : IRequest<bool>
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public string Id { get; set; }
        public string Time { get; set; }
        public string Datacontenttype { get; set; }
        public CreateCustomerEntities Data { get; set; }
    }
}
