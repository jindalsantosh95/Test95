using MediatR;
using VAA.EAPI.Microservices.CRM.Domain;

namespace VAA.EAPI.Microservices.CRM.Infrastructure.Repository
{
    public class RetrieveCountryDetailsRepoQuery : IRequest<CountryDetails>
    {
        public string CountryName { get; set; }

        public RetrieveCountryDetailsRepoQuery(string countryName)
        {
            CountryName = countryName;
        }
    }
}
