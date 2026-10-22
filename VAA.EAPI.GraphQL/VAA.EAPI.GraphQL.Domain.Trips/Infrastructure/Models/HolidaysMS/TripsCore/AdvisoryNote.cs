namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class AdvisoryNote
    {
        public string? Number { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? NoteText { get; set; }
        public string? Priority { get; set; }
        public string? Title { get; set; }
        public bool? Compulsory { get; set; }
    }
}