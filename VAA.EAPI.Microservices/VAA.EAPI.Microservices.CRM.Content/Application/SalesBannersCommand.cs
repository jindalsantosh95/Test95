using VAA.EAPI.Microservices.CRM.Content.Domain.SalesBanner;

namespace VAA.EAPI.Microservices.CRM.Content.Application
{
    public class SalesBannersCommand
    {
        public static explicit operator SalesBannersCommand(SalesBannerFilters source) => new() { Filters = source };

        public SalesBannerFilters Filters { get; set; }
    }
}
