using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit
{
    public class RetroFeedbackCommand : IRequest<Response<RetroFeedbackResponse>>
    {
        public RetroFeedbackRequest RetroFeedbackRequest { get; set; }
    }
}
