
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{

    public class TransportOrigin
    {

        public string ScheduledDepartureLocalDateTime { get; set; }


        public string EstimatedDepartureLocalDateTime { get; set; }


        public Station Station { get; set; }

        public static explicit operator TransportOrigin(Infrastructure.Proxy.DeltaMessageCentre.TransportOrigin source)
        {
            if (source == null)
                return null;

            return new TransportOrigin()
            {
                EstimatedDepartureLocalDateTime = source.EstimatedDepartureLocalDateTime,
                ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
                Station = (Station)source.Station
            };
        }
    }
}
