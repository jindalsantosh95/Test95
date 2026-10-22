namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Passenger
    {
        public string? CustomerId { get; set; }
        public string? NameNumber { get; set; }
        public string? PassengerId { get; set; }
        public string? AssociatedInfantId { get; set; }
        public LoyaltyMembership? LoyaltyMembership { get; set; }
        public PassengerType? PassengerType { get; set; }
        public string? Title { get; set; }
        public string? GivenNames { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public Phone? Phone { get; set; }
        public IEnumerable<Document>? Documents { get; set; }
        public IEnumerable<Slice>? Slices { get; set; }
        public IEnumerable<Ticket>? Tickets { get; set; }
        public string? FirstName { get; set; }
        public IEnumerable<Address>? Addresses { get; set; }
        public string? FirstNameNumber { get; set; }
        public string? LastNameNumber { get; set; }
        public bool? IsLeadPassenger { get; set; }

        public static explicit operator Passenger(Infrastructure.Proxy.DeltaMyTrips.Passenger source)
        {
            var loyaltyAccount = source.LoyaltyProgramAccounts?.FirstOrDefault();

            return new Passenger
            {
                CustomerId = source.CustomerId,
                PassengerId = source.PassengerResourceId,
                LastName = source.Surname,
                FirstName = source.GivenNames,
                FirstNameNumber = source.FirstNameNum,
                LastNameNumber = source.LastNameNum,
                PassengerType = (PassengerType)source?.PassengerType,
                Tickets = source.Tickets?.Select(ticket => (Ticket)ticket),
                Slices = source.PassengerTrips?.Select(slice => (Slice)slice),
                LoyaltyMembership = loyaltyAccount != null ? new LoyaltyMembership() { Number = loyaltyAccount?.Number, ProgrammeCode = loyaltyAccount?.LoyaltyProgram?.Owner?.FirstOrDefault()?.Code, TierName = loyaltyAccount?.LoyaltyProgram?.Owner?.FirstOrDefault()?.Code == "VS" && string.IsNullOrEmpty(loyaltyAccount?.TierLevelName) ? "Red" : loyaltyAccount?.TierLevelName } : null
            };
        }
    }
}
