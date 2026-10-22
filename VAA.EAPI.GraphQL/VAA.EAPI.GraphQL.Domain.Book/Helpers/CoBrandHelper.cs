using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Book.Helpers
{
    public class CoBrandHelper
    {
        public static bool IsMemberCoBrandEligible(Basket basket, CustomerDTO customer)
        {
            if (customer == null)
            {
                return basket.PointOfSale?.CountryCode == Constants.Application.USCountryCode && basket.Summary.PaymentAmount <= 20000 && basket.Summary.PaymentAmount > 0;
            }

            if (basket.CustomerProfile?.FirstName != null && customer.Customer.MemberTier != Constants.Application.VACTier && basket.PointOfSale?.CountryCode == Constants.Application.USCountryCode)
            {
                var address = customer.Customer.AddressDetails?.FirstOrDefault(x => x.PreferredFlag == "Y");
                if (address != null && !string.IsNullOrEmpty(address.CountryCode))
                {
                    return address.CountryCode == Constants.Application.USCountryCode && basket.Summary.PaymentAmount <= 20000 && basket.Summary.PaymentAmount > 0;
                }
                return address?.Country.Is(Constants.Application.USCountryName) ?? false && basket.Summary.PaymentAmount <= 20000 && basket.Summary.PaymentAmount > 0;
            }
            return false;
        }
    }
}