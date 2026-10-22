using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class PassengerInformationRequest
    {
        public RequestOptionCodeType Using { get; set; }
        public string ConfirmationNum { get; set; }
        public string EncryptedConfirmationNum { get; set; }
        public string TicketNum { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }

        public static explicit operator PassengerInformationRequest(DeltaPaxInfo.PassengerInformationRequest source)
        {
            if (source is null) return null;

            return new PassengerInformationRequest()
            {
                Using = (RequestOptionCodeType)source.Using,
                ConfirmationNum = source.ConfirmationNum,
                EncryptedConfirmationNum = source.EncryptedConfirmationNum,
                TicketNum = source.TicketNum,
                GivenNames = source.GivenNames,
                Surname = source.Surname
            };
        }
    }
}