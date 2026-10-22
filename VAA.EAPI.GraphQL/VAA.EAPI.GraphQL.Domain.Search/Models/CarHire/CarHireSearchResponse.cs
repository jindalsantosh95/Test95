namespace VAA.EAPI.GraphQL.Domain.Search.Models.CarHire
{
    public class CarHireSearchResponse
    {
        [UseFiltering]
        [UseSorting]
        public IEnumerable<Car_CJS> Cars { get; set; } = Enumerable.Empty<Car_CJS>();
        public IEnumerable<DateWidenedCarHire> DateWidened { get; set; } = Enumerable.Empty<DateWidenedCarHire>();
        public RulesContent? RulesContent { get; set; }
    }

}
