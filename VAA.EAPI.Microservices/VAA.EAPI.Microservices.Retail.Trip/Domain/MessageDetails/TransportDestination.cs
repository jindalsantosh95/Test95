
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class TransportDestination
    {
        public string ScheduledArrivalLocalDateTime { get; set; }
        public string EstimatedArrivalLocalDateTime { get; set; }
        public Station Station { get; set; }

        public static explicit operator TransportDestination(Infrastructure.Proxy.DeltaMessageCentre.TransportDestination source)
        {
            if (source == null)
                return null;

            return new TransportDestination()
            {
                EstimatedArrivalLocalDateTime = source.EstimatedArrivalLocalDateTime,
                ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
                Station = (Station)source.Station
            };
        }
    }
}
