using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface
{
    public interface ISkyTeamAutoAccrualFeedbackKafkaPublisher
    {
        Task<string> PublishAsync(DistanceBasedAccrualFeedbackPayload feedback, string correlationId, string messageId, CancellationToken cancellationToken);
    }
}
