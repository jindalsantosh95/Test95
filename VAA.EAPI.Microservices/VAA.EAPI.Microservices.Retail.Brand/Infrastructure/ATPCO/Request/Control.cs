using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO.Request
{
    public class Control
    {
        public IEnumerable<string> includes { get; set; }
        public bool include_rq { get; set; }
    }

}
