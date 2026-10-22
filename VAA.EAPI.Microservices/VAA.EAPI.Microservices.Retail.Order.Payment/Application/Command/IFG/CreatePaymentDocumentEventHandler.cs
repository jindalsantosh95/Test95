using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.IFG
{
    public class CreatePaymentDocumentEventHandler : ICreatePaymentDocumentEvent
    {
        private readonly CreatePaymentDocRepoCommandHandler _createPaymentDocRepoCommand;
        private readonly PaymentDocRepoQueryHandler _paymentDocRepoQueryHandler;
        private readonly ServiceResponseInsertCommandHandler _serviceResponseInsertCommandHandler;

        public CreatePaymentDocumentEventHandler(CreatePaymentDocRepoCommandHandler createPaymentDocRepoCommandHandler, PaymentDocRepoQueryHandler paymentDocRepoQueryHandler, ServiceResponseInsertCommandHandler serviceResponseInsertCommandHandler)
        {
            _createPaymentDocRepoCommand = createPaymentDocRepoCommandHandler ?? throw new ArgumentNullException(nameof(createPaymentDocRepoCommandHandler));
            _paymentDocRepoQueryHandler = paymentDocRepoQueryHandler ?? throw new ArgumentNullException(nameof(paymentDocRepoQueryHandler));
            _serviceResponseInsertCommandHandler = serviceResponseInsertCommandHandler ?? throw new ArgumentNullException(nameof(serviceResponseInsertCommandHandler));
        }

        public async Task<Result<Unit>> Handle(EAPI.Domain.Events.CloudEvent request, CancellationToken cancellationToken)
        {
            try
            {
                var requestData = Json.Deserialize<PaymentDocRequest>(Json.Serialize(request.Data));

                var transactionId = string.Empty;

                if (requestData.Type == Constants.Application.DocType)
                {
                    transactionId = PaymentIdHelper.GeneratePaymentId();
                }
                else
                {
                    var paymentDocs = await _paymentDocRepoQueryHandler.Handle(new PaymentDocRepoQuery { OrderId = requestData.OrderId, Query = Constants.Application.RefundDocumentQuery }, cancellationToken);
                    var refundPaymentDoc = Json.Deserialize<PaymentTransactionDocument>(Json.Serialize(paymentDocs.First()));

                    var paymentId = refundPaymentDoc.Id;

                    if (!paymentId.Contains("Refund"))
                    {
                        transactionId = paymentId + "-Refund-" + 1;
                    }
                    else
                    {
                        var recordNumber = int.Parse(paymentId.Split('-').Last()) + 1;
                        transactionId = transactionId.TrimEnd('%') + "-Refund-" + recordNumber;
                    }
                }

                var paymentDoc = PaymentDocumentMapper.Mapper(requestData, transactionId);

                await _createPaymentDocRepoCommand.Handle(new CreatePaymentDocRepoCommand(paymentDoc), cancellationToken);

                var ntdhRequest = NtdhMapper.CashRequestMapper(paymentDoc);

                var fileName = $"{DateTime.UtcNow:yyMMddHHmmssff}-{transactionId}.xml";
                var containerName = ConfigReader.Get(Constants.Environment.RpaNtdhContainerName);

                var fileStream = new MemoryStream(Encoding.Default.GetBytes(Xml.Serialize(ntdhRequest)));
                fileStream.Seek(0, SeekOrigin.Begin);

                await BlobStorageHelper.FileUploadToBlob(fileStream, fileName, containerName);

                var response = PaymentResponseMapper.ResponseMapper(paymentDoc);

                await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.CreatePaymentDocumentEventType }, cancellationToken);

            }
            catch (Exception ex)
            {
                await _serviceResponseInsertCommandHandler.Handle(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ex.Message), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.CreatePaymentDocumentEventType }, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
