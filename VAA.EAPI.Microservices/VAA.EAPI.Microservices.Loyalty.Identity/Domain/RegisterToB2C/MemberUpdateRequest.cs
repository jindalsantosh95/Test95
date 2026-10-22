using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C
{
    public class MemberUpdateRequest : IRequest<bool>
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string CurrentEmailId { get; set; }
        public string NewEmailId { get; set; }
    }
}
