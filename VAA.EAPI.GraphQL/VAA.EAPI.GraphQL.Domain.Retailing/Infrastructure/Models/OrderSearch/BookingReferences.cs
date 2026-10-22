using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.BookingReferences")]
    public class BookingReferences
    {
        public string AirlineDesignator { get; set; }
        public string RLOC { get; set; }
        public string RLOCCreateDate { get; set; }
        public string ExternalRecordLocator { get; set; }
    }
}