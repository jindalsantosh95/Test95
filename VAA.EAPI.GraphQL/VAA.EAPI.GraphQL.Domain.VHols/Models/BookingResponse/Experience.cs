using VAA.EAPI.GraphQL.Domain.VHols.Models.enums;

namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Experience
    {
        public string Id { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public List<Alternative> Alternatives { get; set; }
        public string Destination { get; set; }
        public ProductContent Content { get; set; }
        public ProductType? ProductType { get; set; }
        public string SelectedAlternativeId { get; set; }
        public bool? HasQuestions { get; set; }
        public AvailabilityStatus? AvailabilityStatus { get; set; }
        public bool? OnRequest { get; set; }
    }
}

