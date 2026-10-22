using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.CancelReshopOfferSearch
{
    public class CancelReshopOfferSearchQueryHandler : IRequestHandler<CancelReshopOfferSearchQuery, Result<Response<CancelReshopResponse>>>
    {
        private readonly IMediator _mediator;

        public CancelReshopOfferSearchQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
       
        public async Task<Result<Response<CancelReshopResponse>>> Handle(CancelReshopOfferSearchQuery request, CancellationToken cancellationToken)
        {
            var cancelReshopResponse = await _mediator.Send(new CancelReshopOfferSearchRepoQuery(request.OfferID), cancellationToken);

            if (cancelReshopResponse.Offers == null)
            {
                return Result.Success
                    (
                        new Response<CancelReshopResponse>
                        {
                            Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidOfferRefId, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        }
                    );
            }

            return Result.Success(new Response<CancelReshopResponse>
            {
                Status = Status.OK,
                Value = cancelReshopResponse
            });
        }
    }
}
