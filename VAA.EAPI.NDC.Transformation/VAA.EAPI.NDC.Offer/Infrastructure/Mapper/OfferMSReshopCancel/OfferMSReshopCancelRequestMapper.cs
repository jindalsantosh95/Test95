using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OfferMSReshopCancel
{
    public class OfferMSReshopCancelRequestMapper
    {
        public static OrderReshopRequest MapRequest(OrderDocument orderDocument, bool isCancelDisruption)
        {
            var customerDetails = orderDocument?.OrderItems.Where(x => x.ItemStatus.Status == Constants.Application.Current).SelectMany(orderItem => orderItem.Services).Select(service => service.ServiceBeneficiaries).GroupBy(serviceBeneficiary => serviceBeneficiary.First()).SelectMany(cust => cust.First());

            return new()
            {
                OrderId = orderDocument?.OrderNumber.OrderId,
                BookingReferences = orderDocument.BookingReference,
                Parties = orderDocument.Parties,
                ReshopOrder = GetReshopOrder(orderDocument),
                CustomerDetails = GetCustomerDetails(orderDocument, customerDetails),
                SegmentDetails = GetSegmentDetails(orderDocument?.OrderItems?.First()?.Services, orderDocument?.DataList.SegmentDetails),
                Pos = orderDocument.PointOfSale,
                IsCancelDisruption = isCancelDisruption
            };
        }

        private static IEnumerable<ReshopOrder> GetReshopOrder(OrderDocument orderDocument)
        {
            var orderItems = orderDocument?.OrderItems.Where(oi => oi.ItemStatus?.Status == Constants.Application.Current && oi.Services.First().Product?.ProductType == Product.ProductTypes.FlightEnum);

            return orderItems.Select(oi =>
                new ReshopOrder()
                {
                    OrderItemId = oi?.OrderItemId,
                    Services = GetOrderServices(oi?.Services, orderDocument.DataList, oi?.OwnerAirline),
                    PriceDetails = GetPriceDetails(oi?.PriceDetails.ToList()),

                    AccountableDocumentRefs = oi.AccountableDocumentRefs?.Select(x =>
                    {
                        var fareCalculation = x.FareCalculation?.Replace("/VV81", "");

                        return new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef
                        {
                            FareCalculation = fareCalculation,
                            AllFlown = x.AllFlown,
                            Coupons = x.Coupons,
                            CustomerRefId = x.CustomerRefId,
                            DocumentIssuer = x.DocumentIssuer,
                            DocumentNumber = x.DocumentNumber,
                            DocumentType = x.DocumentType,
                            FirstNameNumber = x.FirstNameNumber,
                            IssueDate = x.IssueDate,
                            IssueTime = x.IssueTime,
                            LastNameNumber = x.LastNameNumber,
                            OriginalDocumentNumber = x.OriginalDocumentNumber,
                            OriginalIssueDate = x.OriginalIssueDate,
                            Primary = x.Primary,
                            ReasonForIssuance = x.ReasonForIssuance
                        };
                    })
                });
        }

        private static IEnumerable<Service> GetOrderServices(IEnumerable<OrderService> services, DataList dataList, string productOwner)
        {
            return services.Select(service =>
            {
                var priceClass = dataList.PriceClass.First(x => x.Id == service.PriceClassRefIds.FirstOrDefault());

                return new Service
                {
                    ServiceId = service.ServiceId,
                    Product = new()
                    {
                        ProductOwner = productOwner,
                        ProductName = priceClass.ProductName,
                        ProductType = Products.ProductTypeEnum.Flight
                    },
                    ProductSegmentAssociation = new()
                    {
                        OfferSegmentRefIds = service.SegmentRefIds
                    },
                    ServiceBeneficiaries = service.ServiceBeneficiaries
                };
            });
        }

        private static IEnumerable<Price> GetPriceDetails(List<Price> priceDetails)
        {
            var taxBreakup = priceDetails.First(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).TaxBreakup.Select(x => new Price { PriceComponentType = Price.PriceComponentTypes.TaxEnum, PriceComponentName = x.Code, Amount = x.Amount });
            priceDetails.AddRange(taxBreakup);

            return priceDetails.Select(price => new Price()
            {
                PriceComponentType = price.PriceComponentType,
                PriceComponentName = price.PriceComponentName,
                Amount = new()
                {
                    MonetaryAmount = price.Amount?.MonetaryAmount,
                    BaseCurrency = price.Amount?.BaseCurrency
                },

                BaseCurrencyAmount = price.BaseCurrencyAmount == null ? null : new()
                {
                    Amount = price.BaseCurrencyAmount.Amount,
                    Currency = price.BaseCurrencyAmount.Currency
                }
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
    }
}
