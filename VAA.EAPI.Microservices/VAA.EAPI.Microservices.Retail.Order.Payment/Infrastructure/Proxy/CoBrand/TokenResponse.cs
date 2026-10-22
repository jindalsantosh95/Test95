using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class TokenResponse
    {
        public string Token_type { get; set; }
        public string Issued_at { get; set; }
        public string Client_id { get; set; }
        public string Access_token { get; set; }
        public string Application_name { get; set; }
        public string Expires_in { get; set; }
        public string Status { get; set; }
    }
}
