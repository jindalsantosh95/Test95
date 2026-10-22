using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class RpaNdthEventProcessCommandHandler : IRequestHandler<RpaNdthEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public RpaNdthEventProcessCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Unit>> Handle(RpaNdthEventProcessCommand request, CancellationToken cancellationToken)
        {
            var ntdhContent = Xml.Deserialize<NdcTicketDataHandoff>(request.Data.ToString());

            var transactionId = ntdhContent.Rxoi.PaymentDocumentReferenceIndicator;
            var fileName = $"{DateTime.UtcNow:yyMMddHHmmssff}-{transactionId}.xml";
            var containerName = ConfigReader.Get(Constants.Environment.RpaNtdhContainerName);

            var fileStream = new MemoryStream(Encoding.Default.GetBytes(request.Data.ToString()));
            fileStream.Seek(0, SeekOrigin.Begin);

            await BlobStorageHelper.FileUploadToBlob(fileStream, fileName, containerName);
            
            return Unit.Value;
        }

    }
}
