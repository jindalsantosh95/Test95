using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
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

        public static explicit operator AdvisoryNote(JToken note)
        {
            if (note is null) throw new System.ArgumentNullException(nameof(note));
            return new()
            {
                Code = note["code"]?.Value<string>(),
                Compulsory = note["compulsory"]?.Value<bool>(),
                Description = note["description"]?.Value<string>(),
                NoteText = note["note"]?.Value<string>(),
                Number = note["no"]?.Value<string>(),
                Priority = note["priority"]?.Value<string>(),
                Title = note["title"]?.Value<string>()
            };
        }
    }
}
