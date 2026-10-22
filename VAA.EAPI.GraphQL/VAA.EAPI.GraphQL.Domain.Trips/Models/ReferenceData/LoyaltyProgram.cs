
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData
{
    public class LoyaltyProgram
    {
        public string ProgramCode { get; set; }
        public string AirlineName { get; set; }
        public string ProgramName { get; set; }
        public string MembNumRegex { get; set; }

        public static explicit operator LoyaltyProgram(FQTProgramRefData source)
        {
            if (source == null)
            {
                return null;
            }

            return new LoyaltyProgram
            {
                ProgramCode = source.ProgramCode,
                AirlineName = source.AirlineName,
                ProgramName = source.ProgramName,
                MembNumRegex = source.MembNumRegex
            };
        }
    }
}
