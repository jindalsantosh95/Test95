using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class AirTicketRQ
    {
        public string Version { get; set; }
        public DesignatePrinter DesignatePrinter { get; set; }
        public Itinerary Itinerary { get; set; }
        public IEnumerable<AirTicketing> Ticketing { get; set; }
        public PricingQualifiers PricingQualifiers { get; set; }
        public PostProcessing PostProcessing { get; set; }
    }
}
