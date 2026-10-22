using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;


namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS
{
    public class ValidationRS
    {
        public AgencyValidateResponse ValidateResponse { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}
