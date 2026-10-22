namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ContactTracing
{
    public class ContactTracingRequest
    {
        public bool ApplyToAllPassengers { get; set; }
        public IEnumerable<ContactTracing> ContactTracings { get; set; }
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public IEnumerable<string> SelectedPassengersIds { get; set; }


        public static explicit operator DeltaSsci.ContactTracingRequest(ContactTracingRequest source)
        {
            return new DeltaSsci.ContactTracingRequest()
            {
                StreetAddress = source.ContactTracings.First().InTransit == true ? Constants.Application.ContactTracingInTransit : source.ContactTracings.First().StreetAddress,
                City = source.ContactTracings.First().City,
                State = source.ContactTracings.First().State,
                Zip = source.ContactTracings.First().Zip,
                Email = source.ContactTracings.First().Email,
                Phone = source.ContactTracings.First().PrimaryPhoneNumber,
                CountryCode = source.ContactTracings.First().PrimaryPhoneNumberCountryCode,
                Phone2 = source.ContactTracings.First().SecondaryPhoneNumber,
                CountryCode2 = source.ContactTracings.First().SecondaryPhoneNumberCountryCode,
                SkipEmail = source.ContactTracings.First().NoEmail == true,
                SkipPhone2 = source.ContactTracings.First().NoSecondaryPhoneNumber == true,
                ApplyForAll = source.ApplyToAllPassengers,
            };
        }
    }
}
