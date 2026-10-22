using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface
{
    public interface ISkyTeamAutoAccrualKafkaPublisher
    {
        Task PublishAsync(DistanceBasedAccrualPayload accrualRequest, CancellationToken cancellationToken);
    }
}
