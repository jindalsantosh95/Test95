using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class CreatePnrRepoQuery : IRequest<string>
    {
        public Pos _Pos { get; set; }
        public Parties _Parties { get; set; }
        public List<SelectedOffer> _SelectedOffers { get; set; }
        public List<PaymentDetails> _PaymentDetails { get; set; }
        public List<OriginDestinationAssociation> _OriginDestinationAssociations { get; set; }
        public List<Customer> _CustomerDetails { get; set; }
        public List<Segment> _SegmentDetails { get; set; }
        public List<BaggageAllowances> _BaggageAllowances { get; set; }
        public List<PriceClass> _PriceClass { get; set; }

        public CreatePnrRepoQuery(Pos pos, Parties parties, List<SelectedOffer> selectedOffers, List<PaymentDetails> paymentsDetails,
            List<OriginDestinationAssociation> originDestinationAssociations, List<Customer> customer, List<Segment> segments,
            List<BaggageAllowances> baggageAllowances, List<PriceClass> priceClasses)
        {
            _Pos = pos;
            _Parties = parties;
            _SelectedOffers = selectedOffers;
            _PaymentDetails = paymentsDetails;
            _OriginDestinationAssociations = originDestinationAssociations;
            _CustomerDetails = customer;
            _SelectedOffers = selectedOffers;
            _BaggageAllowances = baggageAllowances;
            _PriceClass = priceClasses;
            _SegmentDetails = segments;
        }
    }
}
