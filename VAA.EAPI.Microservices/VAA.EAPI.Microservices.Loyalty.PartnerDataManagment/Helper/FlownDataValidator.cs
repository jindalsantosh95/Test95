using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper
{
    public static class FlownDataValidator
    {
        public static bool ValidateMandatoryFields(AutoAccrualFlownDetails flownRecord)
        {
            return !flownRecord.FFPCarrierCode.IsNullOrWhiteSpace()
                && !flownRecord.FFPAccountNumber.IsNullOrWhiteSpace()
                && !flownRecord.FirstName.IsNullOrWhiteSpace()
                && !flownRecord.LastName.IsNullOrWhiteSpace()
                && !flownRecord.MarketingAirlineCode.IsNullOrWhiteSpace()
                && !flownRecord.MarketingFlightNum.IsNullOrWhiteSpace()
                && !flownRecord.OperatingAirlineCode.IsNullOrWhiteSpace()
                && !flownRecord.DepartureDate.IsNullOrWhiteSpace()
                && !flownRecord.OriginPort.IsNullOrWhiteSpace()
                && !flownRecord.DestinationPort.IsNullOrWhiteSpace()
                && !flownRecord.TicketNumber.IsNullOrWhiteSpace()
                && !flownRecord.FlownCabinClass.IsNullOrWhiteSpace()
                && !flownRecord.MarketingCarrierBookingClass.IsNullOrWhiteSpace()
                && !flownRecord.OperatingCarrierBookingClass.IsNullOrWhiteSpace()
                && !flownRecord.OperatingAirAuthNumber.IsNullOrWhiteSpace();
        }
    }
}
