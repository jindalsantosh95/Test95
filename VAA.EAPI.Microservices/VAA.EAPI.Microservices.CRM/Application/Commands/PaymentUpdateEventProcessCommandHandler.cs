using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.CRM.Domain;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;
using VAA.EAPI.Microservices.CRM.Mapper;

namespace VAA.EAPI.Microservices.CRM.Application.Commands
{
    public class PaymentUpdateEventProcessCommandHandler : IRequestHandler<PaymentUpdateEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentUpdateEventProcessCommandHandler> _logger;
        public PaymentUpdateEventProcessCommandHandler(IMediator mediator, ILogger<PaymentUpdateEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Unit>> Handle(PaymentUpdateEventProcessCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"PaymentUpdateEventProcessCommand Handler | CorreltionId {request.Id}");
            var paymentReqeust = Json.Deserialize<PaymentRequest>(Convert.ToString(request.Data));
            try
            {
                //Repository call for get Payment Details from CRM 
                var paymentDetails = await _mediator.Send(new RetrievePaymentDetailsRepoQuery(paymentReqeust.PaymentNumber), cancellationToken);
                var customerCountryid = string.Empty;
                string paymentId = paymentDetails.Value?.FirstOrDefault()?.VaaPaymentid;

                if (paymentId == null)
                    throw new DomainException(VAA.EAPI.Microservices.CRM.Constants.ErrorCodes.InvalidPaymentNumber);

                //Repository calll for Get Country Details from CRM
                var countryDetails = await _mediator.Send(new RetrieveCountryDetailsRepoQuery(paymentReqeust.BankCountryName), cancellationToken);
                var countryId = countryDetails.Value?.FirstOrDefault()?.VaaCountryid;

                if (countryId == null)
                    throw new DomainException(VAA.EAPI.Microservices.CRM.Constants.ErrorCodes.InvalidBankCountryName);

                if (!paymentReqeust.Country.IsNullOrWhiteSpace())
                {
                    var customerCountryDetails = await _mediator.Send(new RetrieveCountryDetailsRepoQuery(paymentReqeust.Country), cancellationToken);
                    var customerCountryDetailsCountryId = customerCountryDetails.Value?.FirstOrDefault()?.VaaCountryid;
                    if (customerCountryDetailsCountryId == null)
                        throw new DomainException(VAA.EAPI.Microservices.CRM.Constants.ErrorCodes.InvalidCustomerCountryName);
                }

                //Repository call for updating Payment details in CRM
                var updatePaymentDetailsRepoCommand = UpdatePaymentDetailsRepoCommandMapper.MaptoUpdateReqeustCommand(paymentReqeust, paymentId, countryId, customerCountryid);
                var paymentUpdateResponse = await _mediator.Send(updatePaymentDetailsRepoCommand, cancellationToken);
                var response = UpdatePaymentDetailsRepoCommandMapper.MaptoUpdatePaymentResponse(paymentUpdateResponse, request.Id);
                if (response.Status == VAA.EAPI.Microservices.CRM.Domain.Enums.StatusEnum.Failure)
                    throw new DomainException(VAA.EAPI.Microservices.CRM.Constants.ErrorCodes.PaymentUpdateError);

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = request.Subject }, cancellationToken);
            }
            catch (DomainException ex)
            {
                var errorResponse = Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(ex, request.Id, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));
                _logger.LogError(ex, $"PaymentUpdateEventProcessCommand Handler | CorreltionId : {request.Id} | Error Response : {errorResponse}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = errorResponse, Status = ServiceResponseProcessStatus.Error, Type = request.Subject }, cancellationToken);
            }
            catch (Exception ex)
            {
                var errorResponse = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscTechError, request.Id, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));
                _logger.LogError(ex, $"PaymentUpdateEventProcessCommand Handler | CorreltionId : {request.Id} | Error Message : {ex.Message}| Error Response : {errorResponse}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = errorResponse, Status = ServiceResponseProcessStatus.Error, Type = request.Subject }, cancellationToken);
            }
            return Unit.Value;
        }
    }
}