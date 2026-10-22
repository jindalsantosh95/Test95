namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Gender
    {
        public string Code { get; set; }

        public static explicit operator Gender(DeltaTrm.Gender trmGender)
        {
            return new()
            {
                Code = trmGender.Code
            };
        }
    }
}