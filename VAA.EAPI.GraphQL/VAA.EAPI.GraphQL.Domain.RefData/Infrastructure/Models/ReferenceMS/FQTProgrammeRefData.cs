namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("fqtProgrammes", "fqtProgrammes", "")]
    public class FQTProgrammeRefData
    {
        public string ProgramCode { get; set; }
        public string AirlineName { get; set; }
        public string ProgramName { get; set; }
        public string MembNumRegex { get; set; }
    }
}