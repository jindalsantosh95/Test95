using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Insurance.Domain;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Services.OrderCreate
{
    public class GetQuoteServieCommand : IRequest<InsuranceQuoteResponse>
    {
        public InsuranceQuoteRequest Request { get; set; }
    }
}
