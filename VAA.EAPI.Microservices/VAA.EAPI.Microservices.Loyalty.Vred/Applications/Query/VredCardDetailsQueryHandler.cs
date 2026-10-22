using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query
{
    public class VredCardDetailsQueryHandler : IRequestHandler<VredCardDetailsQuery, Result<Response<VredCardDetailsResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VredCardDetailsQueryHandler> _logger;

        public VredCardDetailsQueryHandler(IMediator mediator, ILogger<VredCardDetailsQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<VredCardDetailsResponse>>> Handle(VredCardDetailsQuery request, CancellationToken cancellationToken)
        {
            if (request.MemberNumber.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(request.MemberNumber));

            try
            {
                return Result.Success
                  (
                      new Response<VredCardDetailsResponse>
                      {
                          Value = await _mediator.Send(new VredCardDetailsRepoQuery(request.MemberNumber)),
                          Status = Status.OK
                      }
                  );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Exception occurred while fetching Vred Card Details:  {ex.Message} {ex?.InnerException}");
                return Result.Failure<Response<VredCardDetailsResponse>>(Convert.ToString(ex.Message));
            }
        }
    }
}

