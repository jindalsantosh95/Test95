
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData
{
    public class LoyaltyProgramDetails
    {
        public IEnumerable<LoyaltyProgram> LoyaltyPrograms { get; set; }

        public static explicit operator LoyaltyProgramDetails(FQTProgramDetails source)
        {
            if (source == null)
                return null;


            var priorityCodes = new List<string> { "VS", "DL", "AF", "KL" };
            return new LoyaltyProgramDetails
            {
                LoyaltyPrograms = source.FQTPrograms?
                .Select(fp => (LoyaltyProgram)fp)
                .OrderBy(x => priorityCodes.Contains(x.ProgramCode)
                                        ? priorityCodes.IndexOf(x.ProgramCode)
                                        : int.MaxValue)
                .ThenBy(x => x.ProgramName)
            };
        }
    }
}