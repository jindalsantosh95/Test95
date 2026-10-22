using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderReshop;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class OrderReshopCancelRequestMapper
    {
        public static Parties GetParties(IATA_OrderReshopRQ orderReshopRQ)
        {
            var distributionChain = orderReshopRQ.DistributionChain.OrderBy(x => int.Parse(x.Ordinal));

            if (distributionChain.Count() == 1)
            {
                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = distributionChain.First()?.ParticipatingOrg?.OrgID,
                            Name = distributionChain.First()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.First().OrgRole.ToString(), true),
                            Ordinal = distributionChain.First()?.Ordinal
                        }
                    }
                };
            }
            else if (distributionChain.Count() > 1)
            {
                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = distributionChain.Last().ParticipatingOrg?.OrgID,
                            Name = distributionChain.Last().ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.Last().OrgRole.ToString(), true),
                            Ordinal = distributionChain.Last().Ordinal
                        },
                        new()
                        {
                            AgencyID = distributionChain.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                            Name = distributionChain.FirstOrDefault()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.FirstOrDefault()?.OrgRole.ToString(), true),
                            Ordinal = distributionChain.FirstOrDefault()?.Ordinal
                        }
                    }
                };
            }

            return null;
        }

        public static OrderReshopRequest MapCancelReshopRq(IATA_OrderReshopRQ orderReshopRQ)
        {
            return new OrderReshopRequest()
            {
                BookingReferences = new BookingReference { RLOC = orderReshopRQ?.Request?.OrderRefID != null && orderReshopRQ.Request.OrderRefID.StartsWith(Constants.Application.Vs932TicketPrefix) ? orderReshopRQ.Request.OrderRefID.Substring(5, 6) : null },
                OrderId = orderReshopRQ?.Request?.OrderRefID,
                Parties = GetParties(orderReshopRQ),
            };
        }

		public static OrderReshopRequest MapOrderReshopRq(OrderDocument orderDocument)
        {
            var customerDetails = orderDocument?.OrderItems.Where(x => x.ItemStatus.Status == Constants.Application.Current).SelectMany(orderItem => orderItem.Services).Select(service => service.ServiceBeneficiaries).GroupBy(serviceBeneficiary => serviceBeneficiary.First()).SelectMany(cust => cust.First());

            return new OrderReshopRequest()
            {
                ReshopOrder = GetReshopOrder(orderDocument),
                CustomerDetails = GetCustomerDetails(orderDocument, customerDetails),
                SegmentDetails = GetSegmentDetails(orderDocument?.OrderItems?.First()?.Services, orderDocument?.DataList.SegmentDetails),
                OrderId = orderDocument.OrderNumber.OrderId,
                BookingReferences = orderDocument.BookingReference,
                OriginDestinationAssociations = orderDocument.DataList.OriginDestinationAssociations,
                Pos = orderDocument.PointOfSale,
                Parties = orderDocument.Parties
            };
        }

		private static IEnumerable<ReshopOrder> GetReshopOrder(OrderDocument orderDocument)
		{
			var orderItems = orderDocument?.OrderItems;
			return orderItems.Select(oi =>
				new ReshopOrder()
				{
					AccountableDocumentRefs = oi?.AccountableDocumentRefs,
					PriceDetails = GetPriceDetails(oi?.PriceDetails.ToList()),
                    Services = GetServices(oi.Services)
				});

		}

        private static IEnumerable<Service> GetServices(IEnumerable<OrderService> orderServices)
        {
            return orderServices.Select(x => new Service()
            {
                ServiceId = x.ServiceId,
                ServiceBeneficiaries = x.ServiceBeneficiaries,
                Product = new() { ProductType = Enum.Parse<Domain.Model.VAA.Reshop.Products.ProductTypeEnum>(x.Product.ProductType.ToString())},
                ProductSegmentAssociation = new() { OfferSegmentRefIds = x.ProductSegmentAssociation.flightSegmentRefIds }
            });
        }


		private static IEnumerable<Customer> GetCustomerDetails(OrderDocument orderDocument, IEnumerable<string> customers)
        {
            return from custId in customers
                   join customer in orderDocument.DataList.CustomerDetails on custId equals customer.CustId
                   select new Customer()
                   {
                       CustId = customer.CustId,
                       Ptc = customer.Ptc == Constants.Application.ChildPTCCode ? Constants.Application.CNNPTCCode : customer.Ptc
                   };
        }

        private static IEnumerable<Segment> GetSegmentDetails(IEnumerable<OrderService> serviceList, IEnumerable<Segment> segments)
        {
            return from service in serviceList
                   from segment in segments
                   where service.SegmentRefIds.Contains(segment.SegmentId)
                   select new Segment()
                   {
                       MarketingCarrier = segment.MarketingCarrier,
                       MarketingFlightNumber = segment.MarketingFlightNumber,
                       OperatingCarrier = segment.OperatingCarrier,
                       OperatingFlightNumber = segment.OperatingFlightNumber,
                       OriginAirportCode = segment.OriginAirportCode,
                       DestinationAirportCode = segment.DestinationAirportCode,
                       ClassOfService = segment.ClassOfService,
                       OriginDestinationRefId = segment.OdRefId,
                       ScheduledArrivalTime = new()
                       {
                           LocalDateTime = DateTime.Parse(segment.ScheduledArrivalTime?.LocalDateTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")
                       },
                       ScheduledDepartureTime = new()
                       {
                           LocalDateTime = DateTime.Parse(segment.ScheduledDepartureTime?.LocalDateTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")
                       },
                       SegmentNumber = segment.SegmentId,
                       SegmentId = segment.SegmentId,
                       FlightDuration = segment.FlightDuration
                   };
        }
        private static IEnumerable<Price> GetPriceDetails(List<Price> priceDetails)
        {
            var taxBreakup = priceDetails.First(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).TaxBreakup.Select(x => new Price { PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum, PriceComponentName = x.Code, Amount = x.Amount });
            priceDetails.AddRange(taxBreakup);

            return priceDetails.Select(price => new Price()
            {
                PriceComponentType = price.PriceComponentType,
                PriceComponentName = price.PriceComponentName,
                Amount = new()
                {
                    MonetaryAmount = string.IsNullOrEmpty(price.BaseCurrencyAmount?.Amount) ? price.Amount?.MonetaryAmount : price.BaseCurrencyAmount?.Amount,
                    BaseCurrency = string.IsNullOrEmpty(price.BaseCurrencyAmount?.Currency) ? price.Amount?.BaseCurrency : price.BaseCurrencyAmount?.Currency
                },
                BaseCurrencyAmount = new()
                {
                    Amount = string.IsNullOrEmpty(price.BaseCurrencyAmount?.Amount) ? null : price.Amount.MonetaryAmount,
                    Currency = string.IsNullOrEmpty(price.BaseCurrencyAmount?.Currency) ? null : price.Amount.BaseCurrency
                }
            });
        }
    }
}