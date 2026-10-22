using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;

namespace VAA.EAPI.NDC.Order.Validations.Common
{
    public class ValidateFlightServieListResponse
    {
        public static IEnumerable<ErrorDetail> ValidateFlightServiceListErrorResponse(FlightOfferServiceRS flightOfferServiceRS)
        {
            var errors = new List<ErrorDetail>();

            if (flightOfferServiceRS.ServiceListResponse == null && flightOfferServiceRS.Error == null)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_34" });

            if (!errors.Any() && !flightOfferServiceRS.Error.IsNullOrEmpty())
            {
                if (flightOfferServiceRS.Error.Any(x => Constants.Application.FlightOfferServiceErrorCodeGeneral.Split(';').ToList().Contains(x.Code)))
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_22" });

                if (flightOfferServiceRS.Error.Any(x => x.Code == Constants.Application.FlightOfferServiceErrorCodeNoDataMatch))
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_44" });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateFlightServiceListResponseOrderChange(FlightOfferServiceRS flightOfferServiceRS, List<Domain.OrderChangeRQ.DistributionChainLinkType> distributionChain, Domain.OrderChangeRQ.AcceptSelectedQuotedOfferListType acceptedOffer)
        {
            var errors = new List<ErrorDetail>();

            var sender = distributionChain.Count > 1 ? distributionChain.LastOrDefault() : distributionChain.FirstOrDefault();
            if (flightOfferServiceRS.ServiceListResponse.Parties?.Participants.FirstOrDefault()?.Name != sender.ParticipatingOrg.Name || flightOfferServiceRS.ServiceListResponse.Parties?.Participants.FirstOrDefault().AgencyID != sender.ParticipatingOrg.OrgID)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "368_2" });

            if (!errors.Any())
            {
                var orderChangeServiceListOfferId = acceptedOffer.SelectedPricedOffer.SelectMany(order => order.SelectedOfferItem).Where(offerItem => offerItem.SelectedALaCarteOfferItem != null && !(offerItem?.SelectedALaCarteOfferItem.OfferFlightAssociations.Item as Domain.OrderChangeRQ.PaxSegmentRefListType).PaxSegmentRefID.IsNullOrEmpty());

                var flightOfferServiceListOfferId = flightOfferServiceRS.ServiceListResponse.Offers.Select(offer => offer?.OfferItems.Where(offerItem => offerItem?.OfferItemId != null).ToList()).FirstOrDefault();

                if (orderChangeServiceListOfferId.Any() && flightOfferServiceListOfferId.Any())
                {
                    var matchedOfferIds = flightOfferServiceListOfferId.Where(off => orderChangeServiceListOfferId.Any(x => off.OfferItemId == x.OfferItemRefID)).Select(off => off.OfferItemId);

                    var duplicateOfferId = orderChangeServiceListOfferId.GroupBy(offer => new { offer.OfferItemRefID, (offer?.SelectedALaCarteOfferItem.OfferFlightAssociations.Item as Domain.OrderChangeRQ.PaxSegmentRefListType).PaxSegmentRefID }).Where(off => off.Count() > 1);

                    if (!matchedOfferIds.Any() || duplicateOfferId.Any())
                        errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_44" });
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateFlightServiceListResponseOrderCreate(FlightOfferServiceRS flightOfferServiceRS, List<Domain.OrderCreateRQ.DistributionChainLinkType> distributionChain, Domain.OrderCreateRQ.AcceptSelectedQuotedOfferListType acceptedOffer)
        {
            var errors = new List<ErrorDetail>();

            var sender = distributionChain.Count > 1 ? distributionChain.LastOrDefault() : distributionChain.FirstOrDefault();
            if (flightOfferServiceRS.ServiceListResponse.Parties?.Participants.FirstOrDefault()?.Name != sender.ParticipatingOrg.Name || flightOfferServiceRS.ServiceListResponse.Parties?.Participants.FirstOrDefault().AgencyID != sender.ParticipatingOrg.OrgID)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "368_2" });

            if (!errors.Any())
            {
                var orderCreateServiceListOfferId = acceptedOffer.SelectedPricedOffer.SelectMany(order => order.SelectedOfferItem).Where(offerItem => offerItem.SelectedALaCarteOfferItem != null && !(offerItem?.SelectedALaCarteOfferItem.OfferFlightAssociations.Item as Domain.OrderCreateRQ.PaxSegmentRefListType).PaxSegmentRefID.IsNullOrEmpty());

                var flightOfferServiceListOfferId = flightOfferServiceRS.ServiceListResponse.Offers.Select(offer => offer?.OfferItems.Where(offerItem => offerItem?.OfferItemId != null)).FirstOrDefault();

                if (orderCreateServiceListOfferId.Any() && flightOfferServiceListOfferId.Any())
                {
                    var matchedOfferIds = flightOfferServiceListOfferId.Where(off => orderCreateServiceListOfferId.Any(x => off.OfferItemId == x.OfferItemRefID)).Select(off => off.OfferItemId);

                    var duplicateOfferId = orderCreateServiceListOfferId.GroupBy(offer => new { offer.OfferItemRefID, (offer?.SelectedALaCarteOfferItem.OfferFlightAssociations.Item as Domain.OrderCreateRQ.PaxSegmentRefListType).PaxSegmentRefID }).Where(off => off.Count() > 1);

                    if (!matchedOfferIds.Any() || duplicateOfferId.Any())
                        errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_44" });
                }
            }

            return errors;
        }
    }
}
