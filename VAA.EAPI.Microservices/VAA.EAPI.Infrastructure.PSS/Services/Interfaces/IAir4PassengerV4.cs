using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface IAir4PassengerV4
    {
        Task<ManageMultipleTravelDocsAndSecureFlightResponse> Air4ManageMultipleTravelDocsAndSecureFlight(ManageMultipleTravelDocsAndSecureFlightRequestType manageMultipleTravelDocsAndSecureFlightRequestType);

        Task<UpdateSFPDResponseType> PSDSSync(UpdateSFPDRequestType updateSFPDRequestType);
    }
}
