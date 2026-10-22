using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Application.Query.VSRetro
{
    public class VSRetroFlownQueryHandler : IRequestHandler<VSRetroFlownQuery, Result<Response<VSFlownDetailsResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VSRetroFlownQueryHandler> _logger;
        private readonly IVSRetroFlownDBRepository _flownDBRepository;


        public VSRetroFlownQueryHandler(IMediator mediator, ILogger<VSRetroFlownQueryHandler> logger, IVSRetroFlownDBRepository flownDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _flownDBRepository = flownDBRepository ?? throw new ArgumentNullException(nameof(flownDBRepository)); ;
        }

        public async Task<Result<Response<VSFlownDetailsResponse>>> Handle(VSRetroFlownQuery request, CancellationToken cancellationToken)
        {
            if (request.TicketNumber.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(request.TicketNumber));

            try
            {
                var response = await _flownDBRepository.GetVSFlownDetailsByTicketNumber(request.TicketNumber);
                
                if (!response.VSRetroFlownDetails.Any())
                    return new Response<VSFlownDetailsResponse>(){Status = Status.BusinessException,Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.TicketNumberValidationError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)};

                return Result.Success
                 (
                    new Response<VSFlownDetailsResponse>
                    {
                        Value =  response,
                        Status = Status.OK
                    }
                 );
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while fetching VS Retro Flown Details:  {ex?.Message} {ex?.InnerException}");

                return new Response<VSFlownDetailsResponse>() { Status = Status.BusinessException, Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.TicketNumberValidationError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };
            }
        }
    }
}
