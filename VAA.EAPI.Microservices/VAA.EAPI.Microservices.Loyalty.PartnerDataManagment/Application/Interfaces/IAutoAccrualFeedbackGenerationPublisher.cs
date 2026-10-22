namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface IAutoAccrualFeedbackGenerationPublisher
    {
        Task PublishAsync(string correlationId, string messageId, CancellationToken cancellationToken);
    }
}
