using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class ManageCustomerEventSubscribeCommandHandler : IRequestHandler<ManageCustomerEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageCustomerEventSubscribeCommandHandler> _logger;

        public ManageCustomerEventSubscribeCommandHandler(IMediator mediator, ILogger<ManageCustomerEventSubscribeCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(ManageCustomerEventProcessCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"ManageCustomerEventSubscriber command handler-Start for Id {request.Id}");
            try
            {
                var manageCustomerRequestData = Json.Deserialize<ManageCustomerRequest>(Convert.ToString(request.Data));
                List<Warning> warnings = new();
                List<string> exceptions = new();
                var pnrRetrieveResponse = await _mediator.Send(new PNRRetrieveHistoryRepoQuery(manageCustomerRequestData.Rloc), cancellationToken);
                if (manageCustomerRequestData.CustomerDetails.Count <= pnrRetrieveResponse.PnrType.Ptcs.Count)
                {
                    var customerDetails = ManageCustomerRQMapper.CustomerMapper(pnrRetrieveResponse, manageCustomerRequestData.CustomerDetails);
                    foreach (var customer in customerDetails)
                    {
                        var sessionId = await _mediator.Send(new PNRSetupContextRepoQuery { RecordLocator = manageCustomerRequestData.Rloc, AssignPrinter = false }, cancellationToken);
                        try
                        {
                            var passengerV4Response = await _mediator.Send(new PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery() { Customer = customer, SessionId = sessionId, RetrieveFlag = false }, cancellationToken);
                        }
                        catch (ExternalServiceIntegrationException ex)
                        {
                            exceptions.Add(ex.InnerException.Message);
                            if (ex.InnerException.Message.Contains(Constants.Application.InvalidDOBMessage))
                            {
                                warnings.Add(ManageCustomerRSMapper.WarningMapper(customer, Constants.Application.InvalidDOBMessage));
                            }
                            else if (ex.InnerException.Message.Contains(Constants.Application.InvalidGenderMessage))
                            {
                                warnings.Add(ManageCustomerRSMapper.WarningMapper(customer, Constants.Application.InvalidGenderMessage));
                            }
                            else
                            {
                                warnings.Add(ManageCustomerRSMapper.WarningMapper(customer, ex.InnerException.Message));
                            }
                        }
                    }
                    var result = ManageCustomerRSMapper.ResponseMapper(warnings, manageCustomerRequestData);
                    if (result == null)
                    {
                        List<ErrorDetails> errors = new();
                        exceptions.ForEach(x => errors.Add(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.PassengerV4ManageCustomerError, request.Id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, x).FirstOrDefault()));

                        await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(errors), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                    }
                    else
                    {
                        await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                    }
                    return Unit.Value;
                }
                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InValidPTC), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                    return Unit.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured in ManageCustomer Event Subscribe Command Handler || ErrorId : {request.Id}");
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                throw;
            }
            finally
            {
                _logger.LogInformation($"ManageCustomerEventSubscriber command handler-End");
            }
        }
    }
}
