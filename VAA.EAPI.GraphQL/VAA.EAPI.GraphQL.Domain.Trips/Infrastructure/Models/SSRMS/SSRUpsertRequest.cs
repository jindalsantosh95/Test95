using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS
{
    public class SSRUpsertRequest
    {
        public string PassengerResourceId { get; set; }
        public IEnumerable<SsrSegment> SSRSegments { get; set; }
        public bool? ApplyToAll { get; set; }
        public SSRModifyActionType SSRModifyActionType { get; set; }

        public static explicit operator SSRUpsertRequest((IEnumerable<Passenger> eligibleSegments, bool applyToAll, Dictionary<string, string> action) source)
        {
            var (request, applyToAll, action) = source;

            return new()
            {
                PassengerResourceId = request.First().PassengerResourceId,
                ApplyToAll = applyToAll,
                SSRModifyActionType = (SSRModifyActionType)Enum.Parse(typeof(SSRModifyActionType), action["actionType"], true),
                SSRSegments = request.First().SsrSegments.Select(s => (SsrSegment)(s, action["ssrCode"]))
            };
        }
    }
}