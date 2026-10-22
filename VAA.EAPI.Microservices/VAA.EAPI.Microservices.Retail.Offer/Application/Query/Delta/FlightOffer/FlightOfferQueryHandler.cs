using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOfferGql;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOffer
{
    public class FlightOfferQueryHandler : IRequestHandler<FlightOfferQuery, Result<Response<FlightOfferResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly IDistributedCacheService _distributedCacheService;

        public FlightOfferQueryHandler(IMediator mediator, IDistributedCacheService distributedCacheService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Response<FlightOfferResponse>>> Handle(FlightOfferQuery request, CancellationToken cancellationToken)
        {
            var sliceZeroPrice = Enumerable.Empty<EAPI.Domain.Virgin.Models.Common.Price>();

            if (!string.IsNullOrEmpty(request.Request.FlightsearchRequest.OfferID))
            {
                var offerDocId = request.Request.FlightsearchRequest.OfferID.Split(Constants.Application.Pipeline).First();
                var offerDocument = await _distributedCacheService.Get<OfferDocument>(offerDocId);

                sliceZeroPrice = offerDocument.FlightOfferResponse?.Offers?.FirstOrDefault(x => x.OfferId.Is(request.Request.FlightsearchRequest.OfferID))?.SalePrice;

                request.Request.Pos = offerDocument.FlightOfferResponse.Pos;

                if (sliceZeroPrice.IsNullOrEmpty())
                    return new Response<FlightOfferResponse> { Status = Status.BadRequest };
            }

            var docId = Guid.NewGuid();
            Response<FlightOfferResponse> flightOfferResponse;

            if (request.Request.FeatureFlagUseGraphQl.GetValueOrDefault(ConfigReader.Get<bool>(Constants.Environment.SwitchToAwsGraphql)))
            {
                flightOfferResponse = await _mediator.Send(new FlightOfferGqlServiceQuery
                {
                    Request = request.Request,
                    SliceZeroPrice = sliceZeroPrice,
                    DocId = docId
                }, cancellationToken);
            }
            else
            {
                flightOfferResponse = await _mediator.Send(new FlightOfferServiceQuery
                {
                    Request = request.Request,
                    SliceZeroPrice = sliceZeroPrice,
                    DocId = docId
                }, cancellationToken);
            }

            if (flightOfferResponse.Status == Status.OK)
            {
                var document = new OfferDocument
                {
                    Id = docId.ToString(),
                    PartitionKey = docId.ToString(),
                    CreatedBy = Constants.Application.SearchOfferChannelID,
                    CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                    DocType = Constants.Application.OfferDocType,
                    FlightOfferResponse = new()
                    {
                        Pos = flightOfferResponse.Value.Pos,
                        Parties = flightOfferResponse.Value.Parties,
                        DataList = flightOfferResponse.Value.DataList,
                        Offers = flightOfferResponse.Value.Offers
                    }
                };

                await _distributedCacheService.Set(document.Id, document, false, TimeSpan.Zero, TimeSpan.FromMinutes(ConfigReader.Try(Constants.Environment.DeltaOfferExpiryLimitInMinutes, 120)));
            }

            return Result.Success(flightOfferResponse);
        }
    }
}
