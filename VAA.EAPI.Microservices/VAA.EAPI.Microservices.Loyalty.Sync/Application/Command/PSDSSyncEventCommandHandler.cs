using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.DevTools;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Sync.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command
{
    public class PSDSSyncEventCommandHandler : IRequestHandler<PSDSSyncEventCommand, Result<bool>>
    {
        private readonly ILogger<PSDSSyncEventCommandHandler> _logger;
        private readonly IMediator _mediator;

        public PSDSSyncEventCommandHandler(ILogger<PSDSSyncEventCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<bool>> Handle(PSDSSyncEventCommand request, CancellationToken cancellationToken)
        {
            Result<Response<string>> result = null;
            bool returnResponse = false;

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var enrolmentChannelList = ConfigReader.Get(Constants.Environment.EnrolmentChannelList).Split("|");

                if (ConfigReader.Get<bool>(Constants.Environment.EnablePSDSSync) && enrolmentChannelList.Contains(request.Data.EnrollmentChannel))
                {
                    bool syncResponse = await _mediator.Send(new PSDSSyncRepoCommand() { UpdateSFPDRequest = CustomerRequestMapper.RequestMapper(request.Data) }, cancellationToken);

                    // Create response for Status database.
                    result = CSharpFunctionalExtensions.Result.Success(
                       new Response<string>()
                       {
                           Status = Status.OK,
                           Value = syncResponse.ToString()
                       }
                       );
                    returnResponse = true;
                }
                else
                {
                    result = CSharpFunctionalExtensions.Result.Failure<Response<string>>($"Enrolment Channel {request.Data.EnrollmentChannel} - PSDS Sync not required");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PSDS sync - Exception");
                result = CSharpFunctionalExtensions.Result.Failure<Response<string>>(ex.Message.ToString());
            }

            // Send outcome to LoyaltySyncEventResponse event store
            await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsSuccess ? ServiceResponseProcessStatus.Completed : ServiceResponseProcessStatus.Error, Type = Constants.Application.CommandResponseStatusKey });

            return returnResponse;

        }
    }
}
