namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class SeatEligiblePassengerInfoType
    {
        public string InfantCarryEligibleInd { get; set; }

        public string NonRevenuePassengerInd { get; set; }

        public string PassengerGivenName { get; set; }

        public string PassengerGivenNameNumber { get; set; }

        public string PassengerSurname { get; set; }

        public string PassengerSurnameNumber { get; set; }

        public string ReasonForNonEligibility { get; set; }
    }
}
