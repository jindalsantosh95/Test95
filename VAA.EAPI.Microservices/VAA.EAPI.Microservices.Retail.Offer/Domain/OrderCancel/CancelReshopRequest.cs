using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel
{
    public class CancelReshopRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public ReshopActions ReshopActions { get; set; }

    }
}