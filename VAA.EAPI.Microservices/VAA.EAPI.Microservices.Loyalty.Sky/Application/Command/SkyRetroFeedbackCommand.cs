using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Application.Command
{
    public class SkyRetroFeedbackCommand : IRequest<Response<SkyOnlineRetroFeedbackResponse>>
    {
        public FeedbackRequest FeedbackRequest { get; set; }

        public SkyRetroFeedbackCommand(FeedbackRequest feedbackRequest)
        {
            FeedbackRequest = feedbackRequest;
        }
    }
}
