using MediatR;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldAccountEmailTriggerCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MemberNumber { get; set; }
        public string CustomerId { get; set; }
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
        public string ChildFirstName { get; set; }
        public string ChildLastName { get; set; }
        public string ChildMemberNumber { get; set; }
        public bool IsYoungFlyerParentEmail { get; set; }
    }
}
