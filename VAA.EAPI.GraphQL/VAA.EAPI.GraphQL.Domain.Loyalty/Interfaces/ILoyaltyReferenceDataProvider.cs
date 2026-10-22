using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;

public interface ILoyaltyReferenceDataProvider
{
    Task<IEnumerable<AirportInfo>> GetAirports();

    Task<IEnumerable<AirlineInfo>> GetAirlines();

    Task<IEnumerable<BlacklistedEmailList>> GetBlacklistedEmails();
}
