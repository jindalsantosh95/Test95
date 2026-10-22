using VAA.EAPI.Microservices.CRM.Domain.Enums;

namespace VAA.EAPI.Microservices.CRM.Domain
{
    public class PaymentResponse
    {
        public StatusEnum Status { get; set; }
        public string CorrelationId { get; set; }

    }

}
