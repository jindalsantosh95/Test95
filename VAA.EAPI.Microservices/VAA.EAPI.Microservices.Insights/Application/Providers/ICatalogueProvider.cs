using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{
    public interface ICatalogueProvider
    {
        Task<ICollection<Endpoint>> FetchCatalogueAsync(Guid subscriptionId, string apiVersion, string environment);
    }
}
