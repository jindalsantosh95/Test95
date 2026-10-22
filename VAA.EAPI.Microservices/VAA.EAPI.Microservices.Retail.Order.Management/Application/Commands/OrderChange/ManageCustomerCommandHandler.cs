using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class ManageCustomerCommandHandler : IRequestHandler<ManageCustomerCommand, Result<Response<string>>>
    {
        private readonly ILogger<ManageCustomerCommandHandler> _logger;
        private readonly IMediator _mediator;

        public ManageCustomerCommandHandler(ILogger<ManageCustomerCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<string>>> Handle(ManageCustomerCommand request, CancellationToken cancellationToken)
        {
            var id = string.Empty;

            _logger.LogInformation($"ManageCustomer command handler-Start");
            try
            {
                var response = ValidateRequest(request);

                if (response.Status == Status.OK)
                {
                    var cloudEvent = ManageCustomerCommandToEventMapper.CommandToEventMapper(request);

                    id = cloudEvent.Id;

                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    _logger.LogDebug($"Store event in event store - End");
                    try
                    {
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent, cancellationToken);
                        _logger.LogDebug($"Publish event - End");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Error while publishing event:{ex}");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = OrderEventType.ManageCustomerInitiate.ToString(), Data = request, Subject = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                        throw;
                    }

                    var manageCustomerResponse = ManageCustomerEventCommandResponseMapper.ManageCustomerResponseMapper(cloudEvent.Id);
                    return Result.Success(manageCustomerResponse);
                }
                else
                    return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in manage customer command handler for CorrelationId: {AsyncContext.Current.CorrelationId}");
                return Result.Failure<Response<string>>(ex.Message);
            }
            finally
            {
                _logger.LogInformation($"ManageCustomer command handler-End");
            }
        }

        public Response<string> ValidateRequest(ManageCustomerCommand request)
        {
            var response = new Response<string>();

            if (string.IsNullOrEmpty(request.Rloc) || request.Rloc.Trim().Length != Constants.Application.PNRLength || request.Rloc.Any(x => !char.IsLetterOrDigit(x)))
            {
                return response = new()
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidAttributeLenght), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }

            if (request.CustomerDetails.Any(x => string.IsNullOrEmpty(x.Customer.DateOfBirth) || !DateTime.TryParseExact(x.Customer.DateOfBirth, Constants.Application.ManageCustomerDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)))
            {
                return response = new()
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidDOB), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }

            if (request.CustomerDetails.Any(x => string.IsNullOrEmpty(x.Customer.Gender) || !ConfigReader.Get(Constants.Environment.GenderList).Split(",").Contains(x.Customer.Gender)))
            {
                return response = new()
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidGender), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }

            return response = new()
            {
                Status = Status.OK
            };
        }
    }

}
