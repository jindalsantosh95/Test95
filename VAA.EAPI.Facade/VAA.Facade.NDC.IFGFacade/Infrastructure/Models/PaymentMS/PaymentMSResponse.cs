using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Domain;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS
{
    public class PaymentMSResponse
    {
        public PaymentResponse PaymentDocument { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
