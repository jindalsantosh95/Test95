using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<Detail> Details { get; set; }
        public InnerError InnerError { get; set; }
    }
}
