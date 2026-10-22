using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Insurance.Domain;


namespace VAA.EAPI.Microservices.Retail.Insurance.Application.Commands
{
    public class CreateQuoteCommand : IRequest<Result<Response<InsuranceQuoteResponse>>>
    {
        public InsuranceQuoteRequest Request { get; set; }
        
    }
}
