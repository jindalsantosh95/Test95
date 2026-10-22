namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class PaidBag : Bag
    {
        public int Excess_First { get; set; }
        public int Excess_Last { get; set; }
        public Fee Fee { get; set; }
        public int? Free { get; set; }
        public string Os_Sequence { get; set; }
    }
}