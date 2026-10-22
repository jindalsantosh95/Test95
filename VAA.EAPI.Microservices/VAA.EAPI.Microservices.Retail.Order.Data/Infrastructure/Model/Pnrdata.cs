namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Model
{
    public class Pnrdata
    {
        public enum Operations
        {
            OrderCreate,
            OrderChange,
            OrderCancel
        }

        public string Rloc { get; set; }
        public bool? IsRFC { get; set; }
        public Operations Operation { get; set; }
        public bool IsCancelDisruption { get; set; }
    }
}
