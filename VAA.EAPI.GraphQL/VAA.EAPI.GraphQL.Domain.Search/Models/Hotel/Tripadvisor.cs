namespace VAA.EAPI.GraphQL.Domain.Search.Models.Hotel
{
    public class Tripadvisor
    {
        public int? NumReviews { get; set; }
        public string? RatingImageUrl { get; set; }
        [UseFiltering]
        public decimal RatingScore
        {
            get
            {
                return Helpers.TripAdvisor.ConvertToRating(RatingImageUrl ?? "");
            }
        }
    }

}
