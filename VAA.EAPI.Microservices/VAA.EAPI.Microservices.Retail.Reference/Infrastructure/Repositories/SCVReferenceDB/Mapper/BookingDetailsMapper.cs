using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Mapper
{
    public class BookingDetailsMapper
    {
        public static IEnumerable<BookingDetails> MapBookingDetailsData(IEnumerable<PartnerBooking> partnerBooking)
        {
            return partnerBooking.Select(item => (BookingDetails)item);
        }


        public static PreValidationPartnerData MapBookingDetailsDataV2(IEnumerable<PartnerBookingV2> partnerBookingData)
        {
            return new PreValidationPartnerData
            {
                ListOfValues = partnerBookingData.ToDictionary(r => r.Type, r => r.Value),
                PartnerTerms = partnerBookingData.Select(r => new PartnerTerms
                {
                    RetroClaimPeriod = r.RetroClaimPeriod,
                    AgreementStartDate = r.AgreementStartDate,
                    AgreementEndDate = r.AgreementEndDate,
                    RetroClaimMinimumPeriod = r.RetroClaimMinimumPeriod,
                    PartnerCode = r.PartnerCode
                }),
                PartnerBookingClasses = partnerBookingData.Select(r => new PartnerBookingClass
                {
                    RedemptionClassIndicator = r.RedemptionClassIndicator,
                    BookingClass = r.BookClass,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    CabinClass = r.CabinClass,
                    PartnerCode = r.PartnerCode
                }),
                Distance = (Distance)partnerBookingData.First()
            };
        }
    }
}
