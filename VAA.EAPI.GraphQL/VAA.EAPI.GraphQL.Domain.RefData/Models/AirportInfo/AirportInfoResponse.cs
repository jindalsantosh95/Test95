using HotChocolate.Data;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.RefData.Models.AirportInfo
{
    public class AirportInfoResponse
    {
        public string SearchTerm { get; set; }
        public int Count { get; set; }
        [UseSorting]
        [UseFiltering]
        public IEnumerable<AirportInfo> ListOfCities { get; set; } = new List<AirportInfo>();
    }
}