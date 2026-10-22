using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.PartnersRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class PartnersRetrievalQuery : IRequest<Result<Response<PartnersResponse>>>
    {
    }

    public class PartnersRetrievalResponseQueryHandler : IRequestHandler<PartnersRetrievalQuery, Result<Response<PartnersResponse>>>
    {
        private readonly ILogger<PartnersRetrievalResponseQueryHandler> _logger;
        private readonly IMediator _mediator;
        public PartnersRetrievalResponseQueryHandler(IMediator mediator, ILogger<PartnersRetrievalResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<PartnersResponse>>> Handle(PartnersRetrievalQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Partners Retrieval Query handler process started");

            try
            {
                var partnerResponse = await _mediator.Send(new PartnersRetrieveCommand { });
                return partnerResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in Partners Retrieval Query handler");
                return Result.Failure<Response<PartnersResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation("Partners Retrieval Query handler process ended");
            }
        }
    }
}
