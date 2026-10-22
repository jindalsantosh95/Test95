namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class TestConfig
    {
        public Search Search { get; set; }

        public Book Book { get; set; }

        public Internal Internal { get; set; }

        public SSRDetail SSRDetail { get; set; }

        public OfferFilterCriteria OfferFilterCriteria { get; set; }

        public TestConfig()
        {
            Search = new Search();
            Book = new Book();
            Internal = new Internal();
            SSRDetail = new SSRDetail();
            OfferFilterCriteria = new OfferFilterCriteria();
        }
    }
}