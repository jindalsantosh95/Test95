using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper;


namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories
{
    public class PNRRetrieveQueryHandler : IRequestHandler<PNRRetrieveQuery, Result<Response<RetrieveResponseType>>>
    {
        private readonly IMediator _mediator;
        

        public PNRRetrieveQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<RetrieveResponseType>>> Handle(PNRRetrieveQuery requestData, CancellationToken cancellationToken)
        {
            if (requestData.PNR is null) throw new ArgumentNullException(nameof(requestData.PNR));

            var serviceResponse = await _mediator.Send(new PNRRetrieveRepoQuery(PNRRetrieveMapper.PNRRetrieveRequestMapper(requestData.PNR)), cancellationToken);
           
            return new Response<RetrieveResponseType>
            {
                Status = Status.OK,
                Value = serviceResponse
            };
        }


    }
}
