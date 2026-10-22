using MediatR;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository
{
    public class SkyRetroFeedbackRepoCommand : IRequest<SkyOnlineRetroFeedbackResponse>
    {
        public FeedbackRequest FeedbackRequest { get; set; }

        public SkyRetroFeedbackRepoCommand(FeedbackRequest feedbackRequest)
        {
            FeedbackRequest = feedbackRequest;
        }
    }
}
