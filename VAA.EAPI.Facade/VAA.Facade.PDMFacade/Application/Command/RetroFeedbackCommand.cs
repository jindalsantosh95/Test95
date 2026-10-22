using VAA.Facade.PDMFacade.Domain;
using VAA.EAPI.Domain.Entities.Common;
using MediatR;

namespace VAA.Facade.PDMFacade.Application.Command
{
    public class RetroFeedbackCommand :IRequest<Response<RetroFeedbackResponse>>
    {
        public RetroFeedbackRequest RetroFeedbackRequest { get; set; }        
    }
}
