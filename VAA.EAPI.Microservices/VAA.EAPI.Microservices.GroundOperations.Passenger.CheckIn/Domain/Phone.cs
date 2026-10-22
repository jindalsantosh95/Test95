namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Phone
    {
        public string CountryCode { get; set; }
        public string Number { get; set; }

        public static explicit operator Phone(DeltaTrm.PhoneInfo trmPhone)
        {
            return new Phone()
            {
                CountryCode = trmPhone.CountryCode,
                Number = trmPhone.PhoneNumber
            };
        }
    }
}