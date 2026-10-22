using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.OfferSearch
{
    public class OfferSearchQueryHandler : IRequestHandler<OfferSearchQuery, Result<Response<OfferResponse>>>
    {
        private readonly IMediator _mediator;

        public OfferSearchQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<Result<Response<OfferResponse>>> Handle(OfferSearchQuery request, CancellationToken cancellationToken)
        {
            var reshopResponse = await _mediator.Send(new OfferSearchRepoQuery(request.OfferID), cancellationToken);

            if (reshopResponse.Offers == null)
            {
                return Result.Success
                    (
                        new Response<OfferResponse>
                        {
                            Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidOfferRefId, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        }
                    );
            }

            return Result.Success(new Response<OfferResponse>
            {
                Status = Status.OK,
                Value = reshopResponse
            });
        }
    }
}
