using System.Data;

using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service.VSRetro.Interface
{
    public interface IVSRetroFlownDBService
    {
        public Task<DataTable> GetVSFlownDetailsAsync(string ticketNumber, string operatingCarrier, string noShowIndicator);
        public Task<DataTable> GetVSRetroFlownDetailsAsync(string ticketNumber, string pnr, string flightNumber);
        public Task<bool> UpdateVSRetroFlownDetailsAsync(UpdateVSFlownDetailsRequest request, CancellationToken cancellationToken);
    }
}
