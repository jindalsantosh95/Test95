using MediatR;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class MillionMilerEmailTriggerCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MemberNumber { get; set; }
        public string CustomerId { get; set; }
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
    }
}
