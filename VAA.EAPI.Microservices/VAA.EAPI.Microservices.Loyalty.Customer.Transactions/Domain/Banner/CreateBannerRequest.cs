using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner
{
    public class CreateBannerRequest
    {
        public string CustomerID { get; set; }
        public string FcNumber { get; set; }
        public IEnumerable<Banners> Banners { get; set; }
    }
}
