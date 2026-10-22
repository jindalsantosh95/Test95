using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query
{
    public class VredStatementsQueryHandler : IRequestHandler<VredStatementsQuery, Result<Response<VredStatementResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VredStatementsQueryHandler> _logger;
        public VredStatementsQueryHandler(IMediator mediator, ILogger<VredStatementsQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<VredStatementResponse>>> Handle(VredStatementsQuery request, CancellationToken cancellationToken)
        {
            if (request.MemberNumber.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(request.MemberNumber));

            try
            {
                return Result.Success
                  (
                      new Response<VredStatementResponse>
                      {
                          Value = await _mediator.Send(new VredStatementsRepoQuery(request.MemberNumber)),
                          Status = Status.OK
                      }
                  );
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"Exception occurred while fetching Vred statements:  {ex.Message} {ex?.InnerException}");
                return Result.Failure<Response<VredStatementResponse>>(Convert.ToString(ex.Message));
            }
        }
    }
}
