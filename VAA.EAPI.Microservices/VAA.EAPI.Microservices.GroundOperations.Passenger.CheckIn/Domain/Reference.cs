using NodaTime;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Reference
    {
        public string PnrLocator { get; set; }
        public LocalDate? CreatedDate { get; set; }

        public static explicit operator Reference(DeltaTrm.TravelRequirements? requirements)
        {
            var createdDate = DateTimeHelper.ParseLocalIsoDateTime(requirements?.BookingReference.CreationDate);
            return new Reference()
            {
                PnrLocator = requirements?.BookingReference.Id!,
                CreatedDate = createdDate != null ? createdDate.Value.Date : null
            };
        }
    }
}