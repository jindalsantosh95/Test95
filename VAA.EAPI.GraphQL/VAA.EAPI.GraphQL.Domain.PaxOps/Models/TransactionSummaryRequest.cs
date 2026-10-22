namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    public sealed class TransactionSummaryRequest
    {
        public string TravelPartyId { get; init; } = string.Empty;
        public string RecordLocator { get; init; } = string.Empty;
        public string OriginAirportCode { get; init; } = string.Empty;
        public string TransactionError { get; init; } = string.Empty;

        public static explicit operator TransactionSummaryRequest((string travelPartyId, string pnrLocator, string originCode, string transactionError) source) => new()
        {
            TravelPartyId = source.travelPartyId,
            RecordLocator = source.pnrLocator,
            OriginAirportCode = source.originCode,
            TransactionError = source.transactionError
        };

        public static async Task<TransactionSummaryRequest> CreateAsync(string checkInId, string transactionError) =>
            (TransactionSummaryRequest)(
                await Helper.CheckInIdHelper.GetTravelPartyId(checkInId),
                await Helper.CheckInIdHelper.GetPnrLocator(checkInId),
                await Helper.CheckInIdHelper.GetOriginCode(checkInId),
                transactionError);
    }
}
