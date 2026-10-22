using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners
{
    public sealed class MetaPartnerCommand
    {
        public static explicit operator MetaPartnerCommand((OrderCreateRequest orderCreateRequest, OrderDocument orderDocument) source) => new(source);
        public static explicit operator MetaPartnerCommand(MetaPartnerEventCommand source) => source.Data.ToString().DecompressTo<MetaPartnerCommand>();

        private readonly OrderCreateRequest _orderCreateRequest;
        private readonly OrderDocument _orderDocument;

        public string MetaPartnerId => Attributes.TryGetValue("MetaPartnerId", out string metaPartnerId) ? metaPartnerId : string.Empty;
        public string Currency => Attributes.TryGetValue("Currency", out string currency) ? currency : string.Empty;
        public string Total => Attributes.TryGetValue("Total", out string price) ? price : string.Empty;
        public string BookedOn => Attributes.TryGetValue("BookedOn", out string metaPaBookedOnrtnerId) ? metaPaBookedOnrtnerId : string.Empty;
        public string BookingId => Attributes.TryGetValue("BookingId", out string bookingId) ? bookingId : string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = [];


        public MetaPartnerCommand() { }

        private MetaPartnerCommand((OrderCreateRequest orderCreateRequest, OrderDocument orderDocument) source)
        {
            _orderCreateRequest = source.orderCreateRequest ?? throw new ArgumentNullException(nameof(source.orderCreateRequest));
            _orderDocument = source.orderDocument ?? throw new ArgumentNullException(nameof(source.orderDocument));

            foreach (var a in _orderCreateRequest.MetaPartner)
                Attributes.Add(a.Key, a.Value);

            var paymentDetails = _orderDocument.PaymentDetails.First();

            Attributes.AddOrUpdate("Currency", paymentDetails.Amount.BaseCurrency);
            Attributes.AddOrUpdate("Total", (paymentDetails.Amount.MonetaryAmount.Parse<double>() + (paymentDetails.MilesEquivalentPrice?.MonetaryAmount?.Parse<double>() ?? 0.0)).ToString());
            Attributes.AddOrUpdate("BookedOn", _orderDocument.CreatedOn);
            Attributes.AddOrUpdate("BookingId", _orderDocument.BookingReference.RLOC);

            Attributes.AddOrUpdate("PaxCount", _orderCreateRequest.CustomerDetails.Count().ToString());
            Attributes.AddOrUpdate("AdultCount", _orderCreateRequest.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaAdultPTC).ToString());
            Attributes.AddOrUpdate("TeenCount", _orderCreateRequest.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaYoungAdultPTC).ToString());
            Attributes.AddOrUpdate("ChildCount", _orderCreateRequest.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaChildPTC).ToString());
            Attributes.AddOrUpdate("InfantCount", _orderCreateRequest.CustomerDetails.Count(a => a.Ptc == Constants.Application.DeltaInfantPTC).ToString());
            Attributes.AddOrUpdate("FlightFare", (paymentDetails.Amount.MonetaryAmount.Parse<double>() + (paymentDetails.MilesEquivalentPrice?.MonetaryAmount?.Parse<double>() ?? 0.0)).ToString());
            Attributes.AddOrUpdate("PaymentFee", "0"); // Need to check with Fayyaz -687524
            Attributes.AddOrUpdate("PaymentMethod", paymentDetails.CardMethod.CardType.ToString());
            Attributes.AddOrUpdate("FareBasisCode", _orderCreateRequest.SelectedOffers.First().OfferItems.First().Services.First().FareDetails.FareBasisCode);
            Attributes.AddOrUpdate("MarketingCarrier", _orderDocument.DataList.SegmentDetails.First().MarketingCarrier);
            Attributes.AddOrUpdate("CabinClass", _orderCreateRequest.SelectedOffers.First().OfferItems.First().Services.First().FareDetails.FareSegments.First().CabinName);
            Attributes.AddOrUpdate("RouteType", GetRouteType(_orderDocument.DataList?.OriginDestinationAssociations));
            Attributes.AddOrUpdate("HaulType", "long-haul");
            Attributes.AddOrUpdate("CommissionCurrency", paymentDetails.Amount.BaseCurrency);
        }
        private static string GetRouteType(IEnumerable<OriginDestinationAssociation> originDestinationAssociations)
        {
            var ods = originDestinationAssociations?.ToArray();
            if (ods is null || ods.Length == 0) return string.Empty;

            if (ods.Length == 1) return "one-way";

            if (ods.Length == 2 &&
                string.Equals(ods[0]?.OriginDestination?.OriginAirport, ods[1]?.OriginDestination?.DestinationAirport, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ods[0]?.OriginDestination?.DestinationAirport, ods[1]?.OriginDestination?.OriginAirport, StringComparison.OrdinalIgnoreCase))
            {
                return "return";
            }

            return "multiple-destination";
        }
    }
}
