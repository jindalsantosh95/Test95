using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:ActualCabinClass")]
    public class ActualCabinClass
    {
        public string Code { get; set; }
        public string ProductCode { get; set; }
        public string SeatAssignmentType { get; set; }
        public string SeatStatus { get; set; }

        public static explicit operator ActualCabinClass(Infrastructure.Models.CheckInMS.ActualCabinClass source) => new()
        {
            Code = source.Code,
            ProductCode = source.ProductCode,
            SeatAssignmentType = source.SeatAssignmentType,
            SeatStatus = source.SeatStatus
        };
    }
}