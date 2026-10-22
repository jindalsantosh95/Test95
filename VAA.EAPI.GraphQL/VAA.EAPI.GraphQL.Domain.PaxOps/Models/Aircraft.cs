using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Aircraft")]
    public class Aircraft
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SubfleetCode { get; set; }

        public static explicit operator Aircraft(Infrastructure.Models.CheckInMS.Aircraft source) => new()
        {
            Code = source.Code,
            Name = source.Name,
            SubfleetCode = source.SubfleetCode
        };
    }
}
