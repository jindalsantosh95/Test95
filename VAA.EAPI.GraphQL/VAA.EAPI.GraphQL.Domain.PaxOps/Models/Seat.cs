using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Seat")]
    public class Seat
    {
        public int Row { get; set; }
        public string Column { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<ActualCabinClass> ActualCabinClass { get; set; }

        public static explicit operator Seat(Infrastructure.Models.CheckInMS.Seat source) => new()
        {
            Row = source.Row,
            Column = source.Column,
            Name = source.Name,
            Type = source.Type,
            ActualCabinClass = source.ActualCabinClass?.Select(acc => (ActualCabinClass)acc)
        };
    }
}