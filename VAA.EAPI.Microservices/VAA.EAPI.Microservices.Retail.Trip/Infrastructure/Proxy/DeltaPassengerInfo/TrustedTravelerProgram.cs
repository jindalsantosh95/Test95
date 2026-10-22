
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class TrustedTravelerProgram
    {
        public string Number { get; set; }
        public TrustedTravelerProgramCodeType Program { get; set; }

        public static explicit operator TrustedTravelerProgram(Domain.PassengerDetails.TrustedTravelerProgram source)
        {
            if (source is null) return null;

            return new TrustedTravelerProgram()
            {
                Number = source.Number,
                Program = (TrustedTravelerProgramCodeType)source.Program
            };
        }
    }
}