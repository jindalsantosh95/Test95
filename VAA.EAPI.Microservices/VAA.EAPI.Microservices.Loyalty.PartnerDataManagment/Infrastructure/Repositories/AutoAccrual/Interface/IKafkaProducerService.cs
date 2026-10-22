namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface
{
    public interface IKafkaProducerService
    {
        Task PublishAsync(string encryptedPayload, CancellationToken cancellationToken);
    }
}