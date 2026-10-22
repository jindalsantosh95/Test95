using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentReferenceIdGenerateCommandHandler : IRequestHandler<OrderPaymentReferenceIdGenerateCommand, Result<Response<PaymentRefIdResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ICachedChannel _cachedChannel;

        public OrderPaymentReferenceIdGenerateCommandHandler(IMediator mediator, ICachedChannel cachedChannel)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
        }

        public async Task<Result<Response<PaymentRefIdResponse>>> Handle(OrderPaymentReferenceIdGenerateCommand request, CancellationToken cancellationToken)
        {
            var paymentId = await _mediator.Send(new OrderPaymentReferenceIdGenerateRepoCommand() { Channel = request.Channel, Request = request.Request }, cancellationToken);

            var uriBuilder = new UriBuilder(ConfigReader.Get(Constants.Environment.Air4HppPaymentLink).Replace("{paymentReferenceId}", paymentId.Value.Value));

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            query.Add("channelId", request.Channel ?? AsyncContext.Current.GetDeltaChannel());

            var channel = await _cachedChannel.GetChannelEntity(query["channelId"]);

            query.Add("appId", channel?.DlApplicationId ?? string.Empty);

            query.Add("purchasePath", AsyncContext.Current.HasChannelKey(request.Channel ?? AsyncContext.Current.GetDeltaChannel(), "purchasepath") ? AsyncContext.Current.GetChannelKey(request.Channel, "purchasepath") : channel?.DlPaymentPurchasePath ?? string.Empty);

            uriBuilder.Query = query.ToString();

            var response = (PaymentRefIdResponse)uriBuilder;

            response.PaymentURL = ConfigReader.Try(Constants.Environment.MobileViewPaymentUrl);

            return new Response<PaymentRefIdResponse>
            {
                Status = Status.OK,
                Value = response
            };
        }
    }
}
