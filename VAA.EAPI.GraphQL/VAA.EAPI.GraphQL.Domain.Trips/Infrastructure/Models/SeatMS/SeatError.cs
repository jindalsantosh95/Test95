using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    public class SeatError : Error
    {
        public string IsWarning { get; set; }
    }
}
