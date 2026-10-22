using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;
using Status = VAA.EAPI.Domain.Entities.Common.Status;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<Response<Domain.CPD.PaymentResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ILogger<RefundPaymentCommandHandler> _logger;

        public RefundPaymentCommandHandler(IMediator mediator, ILogger<RefundPaymentCommandHandler> logger, IHttpConnector httpConnector)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<Domain.CPD.PaymentResponse>>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                CreateRefundDocumentRequest refundDocument = null;

                var refundApiRequest = (Infrastructure.Models.CPD.RefundPaymentRequest)request.RefundPaymentRequest;
                var refundXmlDocument = new XmlDocument();
                refundXmlDocument.LoadXml(Xml.Serialize(refundApiRequest));

                var refundPaymentApiResponse = await _mediator.Send(new Infrastructure.Services.CPD.RefundPaymentCommand() { RefundPaymentXMLRequest = refundXmlDocument?.InnerXml }, cancellationToken);

                var capturedPaymentDocument = await _mediator.Send(new PaymentDocumentReaderQuery() { TransactionId = request.RefundPaymentRequest.PaymentDetails.First().PaymentTransactionId, SqlQuery = Constants.Application.PaymentQuery }, cancellationToken);

                var response = new Domain.CPD.PaymentResponse();
                response = PaymentResponseMapper.RefundPaymentResponseMapper(refundApiRequest, refundPaymentApiResponse.Value.Value, capturedPaymentDocument);

                if (response.Errors is null)
                {
                    if (refundPaymentApiResponse.Value.Value.Status.Any(status => status.Code == "1001"))
                    {
                        if (capturedPaymentDocument != null)
                        {
                            capturedPaymentDocument.Status = "Void";
                            await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = capturedPaymentDocument, ModificationType = CosmosModificationType.Upsert }), cancellationToken);
                        }
                    }
                    else
                    {
                        var refundTransactionId = refundApiRequest.Refund.Transaction.Id + "%";

                        var RefundPaymentDocument = await _mediator.Send(new PaymentDocumentReaderQuery() { TransactionId = refundTransactionId, SqlQuery = Constants.Application.RefundQuery }, cancellationToken);

                        if (RefundPaymentDocument != null)
                        {
                            if (!RefundPaymentDocument.Id.Contains("Refund"))
                            {
                                refundTransactionId = RefundPaymentDocument.Id + "-Refund-" + 1;
                            }
                            else
                            {
                                var recorNumber = int.Parse(RefundPaymentDocument.Id.Split('-').Last()) + 1;
                                refundTransactionId = refundTransactionId.TrimEnd('%') + "-Refund-" + recorNumber;
                            }

                            refundDocument = RefundDocumentRequestMapper.CreateRefundDocumentMapper(request.RefundPaymentRequest, refundTransactionId, response, capturedPaymentDocument);

                            response.TransactionId = refundTransactionId;

                            await _mediator.Send(new CreateRefundDocRepoCommand(refundDocument));

                            var paymentDetails = capturedPaymentDocument.PaymentDetails.First();
                            var capturedAmountString = !string.IsNullOrWhiteSpace(paymentDetails.Amount?.BalanceAmount) ? paymentDetails.Amount.BalanceAmount: paymentDetails.Amount?.MonetaryAmount;
                            var capturedAmount = decimal.TryParse(capturedAmountString, out var cAmt) ? cAmt : 0; var refundAmount = decimal.TryParse(request.RefundPaymentRequest.PaymentDetails.First().RefundAmount?.MonetaryAmount, out var rAmt) ? rAmt : 0;

                            var balanceAmount = capturedAmount - refundAmount;

                            if(balanceAmount >= 0)
                            {
                                capturedPaymentDocument.PaymentDetails = capturedPaymentDocument.PaymentDetails.Select((p, i) => { if (i == 0)  p.Amount.BalanceAmount = balanceAmount.ToString();return p;});
                            }

                            await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = capturedPaymentDocument, ModificationType = CosmosModificationType.Upsert} ),cancellationToken);

                            var ntdhRequest = NtdhMapper.RefundRequestMapper(capturedPaymentDocument, refundTransactionId);
                            var ntdhCloudEvent = NtdhMapper.EventMapper(ntdhRequest, true);

                            await _mediator.Send(new EventRequestInsertCommand { Id = ntdhCloudEvent.Id, Type = ntdhCloudEvent.Type, Data = ntdhCloudEvent.Data, Subject = ntdhCloudEvent.Subject }, cancellationToken);

                            try
                            {
                                await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = ntdhCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.NtdhTopicName }, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                await _mediator.Send(new ServiceResponseInsertCommand { Id = ntdhCloudEvent.Id, Status = ServiceResponseProcessStatus.Error, Type = ntdhCloudEvent.Type }, cancellationToken);
                                throw new Exception($"Error occurred in RefundPaymentCommandHandler while publishing message to service bus : {ex.Message}");
                            }

                            await _mediator.Send(new ServiceResponseInsertCommand { Id = ntdhCloudEvent.Id, Status = ServiceResponseProcessStatus.Completed, Type = ntdhCloudEvent.Type }, cancellationToken);

                        }
                    }
                }
                if (response.Errors == null)
                    return new Response<Domain.CPD.PaymentResponse>() { Status = Status.OK, Value = response };
                else
                    return new Response<Domain.CPD.PaymentResponse>() { Status = Status.BadRequest, Errors = response.Errors.ToList() };

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in CPD Refund PaymentMS call - {ex.Message}");

                var errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.DefaultTechErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                return Result.Failure<Response<Domain.CPD.PaymentResponse>>(Json.Serialize(errors));
            }
        }
    }
}
