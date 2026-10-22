using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayDocumentDetailsRequest")]
    public class HolidayDocumentDetailsRequest
    {
        public string TripResourceId { get; set; }   
    }
}