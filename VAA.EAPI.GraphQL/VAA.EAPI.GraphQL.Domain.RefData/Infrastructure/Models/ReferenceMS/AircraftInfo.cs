namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("AircraftDetails", "AircraftDetails")]
    public class AircraftInfo
    {
        public string Code { get; set; }
        public string IataCode { get; set; }
        public string FullName { get; set; }
    }
}
