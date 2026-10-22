using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Wallet.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class WalletEventNotificationCommandHandler : IRequestHandler<WalletEventNotificationCommand, Response<string>>
    {
        private readonly IMediator _mediator;

        public WalletEventNotificationCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Response<string>> Handle(WalletEventNotificationCommand request, CancellationToken cancellationToken)
        {
            var correlationIdForEvents = Guid.NewGuid().ToString();

            await PublishEvent(WalletEventGridMapper.WalletDataMapper(ConfigReader.Get<int>(Constants.Environment.RecordCount), correlationIdForEvents), correlationIdForEvents, cancellationToken);

            return WalletResponseMapper(correlationIdForEvents);
        }

        private async Task PublishEvent(CloudEvent walletEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {

            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.WalletNotification, Data = walletEvent.Data, Subject = walletEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(walletEvent, cancellationToken);
            }

            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.WalletNotification, Data = walletEvent.Data, Subject = walletEvent.Subject }, cancellationToken);
                throw;
            }
        }

        private static Response<string> WalletResponseMapper(string correlationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = "location", Value = ConfigReader.Get(Constants.Environment.WalletNotificationResponseUrl).Replace("{CorrelationId}", correlationid) } },
            };
        }
    }

}
