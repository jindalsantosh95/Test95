using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;
using Status = VAA.EAPI.Domain.Entities.Common.Status;


namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class CapturePaymentCommandHandler : IRequestHandler<CapturePaymentCommand, Result<VAA.EAPI.Domain.Entities.Common.Response<Domain.CPD.PaymentResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ILogger<CapturePaymentCommandHandler> _logger;
        private readonly IAzureBlobService _azureBlobService;

        public CapturePaymentCommandHandler(IMediator mediator, ILogger<CapturePaymentCommandHandler> logger, IHttpConnector httpConnector, IAzureBlobService azureBlobService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<Result<VAA.EAPI.Domain.Entities.Common.Response<Domain.CPD.PaymentResponse>>> Handle(CapturePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var countryList = await _azureBlobService.BlobJsonAsAsync<IEnumerable<CountryId>>(ConfigReader.Get(Constants.Environment.CpdCountryBlob), cancellationToken);

                var captureApiRequest = CapturePaymentMapper.CapturePaymentRequest(request.CapturePaymentRequest, countryList);

                var captureXmlDocument = new XmlDocument();
                captureXmlDocument.LoadXml(Xml.Serialize(captureApiRequest));

                var capturePaymentApiResponse = await _mediator.Send(new Infrastructure.Services.CPD.CapturePaymentCommand() { CapturePaymentXMLRequest = captureXmlDocument?.InnerXml }, cancellationToken);

                bool captureStatus = capturePaymentApiResponse.Value.Value.Status.First().Code == "100" ? true : false;
                bool transactionStatus = false;

                var transactionStatusApiRequest = Infrastructure.Mapper.CPD.GetTransactionStatusMapper.GetTransactionStatusRequest(request.CapturePaymentRequest.PaymentDetails.First().PaymentTransactionId);
                var transactionStatusRQ = Xml.Serialize(new Infrastructure.Models.CPD.GetTransactionStatusRequest() { GetTransactionStatus = transactionStatusApiRequest });
                var transactionStatusXmlDocument = new XmlDocument();
                transactionStatusXmlDocument.LoadXml(transactionStatusRQ);
                await Task.Delay(ConfigReader.Try(Constants.Environment.GetTransactionStatusWaitTime, 100), cancellationToken);

                var transactionStatusResponse = await _mediator.Send(new Infrastructure.Services.CPD.GetTransactionStatusQuery() { TransactionStatusXmlRequest = transactionStatusXmlDocument?.InnerXml }, cancellationToken);

                if (capturePaymentApiResponse.Value.Status == Status.OK && !Constants.Application.BatchWiseCardType.Any(type => type == request.CapturePaymentRequest.PaymentDetails.First().CardMethod.CardType.ToString()))
                {
                    transactionStatus = await CaptureResponse(transactionStatusXmlDocument, transactionStatusResponse, cancellationToken);

                    capturePaymentApiResponse.Value.Value.Status.First().Text = transactionStatus ? "Payment Captured" : "Capture Declined";
                    capturePaymentApiResponse.Value.Value.Status.First().Code = transactionStatus ? "100" : Constants.Application.CaptureFailureCode;
                    captureStatus = transactionStatus;
                }

                var paymentDoc = await _mediator.Send(new PaymentDocumentReaderQuery() { TransactionId = request.CapturePaymentRequest.PaymentDetails.First().PaymentTransactionId, SqlQuery = Constants.Application.PaymentQuery }, cancellationToken);

                if (paymentDoc != null)
                {
                    PaymentDocumentMapper.PaymentDocumentUpdateMapper(request.CapturePaymentRequest, paymentDoc, captureStatus, transactionStatusResponse.Value.Value);
                    await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = paymentDoc, ModificationType = CosmosModificationType.Upsert }), cancellationToken);
                    
                    var ntdhRequest = NtdhMapper.RequestMapper(paymentDoc);
                    var ntdhCloudEvent = NtdhMapper.EventMapper(ntdhRequest);

                    await _mediator.Send(new EventRequestInsertCommand { Id = ntdhCloudEvent.Id, Type = ntdhCloudEvent.Type, Data = ntdhCloudEvent.Data, Subject = ntdhCloudEvent.Subject }, cancellationToken);

                    try
                    {
                        await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = ntdhCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.NtdhTopicName }, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await _mediator.Send(new ServiceResponseInsertCommand { Id = ntdhCloudEvent.Id, Status = ServiceResponseProcessStatus.Error, Type = ntdhCloudEvent.Type }, cancellationToken);
                        throw new Exception($"Error occurred in CapturePaymentCommandHandler while publishing message to service bus : {ex.Message}");
                    }

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = ntdhCloudEvent.Id, Status = ServiceResponseProcessStatus.Completed, Type = ntdhCloudEvent.Type }, cancellationToken);

                }

                var response = new Domain.CPD.PaymentResponse();
                response = PaymentResponseMapper.CapturePaymentResponseMapper(captureApiRequest, capturePaymentApiResponse.Value.Value, transactionStatusResponse.Value.Value, paymentDoc);

                if (response.Errors == null)
                    return new VAA.EAPI.Domain.Entities.Common.Response<Domain.CPD.PaymentResponse>() { Status = Status.OK, Value = response };
                else
                {
                    return new VAA.EAPI.Domain.Entities.Common.Response<Domain.CPD.PaymentResponse>() { Status = Status.Error, Errors = response.Errors.ToList() };
                }
            }
            catch (CosmosDBException ex)
            {
                _logger.LogError(ex, $"Error occurred in CPD Capture PaymentMS call while updating Payment Document - {ex.Message}");
                return Result.Failure<VAA.EAPI.Domain.Entities.Common.Response<Domain.CPD.PaymentResponse>>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in CPD Capture PaymentMS call - {ex.Message}");
                var errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.DefaultTechErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                return Result.Failure<Response<Domain.CPD.PaymentResponse>>(Json.Serialize(errors));
            }

        }

        private async Task<bool> CaptureResponse(XmlDocument transactionStatusXmlDocument, Result<Response<Infrastructure.Models.CPD.GetTransactionStatusResponse>> transactionStatusResponse, CancellationToken cancellationToken)
        {
            var transactionStatus = false;
            int retryCount = Int32.Parse(ConfigReader.Get(Constants.Environment.RetryCount));
            int i = 1;

            while(i <= retryCount)
            {
                if (transactionStatus = transactionStatusResponse.Value.Status == Status.OK)
                {
                    var transactionResponse = transactionStatusResponse.Value.Value.Transaction.Status.StatusMessage.OrderByDescending(a => int.Parse(a.Postion)).ToList();

                    if(transactionResponse.First().Id == Constants.Application.CallbackAcceptedCode && transactionResponse[1].Id == Constants.Application.CaptureSuccessCode)
                    {
                        return true;
                    }
                    else if (transactionStatusResponse.Value.Value.Transaction.Status.StatusMessage.Any(status => status.Id == Constants.Application.CaptureFailureCode))
                    {
                        return false;
                    }
                    else
                    {
                        // wait and retry getTransactionStatus call
                        await Task.Delay(ConfigReader.Try(Constants.Environment.GetTransactionStatusWaitTime, 100), cancellationToken);
                        transactionStatusResponse = await _mediator.Send(new Infrastructure.Services.CPD.GetTransactionStatusQuery() { TransactionStatusXmlRequest = transactionStatusXmlDocument?.InnerXml }, cancellationToken);
                    }
                }
                i++;
            }
            
            return transactionStatus;
        }

    }

}
