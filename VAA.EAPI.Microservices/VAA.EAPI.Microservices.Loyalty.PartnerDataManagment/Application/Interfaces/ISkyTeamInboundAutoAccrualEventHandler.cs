namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface ISkyTeamInboundAutoAccrualEventHandler
    {
        Task<bool> Handle(string inboundMessage, CancellationToken cancellationToken);
    }
}
