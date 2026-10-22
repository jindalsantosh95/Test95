namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class OfferFilterCriteria
    {
        public bool DirectFlight { get; set; }
        public bool DirectWithStop { get; set; }
        public bool OneStop { get; set; }
        public bool TwoStops { get; set; }
        public bool LongLayover { get; set; }
        public bool VSFlightRandomOffer { get; set; }
        public bool AllOffersWithoutFilter { get; set; }
        public int LongLayoverDuration { get; set; }

        public OfferFilterCriteria()
        {
            DirectFlight = false;
            DirectWithStop = false;
            OneStop = false;
            TwoStops = false;
            LongLayover = false;
            VSFlightRandomOffer = false;
            AllOffersWithoutFilter = false;
            LongLayoverDuration = 8;
        }
    }
}
