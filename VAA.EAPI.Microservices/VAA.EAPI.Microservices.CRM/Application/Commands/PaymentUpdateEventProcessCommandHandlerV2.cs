using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    public class PaymentUpdateEventProcessCommandHandlerV2 : IRequestHandler<PaymentUpdateEventProcessCommandV2, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentUpdateEventProcessCommandHandlerV2> _logger;
        public PaymentUpdateEventProcessCommandHandlerV2(IMediator mediator, ILogger<PaymentUpdateEventProcessCommandHandlerV2> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Unit>> Handle(PaymentUpdateEventProcessCommandV2 request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"PaymentUpdateEventProcessCommand Handler | CorreltionId {request.Id}");
            var paymentReqeust = Json.Deserialize<PaymentRequest>(Convert.ToString(request.Data));
            try
            {
                //Validate Payment Number Name and Duplicate Payment Update
                var errorList = await ValidatePaymentNumberandDeDup(paymentReqeust, request.Id, cancellationToken);

                if (errorList.Count > 0)
                    throw new DomainException(errorList.First().Code);

                //Validate Country Name
                errorList = await ValidateCountryName(paymentReqeust, request.Id, cancellationToken);

                if(!paymentReqeust.Country.IsNullOrWhiteSpace())
                {
                    errorList = await ValidateUserCountryName(paymentReqeust, request.Id, cancellationToken);
                }

                if (errorList.Count > 0)
                    throw new DomainException(errorList.First().Code);

                var updatePaymentDetailsRepoCommand = UpdatePaymentDetailsRepoCommandMapperV2.MaptoUpdateReqeustCommand(paymentReqeust);
                var paymentUpdateResponse = await _mediator.Send(updatePaymentDetailsRepoCommand, cancellationToken);
                var response = UpdatePaymentDetailsRepoCommandMapperV2.MaptoUpdatePaymentResponse(paymentUpdateResponse, request.Id);

                if (response.Status == CRM.Domain.Enums.StatusEnum.Failure)
                    throw new DomainException(VAA.EAPI.Microservices.CRM.Constants.ErrorCodes.PaymentUpdateError);

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = request.Subject }, cancellationToken);
            }
            catch (DomainException ex)
            {
                var errorResponse = Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(ex, request.Id, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));

                var message = $"PaymentUpdateEventProcessCommand Handler | CorreltionId : {request.Id} | Error Response : {errorResponse}";

                if (ex.Message.Is(Constants.ErrorCodes.DuplicateBankDetails))
                    _logger.LogWarning(message);
                else
                {
                    _logger.LogError(ex, message);

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = errorResponse, Status = ServiceResponseProcessStatus.Error, Type = request.Subject }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscTechError, request.Id, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));

                _logger.LogError(ex, $"PaymentUpdateEventProcessCommand Handler | CorreltionId : {request.Id} | Error Message : {ex.Message}| Error Response : {errorResponse}");

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = errorResponse, Status = ServiceResponseProcessStatus.Error, Type = request.Subject }, cancellationToken);
            }

            return Unit.Value;
        }

        public async Task<List<ErrorDetails>> ValidatePaymentNumberandDeDup(PaymentRequest request, string coorelationId, CancellationToken cancellationToken)
        {
            List<ErrorDetails> errorList = new();
            var paymentDetails = (await _mediator.Send(new RetrievePaymentDetailsRepoQuery(request.PaymentNumber), cancellationToken)).Value?.FirstOrDefault();

            if (string.IsNullOrEmpty(paymentDetails?.VaaPaymentid))//Validate PaymentNumber
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidPaymentNumber), coorelationId, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
            else if (paymentDetails.VaaBankaccountdetailsentered || paymentDetails.VaaPaymentStage == Constants.Application.SentForPaymentStage) //Validate Duplicate paymentUpdate
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.DuplicateBankDetails), coorelationId, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
            else
                request.PaymentId = paymentDetails.VaaPaymentid;

            return errorList;
        }

        private async Task<List<ErrorDetails>> ValidateCountryName(PaymentRequest request, string coorelationId, CancellationToken cancellationToken)
        {
            List<ErrorDetails> errorList = new();
            var countryDetails = (await _mediator.Send(new RetrieveCountryDetailsRepoQuery(request.BankCountryName), cancellationToken)).Value?.FirstOrDefault();

            if (string.IsNullOrEmpty(countryDetails?.VaaCountryid))//Validate PaymentNumber
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidBankCountryName.ToString()), coorelationId, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
            else
                request.CountryId = countryDetails.VaaCountryid;

            return errorList;
        }


        private async Task<List<ErrorDetails>> ValidateUserCountryName(PaymentRequest request, string coorelationId, CancellationToken cancellationToken)
        {
            List<ErrorDetails> errorList = new();
            var countryDetails = (await _mediator.Send(new RetrieveCountryDetailsRepoQuery(request.Country), cancellationToken)).Value?.FirstOrDefault();

            if (string.IsNullOrEmpty(countryDetails?.VaaCountryid))//Validate PaymentNumber
                errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerCountryName.ToString()), coorelationId, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
            else
                request.CustomerCountryId = countryDetails.VaaCountryid;

            return errorList;
        }
    }
}