using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.CustomerDetail;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.ManageAccount
{
    public class UpdateVredCustomerDetailsResponseQuery : UpdateVredCustomerDetailsQueryResponse
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
