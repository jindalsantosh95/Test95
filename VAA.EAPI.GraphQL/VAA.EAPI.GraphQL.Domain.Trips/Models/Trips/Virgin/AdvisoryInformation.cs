namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class AdvisoryInformation
    {
        public string? Priority { get; set; }
        public string? InformationText { get; set; }

        private static string? NormalizeInformationText(string? informationText)
        {
            if (informationText == null)
                return null;

            var normalized = string.Join(' ', informationText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return normalized;
        }

        public static explicit operator AdvisoryInformation(Infrastructure.Models.HolidaysMS.TripsCore.AdvisoryNote source)
        {
            if (source == null) return null;

            return new()
            {
                InformationText = NormalizeInformationText(source.NoteText),
                Priority = source.Priority
            };
        }
    }
}

