namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class FreeBaggageAllowance
    {
        public int AllowedBagCnt { get; set; }
        public Weight? Weight { get; set; }
        public DimensionAllowance? DimensionAllowance { get; set; }
    }
}
