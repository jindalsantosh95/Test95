namespace VAA.EAPI.GraphQL.Domain.Account.Models.Retro
{
    public class MissingPointsResponse<T>
    {
        public string TotalVirginPoints { get; set; }
        public string TotalTierPoints { get; set; }
        public string TotalBonusPoints { get; set; }
        public IEnumerable<T> segmentDetails { get; set; }
        public ErrorResponse ErrorResponse { get; set; }
    }
}
