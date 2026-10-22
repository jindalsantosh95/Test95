
namespace VAA.EAPI.Microservices.Retail.Lopa.Domain.Lopa
{

    public class SeatMap
    {
        public string SeatOfferId { get; set; }
        public string ClassType { get; set; }
        public string Equipment { get; set; }
        public string EquipmentCode { get; set; }
        public List<string> Details { get; set; }
        public List<string> Configuration { get; set; }
        public List<Row> Rows { get; set; }

    }

}