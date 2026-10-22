using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Reference.Domain.SalesBanner;

namespace VAA.EAPI.Microservices.Retail.Reference.Application
{
    public class SalesBannersCommand : IRequest<Dictionary<string, IEnumerable<SalesBanner>>>
    {
        public SalesBannerFilters Filters { get; set; }
    }
}
