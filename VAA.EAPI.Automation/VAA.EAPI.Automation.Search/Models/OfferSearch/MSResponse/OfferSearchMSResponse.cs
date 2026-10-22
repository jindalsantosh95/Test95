namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class OfferSearchMSResponse
    {
        public Pos pos { get; set; }
        public int totalOffers { get; set; }
        public List<Offer> offers { get; set; }
        public DataList dataList { get; set; }
    }
}
