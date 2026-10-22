using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface
{
    public interface IVSRetroFlownDBRepository
    {
        Task<VSFlownDetailsResponse> GetVSFlownDetailsByTicketNumber(string ticketNumber);
        Task<VSFlownDetailsResponse> GetVSRetroFlownDetails(string ticketNumber, string pnr, string flightNumber);
        Task<bool> UpdateVSRetroFlownDetailsAsync(UpdateVSFlownDetailsRequest request, CancellationToken cancellationToken);
    }
}
