namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class OfferSearchBFFRequest
    {
        public List<Slice> slices { get; set; }

        public bool awardSearch { get; set; }

        public Ids ids { get; set; }

        public List<string> passengers { get; set; }

        public List<object> cabinClasses { get; set; }
    }
}
