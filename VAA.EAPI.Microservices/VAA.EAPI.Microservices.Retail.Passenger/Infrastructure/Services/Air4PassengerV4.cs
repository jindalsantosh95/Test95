using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services
{
    public class Air4PassengerV4 : IAir4PassengerV4
    {
        readonly PassengerClient _passengerClient;
        public Air4PassengerV4(PassengerClient pnrCommonClient)
        {
            _passengerClient = pnrCommonClient;
        }

        public async Task<AddResponse> Air4AddPassenger(AddRequestType addRequestType)
        {
            try
            {
                if (_passengerClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _passengerClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return await _passengerClient.AddAsync(addRequestType);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.Air4Exception(e);
            }
        }

        public async Task<AddPhoneResponse> Air4AddPhone(AddPhoneRequestType addPhoneRequestType)
        {
            try
            {
                if (_passengerClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _passengerClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return await _passengerClient.AddPhoneAsync(addPhoneRequestType);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.Air4Exception(e);
            }
        }

        public async Task<AddPTCCodesResponse> Air4AddPTC(AddPTCCodesRequestType addPTCCodesRequestType)
        {
            try
            {
                if (_passengerClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _passengerClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return await _passengerClient.AddPTCCodesAsync(addPTCCodesRequestType);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.Air4Exception(e);
            }
        }

        public async Task<ManageMultipleTravelDocsAndSecureFlightResponse> Air4ManageMultipleTravelDocsAndSecureFlight(ManageMultipleTravelDocsAndSecureFlightRequestType manageMultipleTravelDocsAndSecureFlightRequestType)
        {
            try
            {
                if (_passengerClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _passengerClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return await _passengerClient.ManageMultipleTravelDocsAndSecureFlightAsync(manageMultipleTravelDocsAndSecureFlightRequestType);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.Air4Exception(e);
            }
        }
        public async Task<ManageMultipleInfantV2Response> Air4ManageMultipleInfant(ManageMultipleInfantV2RequestType manageMultipleInfantRequestType)
        {
            try
            {
                if (_passengerClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _passengerClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return await _passengerClient.ManageMultipleInfantV2Async(manageMultipleInfantRequestType);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.Air4Exception(e);
            }
        }
    }
}
