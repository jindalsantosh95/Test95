namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class LoyaltyMembership
    {
        public string ProgrammeCode { get; set; }
        public string Number { get; set; }

        public static explicit operator LoyaltyMembership(DeltaSsci.Loyalty source)
        {
            return new LoyaltyMembership()
            {
                ProgrammeCode = source.Name,
                Number = source.Number
            };
        }
    }
}