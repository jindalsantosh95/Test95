using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCOrderStore
{
    public class OrderDocQueryHandler : IRequestHandler<OrderDocQuery, Response<OrderDocument>>
    {
        private readonly IMediator _mediator;

        public OrderDocQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Response<OrderDocument>> Handle(OrderDocQuery request, CancellationToken cancellationToken)
        {
            var orderdoc = await _mediator.Send(new OrderDocumentRepoQuery() {OrderId = request.OrderId }, cancellationToken);

            if(orderdoc.Any()) return new Response<OrderDocument>() { Status = Status.OK , Value = orderdoc.First() };

            else return new Response<OrderDocument>() 
            {
                Status = Status.BusinessException , 
                Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.OrderRefErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
            }; 
            
        }
    }
}

