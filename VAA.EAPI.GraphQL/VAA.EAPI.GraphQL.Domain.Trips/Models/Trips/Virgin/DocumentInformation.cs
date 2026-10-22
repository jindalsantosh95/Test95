using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class DocumentInformation
    {
        public LocalDateTime? TicketsDocumentAvailableTime { get; set; }

        public static explicit operator DocumentInformation?(Infrastructure.Models.HolidaysMS.TravelDatesInfo source)
        {
            if (source == null) return null;

            return new DocumentInformation()
            {
                TicketsDocumentAvailableTime = source.StartDate.HasValue ? LocalDateTime.FromDateTime(source.StartDate.Value.AddDays(-14)) : null
            };
        }
    }
}
