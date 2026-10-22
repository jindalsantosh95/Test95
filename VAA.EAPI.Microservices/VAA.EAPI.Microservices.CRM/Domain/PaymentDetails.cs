using System.Collections.Generic;
using VAA.EAPI.Microservices.CRM.Domain.CRMSchema;

namespace VAA.EAPI.Microservices.CRM.Domain
{
    public class PaymentDetails
    {
        public List<PaymentEntity> Value { get; set; }
    }
}

