using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Enums;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro
{
    public class UpdateVATicketStatusCommand : IRequest<Result<Response<string>>>
    {
        public string TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string TicketStatus { get; set; }
        public string? MemberNumber { get; set; }
        public string? LoyaltyProgrammeCode { get; set; }
        public string? PostingTypeCode { get; set; }


        public UpdateVATicketStatusCommand(TicketStatusRequest ticketStatusRequest)
        {
            TicketId = ticketStatusRequest.FlownRecordId ?? throw new ArgumentNullException(nameof(ticketStatusRequest.FlownRecordId));

            TicketNumber = ticketStatusRequest.TicketNumber ?? throw new ArgumentNullException(nameof(ticketStatusRequest.TicketNumber));

            TicketStatus = ticketStatusRequest.TicketStatus ?? TicketStatusEnum.ALLOCATED.ToString();
            MemberNumber = ticketStatusRequest.MemberNumber;
            LoyaltyProgrammeCode = ticketStatusRequest.LoyaltyProgrammeCode;
            PostingTypeCode = ticketStatusRequest.PostingTypeCode;
        }
    }
}
