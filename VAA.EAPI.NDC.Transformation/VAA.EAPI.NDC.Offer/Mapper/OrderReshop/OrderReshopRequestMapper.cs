using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.ReshopFareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderReshop;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class OrderReshopRequestMapper
    {
        public static Parties GetParties(IATA_OrderReshopRQ orderReshopRQ)
        {
            var distributionChain = (orderReshopRQ.DistributionChain.OrderBy(x => int.Parse(x.Ordinal))).Where(x => x.OrgRole != DistributionChainRoleCodeType.Carrier);

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

        public static OrderReshopRequest MapOrderReshopRq(IATA_OrderReshopRQ orderReshopRQ)
        {
            return new OrderReshopRequest()
            {
                BookingReferences = GetOrderReshopBookingReference(orderReshopRQ?.Request?.BookingRef),
                OrderId = orderReshopRQ?.Request?.OrderRefID,
                Parties = GetParties(orderReshopRQ),
                ReshopQueries = new ReshopQueries()
                {
                    FlightSpecific = new FlightSpecific()
                    {
                        SearchCriteria = GetSearchCriteria(orderReshopRQ?.Request)
                    }
                },
                ReshopOrder = GetReshopOrder(orderReshopRQ?.Request),
                OriginDestinationAssociations = GetOriginDestinationAssociations(((orderReshopRQ?.Request.UpdateOrder.Item as ReshopOrderType)?.ReshopOrderChoice.Item as ServiceOrderType)?.AddOfferItems),
            };
        }

        private static BookingReference GetOrderReshopBookingReference(BookingRefType[] bookingRefTypes)
        {
            return new BookingReference { RLOC = bookingRefTypes?.First().BookingID };
        }

        private static IEnumerable<ReshopOrder> GetReshopOrder(OrderReshopRequestType orderReshopRequest)
        {
            var reshopOrderTypeRQ = (ReshopOrderType)orderReshopRequest?.UpdateOrder?.Item;
            var serviceOrderRQ = (ServiceOrderType)reshopOrderTypeRQ?.ReshopOrderChoice?.Item;

            if (serviceOrderRQ?.DeleteOrderItem is null)
                return Enumerable.Empty<ReshopOrder>();

            return serviceOrderRQ.DeleteOrderItem.Select(del => new ReshopOrder()
            {
                ProductType = EnumOperations.GetEnumDescription(Product.ProductTypes.FlightEnum),
                OrderItemId = del?.OrderItemRefID,
                Services = GetServices(del?.RetainServiceID)
            });
        }

        private static IEnumerable<Service> GetServices(IEnumerable<string> retainServiceID)
        {
            if (retainServiceID is null)
                return Enumerable.Empty<Service>();

            return retainServiceID.Select(id => new Service() { ServiceId = id });
        }

        private static SearchCriteria GetSearchCriteria(OrderReshopRequestType orderReshopRequest)
        {
            var reshopOrder = (ReshopOrderType)orderReshopRequest.UpdateOrder.Item;
            var serviceOrder = (ServiceOrderType)reshopOrder.ReshopOrderChoice.Item;
            var searchCriteria = new SearchCriteria()
            {
                CabinTypes = GetCabinTypes(orderReshopRequest),
                NonStopOnly = string.Equals(serviceOrder.AddOfferItems.FlightRelatedCriteria?.FlightCriteria?.FirstOrDefault()?.FlightCharacteristicsCriteria?.FirstOrDefault()?.CharacteristicCode.ToString(), Constants.Application.DirectFlightIndicator, StringComparison.InvariantCulture)
            };
            return searchCriteria;
        }

        private static IEnumerable<ReshopCabinType> GetCabinTypes(OrderReshopRequestType orderReshopRequest)
        {
            var reshopOrderTypeRQ = (ReshopOrderType)orderReshopRequest.UpdateOrder.Item;
            var serviceOrderRQ = (ServiceOrderType)reshopOrderTypeRQ.ReshopOrderChoice.Item;
            var flightRequestOriginDestinationsCriteria = (FlightRequestOriginDestinationsCriteriaType)serviceOrderRQ.AddOfferItems.FlightRequest.Item;

            return flightRequestOriginDestinationsCriteria?.OriginDestCriteria?.FirstOrDefault()?.CabinType?.Select(cab => new ReshopCabinType()
            {
                CabinCode = cab.CabinTypeCode,
                PrefLevel = cab.PrefLevel.PrefLevelCode.ToString()
            });
        }

        private static IEnumerable<OriginDestinationAssociation> GetOriginDestinationAssociations(AddOfferItemsType addOfferItemsType)
        {
            var ondListRQ = (FlightRequestOriginDestinationsCriteriaType)addOfferItemsType?.FlightRequest?.Item;

            return ondListRQ?.OriginDestCriteria.Select(ond =>
            {
                var departureTime = Convert.ToDouble(ond?.OriginDepCriteria?.Time.Hour);
                return new OriginDestinationAssociation()
                {
                    OriginDestination = new OriginDestination()
                    {
                        OriginAirport = ond?.OriginDepCriteria?.IATA_LocationCode,
                        DestinationAirport = ond?.DestArrivalCriteria?.IATA_LocationCode,
                        DepartureDate = departureTime == 0 ? ond?.OriginDepCriteria?.Date.ToString() : ond?.OriginDepCriteria?.Date.AddHours(departureTime).ToString(),
                        DepartureTimeRange = departureTime == 0 ? null : GetDepartureTimeRange(ond)
                    }
                };
            });
        }

        private static DepartureTimeRange GetDepartureTimeRange(DepartureTimeRange departureTimeRange)
        {
            return departureTimeRange != null ? new DepartureTimeRange { LatestTime = departureTimeRange.LatestTime, EarliestTime = departureTimeRange.EarliestTime } : null;
        }

        private static DepartureTimeRange GetDepartureTimeRange(OriginDestCriteriaType originDestType)
        {
            double hoursBeforeDeparture = 0;
            double hoursAfterDeparture = 0;
            var timebeforeDuration = originDestType.OriginDepCriteria?.TimeBeforeDuration;
            var timeAfterDuration = originDestType.OriginDepCriteria?.TimeAfterDuration;
            var departureTime = originDestType.OriginDepCriteria?.Date.AddHours(double.Parse(originDestType.OriginDepCriteria?.Time.Hour.ToString()));

            if (!string.IsNullOrEmpty(timebeforeDuration))
            {
                var beforeDuration = RegexCompiled.Match(timebeforeDuration, Constants.Application.RegexToValidateNumeric)?.Value;
                hoursBeforeDeparture = !string.IsNullOrEmpty(beforeDuration) ? double.Parse(beforeDuration) : 0;
            }

            if (!string.IsNullOrEmpty(timeAfterDuration))
            {
                var afterDuration = Regex.Match(timeAfterDuration, Constants.Application.RegexToValidateNumeric)?.Value;
                hoursAfterDeparture = !string.IsNullOrEmpty(afterDuration) ? double.Parse(afterDuration) : 0;
            }

            return new DepartureTimeRange
            {
                EarliestTime = Convert.ToDateTime(departureTime).AddHours(-hoursBeforeDeparture).ToString(Constants.Application.DateTimeFormat),
                LatestTime = Convert.ToDateTime(departureTime).AddHours(hoursAfterDeparture).ToString(Constants.Application.DateTimeFormat)
            };
        }

        public static OrderReshopBundleQuery MapOrderReshopRq(OrderDocument orderDocument, OrderReshopRequest reshopRequest, IEnumerable<ReshopFareFamily> reshopFareFamilies, IEnumerable<AirportDetail> airportDetails, IEnumerable<OrderItemServices> svcList, AgencyValidateResponse agency, OrderDocument orderdoc, bool isReshopHoldBooking, bool isReshopDisruption)
        {
            var customerDetails = orderDocument?.OrderItems.Where(x => x.ItemStatus?.Status == Constants.Application.Current && x.Services.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum).SelectMany(orderItem => orderItem.Services).Select(service => service.ServiceBeneficiaries).GroupBy(serviceBeneficiary => serviceBeneficiary.First()).SelectMany(cust => cust.First());

            List<string> addODList = new();
            var parties = orderDocument.Parties.Participants.FirstOrDefault();
            return new OrderReshopBundleQuery()
            {
                OrderId = orderDocument?.OrderNumber.OrderId,
                FareTypeRequest = new()
                {
                    FareTypeCode = orderdoc?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = orderdoc?.FareTypeRequest?.FareSubTypeCode
                },
                BookingReferences = new()
                {
                    AirlineDesignator = orderDocument?.OrderNumber.OwnerAirline,
                    RLOC = orderDocument?.BookingReference?.RLOC,
                    RLOCCreateDate = DateTimeHelper.ConvertToUtc(orderDocument?.BookingReference?.RLOCCreateDate, string.Empty, "ddMMMyy")
                },
                Parties = new()
                {
                    Participants = [new ()
                    {
                         PartyType = parties.PartyType,
                         Name = parties.Name,
                         AgencyID = parties.AgencyID,
                         Ordinal = parties.Ordinal,
                         PccCode = parties.PccCode,
                         Contacts = new ()
                         {
                             Email = parties.Contacts?.Email,
                         },
                         IsCreditCardAccepted = parties.IsCreditCardAccepted,
                         AccountCode = (orderdoc.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode) ? agency.AgencyDetails.AccountCode : (orderdoc.FareTypeRequest?.FareTypeCode == Constants.Application.CorporateFareTypeCode) ? agency.CorporateDetails.CorporateAccountCode : null
                    }],
                    CorporateAccount = orderdoc.Parties?.CorporateAccount
                },
                ReshopOrder = GetReshopOrder(orderDocument, reshopRequest.ReshopOrder, svcList),
                OriginDestinationAssociations = GetOriginDestinationAssoc(orderDocument, reshopRequest.OriginDestinationAssociations, addODList, airportDetails),
                ReshopQueries = GetReshopQueries(orderDocument, reshopRequest, customerDetails, reshopRequest.OriginDestinationAssociations, addODList, reshopFareFamilies),
                CustomerDetails = GetCustomerDetails(orderDocument, customerDetails,orderdoc.DataList),
                SegmentDetails = GetSegmentDetails(orderDocument?.OrderItems?.First()?.Services, orderDocument?.DataList.SegmentDetails),
                Pos = new()
                {
                    CityCode = Constants.Application.POSCityCodeTBD,
                    CountryCode = orderDocument?.PointOfSale?.CountryCode,
                    CurrencyCode = orderDocument?.PointOfSale?.CurrencyCode,
                    DefaultCityCode = orderDocument?.PointOfSale?.DefaultCityCode
                },

                FormOfPayments = orderdoc.OrderItems.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.FormOfPayments,
                IsReshopHoldBooking = isReshopHoldBooking,
                IsReshopDisruption = isReshopDisruption
            };
        }

        private static IEnumerable<Customer> GetCustomerDetails(OrderDocument orderDocument, IEnumerable<string> customers , DataList dataList)
        {
            var matchedCustomers = dataList.CustomerDetails.Where(x => customers.Contains(x.CustId));

            var custIdToItaPtcMap = matchedCustomers.ToDictionary(c => c.CustId, c => c.ItaPtc);

            return from custId in customers.Distinct()
                   join customer in orderDocument.DataList.CustomerDetails.Distinct() on custId equals customer.CustId

                   select new Customer()
                   {
                       CustId = customer.CustId,
                       LinkedInfantId = customer.LinkedInfantId ?? string.Empty,
                       Ptc = customer.Ptc == Constants.Application.ChildPTCCode ? Constants.Application.CNNPTCCode : customer.Ptc,
                       ItaPtc = custIdToItaPtcMap.ContainsKey(custId) ? custIdToItaPtcMap[custId] : string.Empty
                   };
        }

        private static IEnumerable<ReshopOrder> GetReshopOrder(OrderDocument orderDocument, IEnumerable<ReshopOrder> reshopOrder, IEnumerable<OrderItemServices> svcList)
        {
            var allOrderItems = orderDocument?.OrderItems;
            var flightOrderItems = orderDocument?.OrderItems.Where(orderItem => reshopOrder.Any(z => z.OrderItemId == orderItem.OrderItemId));

            if (!reshopOrder.Any() && !flightOrderItems.Any())
            {
                flightOrderItems = orderDocument?.OrderItems.Where(oi => oi.ItemStatus.Status == Constants.Application.Current && oi.Services.First().Product?.ProductCode?.ToUpper() != Constants.Application.SeatProductCode);
            }
            var reshopOrders = new List<ReshopOrder>();

            foreach (var orderItem in allOrderItems)
            {
                if (orderItem.Services == null || !orderItem.Services.Any())
                    continue;

                var product = orderItem.Services.FirstOrDefault()?.Product;
                var productType = product?.ProductType;
                bool isFlightProduct = productType == Product.ProductTypes.FlightEnum;

                // ✅ Case 1: Flight products
                if (isFlightProduct)
                {
                    var reshop = new ReshopOrder
                    {
                        OrderItemId = orderItem.OrderItemId,

                        ServiceBeneficiaries = svcList?.Where(x => x.RetainServices != null && x.OrderItemId == orderItem.OrderItemId).SelectMany(x => x.ServiceBeneficiaries),

                        RetainServices = svcList?.Where(x => x.RetainServices != null && x.OrderItemId == orderItem.OrderItemId).SelectMany(x => x.RetainServices),

                        DeleteServices = svcList?.Where(x => x.DeleteServices != null && x.OrderItemId == orderItem.OrderItemId).SelectMany(x => x.DeleteServices),

                        OrderProductType = svcList?.Where(x => x.OrderItemId == orderItem.OrderItemId).Select(x => x.OrderProductType).FirstOrDefault(),

                        Services = GetOrderServices(orderItem.Services, orderDocument.DataList, orderItem.OwnerAirline),
                        
                        PriceDetails = GetPriceDetails(orderItem.PriceDetails?.ToList()),

                        AccountableDocumentRefs = GetAccountDocDetails(orderItem, orderDocument),
                    
                    };

                    reshopOrders.Add(reshop);
                }
                // ✅ Case 2: Seat or null products — create one ReshopOrder per svcList entry
                else
                {
                    var relatedSvcItems = svcList.Where(svc => svc.OrderItemId == orderItem.OrderItemId);

                    foreach (var svc in relatedSvcItems)
                    {
                        var reshop = new ReshopOrder
                        {
                            OrderItemId = orderItem.OrderItemId,
                            ServiceBeneficiaries = svc.ServiceBeneficiaries,
                            RetainServices = svc.RetainServices,
                            DeleteServices = svc.DeleteServices,
                            OrderProductType = svc.OrderProductType
                        };

                        reshopOrders.Add(reshop);
                    }
                }
            }
            return reshopOrders;
        }

        private static IEnumerable<AccountableDocumentRef> GetAccountDocDetails(OrderItems orderItem, OrderDocument orderDocument)
        {
            IEnumerable<AccountableDocumentRef> accountableDocumentRefs = null;
            if (orderItem?.AccountableDocumentRefs is null && orderDocument?.TimeLimits is not null)
            {
                accountableDocumentRefs = new[]
                {
                    new AccountableDocumentRef
                    {
                        FareCalculation = orderItem.PriceDetails?.FirstOrDefault()?.FareCalculation?.Replace("/VV81", ""),
                        IssueDate = orderDocument?.CreationDateTime?.UtcDateTime?.Substring(0, 10),
                        CustomerRefId = orderItem.Services?.FirstOrDefault()?.ServiceBeneficiaries?.FirstOrDefault(),
                        Coupons =
                        [
                            new Coupon
                            {
                                FareBasis = orderDocument?.DataList?.PriceClass?
                                    .FirstOrDefault(pc => pc.Id == orderItem.Services?.FirstOrDefault()?.PriceClassRefIds?.FirstOrDefault())
                                    ?.FareBasisCode
                            }
                        ]
                    }
                };

            }
            else
            {
                accountableDocumentRefs = orderItem.AccountableDocumentRefs?.Select(x => new EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef
                {
                    FareCalculation = x.FareCalculation?.Replace("/VV81", ""),
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
                });
            }
                
            
            return accountableDocumentRefs;
        }

        private static ReshopQueries GetReshopQueries(OrderDocument orderDocument, OrderReshopRequest reshopRequest, IEnumerable<string> customerDetails, IEnumerable<OriginDestinationAssociation> originDestinationAssociations, IEnumerable<string> addODList, IEnumerable<ReshopFareFamily> reshopFareFamilies)
        {
            var retainOndList = orderDocument?.DataList?.OriginDestinationAssociations.
                Where(originDest => !originDestinationAssociations.Any(oND => originDest.OriginDestination.OriginAirport == oND.OriginDestination.OriginAirport) &&
                !originDestinationAssociations.Any(originNDestination => originDest.OriginDestination?.DestinationAirport == originNDestination.OriginDestination.DestinationAirport));

            return new ReshopQueries()
            {
                FlightSpecific = new()
                {
                    FlightsRequest = new()
                    {
                        Add = GetAddItems(addODList),
                        Retain = GetRetainItems(reshopRequest.ReshopOrder, retainOndList, orderDocument)
                    },
                    Beneficiaries = customerDetails.Select(x => x),
                    SearchCriteria = new()
                    {
                        CarryOnBaggageAllowance = true,
                        CheckInBaggageAllowance = true,
                        FareFamilies = GetFareFamilies(reshopRequest.ReshopQueries?.FlightSpecific?.SearchCriteria?.CabinTypes, orderDocument, reshopFareFamilies),
                        NonStopOnly = reshopRequest.ReshopQueries?.FlightSpecific?.SearchCriteria?.NonStopOnly
                    }
                }
            };
        }

        private static IEnumerable<string> GetFareFamilies(IEnumerable<ReshopCabinType> cabinTypes, OrderDocument orderDocument, IEnumerable<ReshopFareFamily> reshopFareFamilies)
        {
            IEnumerable<string> fareFamilies = Enumerable.Empty<string>();
            List<FareFamilies> fareRQCombinations = new List<FareFamilies>();

            var productCode = (from pc in orderDocument.DataList.PriceClass
                               where pc.Id == orderDocument.OrderItems.FirstOrDefault().Services.FirstOrDefault().PriceClassRefIds.FirstOrDefault()
                               select pc.ProductName).First();

            if (cabinTypes != null)
            {
                fareFamilies = (from cabin in cabinTypes
                                from family in reshopFareFamilies
                                where cabin.CabinCode == family.CabinCode &&
                                string.Equals(family.ExistingFareFamily, productCode, StringComparison.OrdinalIgnoreCase)
                                select family.ItaFareFamilies).SelectMany(x => x).Distinct();
            }
            else
            {
                FareFamilies fareRQ = new FareFamilies()
                {
                    CabinCode = "",
                };
                fareRQCombinations.Add(fareRQ);

                fareFamilies = (from priceClass in orderDocument.DataList.PriceClass
                                from family in reshopFareFamilies
                                from cabin in fareRQCombinations
                                where priceClass.ProductName == family.ExistingFareFamily && cabin.CabinCode == family.CabinCode &&
                                string.Equals(family.ExistingFareFamily, productCode, StringComparison.OrdinalIgnoreCase)
                                select family.ItaFareFamilies).SelectMany(x => x).Distinct();
            }
            return fareFamilies;

        }

        private static IEnumerable<Add> GetAddItems(IEnumerable<string> addODList)
        {
            return addODList.Select(od => new Add() { OriginDestinationId = od });
        }

        private static IEnumerable<Retain> GetRetainItems(IEnumerable<ReshopOrder> reshopOrder, IEnumerable<VAA.EAPI.Domain.Virgin.Models.Products.Flight.OriginDestinationAssociation> retainOnd, OrderDocument orderDocument)
        {
            if (!reshopOrder.Any() || reshopOrder.First().Services == null || !reshopOrder.First().Services.Any())
                return null;

            var retainServiceIDs = reshopOrder.First().Services.Select(x => x.ServiceId);

            var distinctPax = orderDocument.OrderItems.First().Services.SelectMany(x => x.ServiceBeneficiaries).Distinct();
            int serviceId = 0;
            var serviceList = distinctPax.SelectMany(pax =>
            {
                return orderDocument.OrderItems.First().Services.Where(s => s.Product.ProductType == Product.ProductTypes.FlightEnum).Select(s =>
                {
                    return new OrderService
                    {
                        ServiceId = $"{Constants.Application.SV}{++serviceId}",
                        ServiceBeneficiaries = [pax],
                        SegmentRefIds = s.SegmentRefIds
                    };
                });
            }).ToArray();

            var segmentIDList = retainServiceIDs.SelectMany(x => serviceList.Where(s => x == s.ServiceId)).SelectMany(x => x.SegmentRefIds).Distinct();

            return retainOnd?.Select(ond => new Retain()
            {
                OriginDestinationId = ond.OdLabel,
                SegmentId = segmentIDList
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
            var taxBreakup = priceDetails.First(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).TaxBreakup.Select(x => new Price { PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum, PriceComponentName = x.Code, Amount = x.Amount });
            priceDetails.AddRange(taxBreakup);

            return priceDetails.Select(price => new Price()
            {
                PriceComponentType = price.PriceComponentType,
                PriceComponentName = price.PriceComponentName,
                Amount = new()
                {
                    MonetaryAmount = string.IsNullOrEmpty(price.Amount?.MonetaryAmount) ? price.BaseCurrencyAmount?.Amount : price.Amount?.MonetaryAmount,
                    BaseCurrency = string.IsNullOrEmpty(price.Amount?.BaseCurrency) ? price.BaseCurrencyAmount?.Currency : price.Amount?.BaseCurrency
                },
                BaseCurrencyAmount = new()
                {
                    Amount = string.IsNullOrEmpty(price.BaseCurrencyAmount?.Amount) ? price.Amount.MonetaryAmount : price.BaseCurrencyAmount?.Amount,
                    Currency = string.IsNullOrEmpty(price.BaseCurrencyAmount?.Currency) ? price.Amount.BaseCurrency : price.BaseCurrencyAmount ?.Currency
                }
            });
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

        private static IEnumerable<OriginDestinationAssociation> GetOriginDestinationAssoc(OrderDocument orderDocument, IEnumerable<OriginDestinationAssociation> originDestinationAssociations, List<string> addODList, IEnumerable<AirportDetail> airportDetails)
        {
            List<OriginDestinationAssociation> ondList = new();

            foreach (var od in orderDocument.DataList.OriginDestinationAssociations)
            {
                OriginDestinationAssociation originDestinationAssociation = new()
                {
                    OdLabel = od?.OdLabel,
                    OriginDestination = new()
                    {
                        OriginAirport = od?.OriginDestination?.OriginAirport,
                        DestinationAirport = od?.OriginDestination?.DestinationAirport,
                        DepartureDate = od?.OriginDestination?.DepartureDate
                    }
                };
                ondList.Add(originDestinationAssociation);
            }

            foreach (var ond in originDestinationAssociations)
            {
                var ondAsscociation = new OriginDestinationAssociation
                {
                    OdLabel = Constants.Application.OriginDestinationRef + (ondList.Count + 1)
                };
                addODList.Add(ondAsscociation.OdLabel);

                ondAsscociation.OriginDestination = new OriginDestination()
                {
                    OriginAirport = airportDetails.FirstOrDefault(x => x.AirportCode == ond.OriginDestination.OriginAirport)?.CityCode ?? ond.OriginDestination.OriginAirport,
                    DestinationAirport = airportDetails.FirstOrDefault(x => x.AirportCode == ond.OriginDestination.DestinationAirport)?.CityCode ?? ond.OriginDestination.DestinationAirport,
                    DepartureDate = Convert.ToDateTime(ond.OriginDestination.DepartureDate).ToString(Constants.Application.reshopDeptDateforamt),
                    DepartureTimeRange = GetDepartureTimeRange(ond.OriginDestination.DepartureTimeRange)
                };
                ondList.Add(ondAsscociation);
            }

            return ondList;
        }

        public static OrderDocument UpdateFares(OrderDocument orderMSOrderDoc, OrderDocument sabreOrderDoc)
        {
            var ticketedPnr = sabreOrderDoc.OrderItems.First().AccountableDocumentRefs;
            var fareBasis = sabreOrderDoc.DataList.PriceClass?.FirstOrDefault()?.Id;

            if (ticketedPnr is null || fareBasis is null)
            {
                sabreOrderDoc.DataList.PriceClass = orderMSOrderDoc.DataList.PriceClass;

                var orderDocuments = from orderItem in orderMSOrderDoc.OrderItems
                                     from service in orderItem.Services
                                     from sabreOrderItem in sabreOrderDoc.OrderItems
                                     from sabreService in sabreOrderItem.Services
                                     where service.SegmentRefIds.SequenceEqual(sabreService.SegmentRefIds)
                                     select new { orderItem, sabreOrderItem };

                foreach (var docs in orderDocuments)
                {
                    docs.sabreOrderItem.PriceDetails = docs.orderItem.PriceDetails;
                    foreach (var service in docs.sabreOrderItem.Services)
                    {
                        service.PriceClassRefIds = docs.orderItem.Services.FirstOrDefault(s => s.SegmentRefIds.SequenceEqual(service.SegmentRefIds))?.PriceClassRefIds;
                    }
                }
            }

            return sabreOrderDoc;
        }
    }
}