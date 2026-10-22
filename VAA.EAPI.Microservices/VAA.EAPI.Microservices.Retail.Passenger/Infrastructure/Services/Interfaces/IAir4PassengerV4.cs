using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces
{
    public interface IAir4PassengerV4
    {
        Task<AddResponse> Air4AddPassenger(AddRequestType addRequestType);
        Task<AddPTCCodesResponse> Air4AddPTC(AddPTCCodesRequestType addPTCCodesRequestType);
        Task<AddPhoneResponse> Air4AddPhone(AddPhoneRequestType addPhoneRequestType);
        Task<ManageMultipleTravelDocsAndSecureFlightResponse> Air4ManageMultipleTravelDocsAndSecureFlight(ManageMultipleTravelDocsAndSecureFlightRequestType manageMultipleTravelDocsAndSecureFlightRequestType);
        Task<ManageMultipleInfantV2Response> Air4ManageMultipleInfant(ManageMultipleInfantV2RequestType manageMultipleInfantRequestType);
    }
}
