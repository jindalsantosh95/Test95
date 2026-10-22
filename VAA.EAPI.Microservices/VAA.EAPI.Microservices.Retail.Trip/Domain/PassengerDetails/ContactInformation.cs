using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class ContactInformation
    {
        public Email Email { get; set; }
        public Phone Phone { get; set; }
        public bool Refused { get; set; }
        public string RelationshipToPassenger { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public bool ApplyToAllPassengers { get; set; }

        public static explicit operator ContactInformation(DeltaPaxInfo.ContactInformation source)
        {
            if (source is null) return null;

            return new ContactInformation()
            {
                Email = (Email)source.Email,
                Phone = (Phone)source.Phone,
                Refused = source.Refused,
                RelationshipToPassenger = source.RelationshipToPassenger,
                GivenNames = source.GivenNames,
                Surname = source.Surname,
                ApplyToAllPassengers = source.ApplyToAllPassengers
            };
        }
    }
}