namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface IAutoAccrualFallbackHandler
    {
        Task HandleAsync(CancellationToken cancellationToken);
    }
}
