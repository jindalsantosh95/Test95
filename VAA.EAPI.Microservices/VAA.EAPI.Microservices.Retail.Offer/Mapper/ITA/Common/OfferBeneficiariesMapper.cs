using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common
{
    public class OfferBeneficiariesMapper
    {
        public static IEnumerable<string> GetServiceBeneficiaries(IEnumerable<Customer> customerDetail, string ptc)
        {
            return customerDetail.Where(c => c.Ptc.Equals(ptc)).Select(c => c.CustId);
        }
    }
}
