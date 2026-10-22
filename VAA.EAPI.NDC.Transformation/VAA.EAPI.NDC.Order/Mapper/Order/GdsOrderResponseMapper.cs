using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Helper;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using OrderItems = VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS.OrderItems;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public class GdsOrderResponseMapper : IGdsOrderResponseMapper
    {
        private IEnumerable<PaxType> PaxList = Enumerable.Empty<PaxType>();
        private IEnumerable<PaxSegmentType> PaxSegmentList = Enumerable.Empty<PaxSegmentType>();
        private IEnumerable<ContactInfoType> ContactInfoList = Enumerable.Empty<ContactInfoType>();
        private IEnumerable<OriginDestType> OriginDestList = Enumerable.Empty<OriginDestType>();
        private IEnumerable<PaxJourneyType> PaxJourneyList = Enumerable.Empty<PaxJourneyType>();
        private IEnumerable<BaggageAllowanceType> BaggageAllowanceList = Enumerable.Empty<BaggageAllowanceType>();
        private IEnumerable<DatedMarketingSegmentType> DatedMarketingSegmentList = Enumerable.Empty<DatedMarketingSegmentType>();
        private IEnumerable<DatedOperatingSegmentType> DatedOperatingSegmentList = Enumerable.Empty<DatedOperatingSegmentType>();
        private IEnumerable<DatedOperatingLegType> DatedOperatingLegList = Enumerable.Empty<DatedOperatingLegType>();
        private IEnumerable<ServiceDefinitionType> ServiceDefinitionList = Enumerable.Empty<ServiceDefinitionType>();
        private IEnumerable<PriceClassType> PriceClassList = Enumerable.Empty<PriceClassType>();

        public IATA_OrderViewRS MapOrderResponseForGDS(OrderSearchResponse orderSearchResponse, IEnumerable<NDCWarning> warnings = null)
        {
            int adultCustomerIdCount = 1;
            int youngAdultCustomerIdCount = 1;
            int childCustomerIdCount = 1;
            int infantCustomerIdCount = 1;
            int paxCustomerIdCount = 1;

            foreach (var item in orderSearchResponse.OrderItems)
            {
                AddPaxList(orderSearchResponse.CustomerDetails.First(customer => customer.CustId == item.Services.FirstOrDefault().ServiceBeneficiaries.FirstOrDefault()), orderSearchResponse.OriginDestinationAssociations.First().OriginDestination.DepartureDate, ref adultCustomerIdCount, ref youngAdultCustomerIdCount, ref childCustomerIdCount, ref infantCustomerIdCount, ref paxCustomerIdCount);
            }

            var segGroup = orderSearchResponse.SegmentDetails.GroupBy(x => x.OriginDestinationRefId).Select(x => x.ToList());
            foreach (var seg in segGroup)
            {
                var originDest = orderSearchResponse.OriginDestinationAssociations.FirstOrDefault(x => x.ODLabel == seg.First().OriginDestinationRefId);
                var journeyId = GetOrderSearchPaxJourneyId(seg, orderSearchResponse.SegmentDetails);
                AddOrderSearchOriginDestination(originDest?.OriginDestination, journeyId);
            }

            var orderItems = GetOrderItems(orderSearchResponse);
            foreach (var gdsOrderItem in orderItems)
                gdsOrderItem.StatusCode = GetOrderItemStatus(gdsOrderItem);

            return new IATA_OrderViewRS()
            {
                Items = new OrderViewResponseType[]
                {
                    new()
                    {
                        Order = new OrderedOrderType[]
                        {
                            new()
                            {
                                OrderID = orderSearchResponse.BookingReferences.RLOC,
                                OwnerCode = orderSearchResponse.OrderNumber.OwnerAirline,
                                TotalPrice = GetTotalPrice(orderSearchResponse),
                                StatusCode = orderItems.Any(x => x.StatusCode == "ACTIVE") ? "OPENED" : "CLOSED",
                                OrderItem = orderItems,
                            }
                        },
                        DataLists = GetDataList(orderSearchResponse),
                        TicketDocInfo = orderSearchResponse.OrderItems?.FirstOrDefault().AccountableDocumentRefs != null ? GetTicketDocInfo(orderSearchResponse) : null,
                        Warning = GetWarnings(warnings)
                    }
                },
                PaymentFunctions = GetPaymentFunction(orderSearchResponse)
            };
        }

        public PaymentFunctionsOrderType[] GetPaymentFunction(OrderSearchResponse orderSearchResponse)
        {
            var transactionIds = orderSearchResponse.OrderItems.Where(item => item.PaymentDetails != null)?.SelectMany(id => id.PaymentDetails)?.Select(x => x.PaymentReferenceId)?.Distinct()?.ToList();

            var paymentFunction = transactionIds.Where(x => x != null)?.Select(item =>
            {
                var orderItems = orderSearchResponse.OrderItems.Where(x => x.PaymentDetails != null && x.PaymentDetails.Any(x => x.PaymentReferenceId == item));

                return new PaymentFunctionsOrderType()
                {
                    OrderAssociation = new OrderAssociationType()
                    {
                        OrderRefID = orderSearchResponse.OrderNumber.OrderId,
                        OrderItemRefID = orderItems?.Select(item => item.OrderItemId)?.ToArray()
                    },
                    PaymentProcessingSummary = new PaymentProcessingSummaryType()
                    {
                        Amount = new AmountType()
                        {
                            CurCode = orderSearchResponse.OrderItems.First().PriceDetails.First().Amount.BaseCurrency,
                            Value = ConvertToDecimal(Convert.ToDecimal(orderItems.SelectMany(x => x.PaymentDetails).First(p => p.PaymentReferenceId == item).Amount?.MonetaryAmount))
                        },
                        PaymentID = item,
                        PaymentStatusCode = orderItems.SelectMany(x => x.PaymentDetails).First(p => p.PaymentReferenceId == item).PaymentProcessingStatus.ToString(),
                        PaymentProcessingSummaryPaymentMethod = new PaymentProcessingSummaryPaymentMethodType()
                        {
                            Item = orderItems.First().PaymentDetails is not null ? GetPaymentProcessingSummaryPaymentMethod(orderItems.First()) : null,
                        }
                    }
                };
            });

            return paymentFunction.ToArray();
        }

        private static decimal ConvertToDecimal(decimal number)
        {
            var monetaryHolder = Convert.ToDecimal(number);
            string correctedMonetaryAmount = monetaryHolder.ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);

            return Convert.ToDecimal(correctedMonetaryAmount);
        }

        private static ReceiptPaymentCardType GetPaymentProcessingSummaryPaymentMethod(OrderItems orderItem)
        {
            return new ReceiptPaymentCardType()
            {
                CardBrandCode = orderItem.PaymentDetails?.FirstOrDefault()?.CardMethod?.CardBrandCode,
                MaskedCardID = orderItem.PaymentDetails?.FirstOrDefault()?.CardMethod?.CardNumber,
                ExpirationDate = orderItem.PaymentDetails?.FirstOrDefault()?.CardMethod?.ExpiryDate?.Replace("/", ""),

            };
        }

        private TicketDocInfoType[] GetTicketDocInfo(OrderSearchResponse orderSearchResponse)
        {
            var responseTypeTicketDocInfo = new List<TicketDocInfoType>();

            foreach(var orderItem in orderSearchResponse.OrderItems)
            {
                if (orderItem.AccountableDocumentRefs != null)
                {
                    var ticketDocumentList = new List<TicketType>();

                    foreach (var ad in orderItem.AccountableDocumentRefs)
                    {
                        foreach (var rti in responseTypeTicketDocInfo)
                        {
                            foreach (var td in rti.Ticket)
                                ticketDocumentList.Add(td);
                        }

                        if (!ticketDocumentList.Any(td => td.TicketNumber == ad.DocumentNumber))
                            responseTypeTicketDocInfo.Add(MapTicketDocInfo(orderItem.AccountableDocumentRefs, orderSearchResponse, orderItem.Services.First().ProductSegmentAssociation.FlightSegmentIds.FirstOrDefault()));
                        break;
                    }
                }
            }

            return responseTypeTicketDocInfo.ToArray();
        }

        private TicketDocInfoType MapTicketDocInfo(IEnumerable<AccountableDocumentRef> accountableDocumentRefs, OrderSearchResponse orderSearchResponse, string segmentID)
        {
            List<TicketType> ticketList = new();

            var responseTypeTicketDocInfo = new TicketDocInfoType
            {
                PaxRefID = GetSearchPaxId(orderSearchResponse.CustomerDetails.First(cust => cust.CustId == accountableDocumentRefs.First().CustomerRefId)),
                BookletQty = Convert.ToDecimal(Constants.Application.NumberOfBooklets),
                BookletQtySpecified = true
            };

            foreach(var ad in accountableDocumentRefs.GroupBy(x => x.DocumentNumber))
            {
                var accountDoc = accountableDocumentRefs.Where(x => x.DocumentNumber == ad.Key)?.FirstOrDefault();

                TicketType ticketType = new()
                {
                    TicketNumber = accountDoc?.DocumentNumber,
                    TicketDocTypeCode = accountDoc?.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum ? Constants.Application.TicketTypeT : Constants.Application.DocTypePaidSeats,
                    PrimaryDocInd = (accountDoc?.Primary) != null && (bool)accountDoc?.Primary,
                    PrimaryDocIndSpecified = (accountDoc?.Primary) != null && (bool)accountDoc?.Primary,
                    ReportingTypeCode = ReportingTypeCodeType.Airline
                };

                if (accountDoc.Coupons != null && accountDoc.Coupons.Any())
                    ticketType.Coupon = GetCouponInfo(accountDoc, orderSearchResponse.SegmentDetails, segmentID);

                ticketList.Add(ticketType);
            }

            responseTypeTicketDocInfo.Ticket = ticketList.ToArray();
            responseTypeTicketDocInfo.IssueInfo = string.IsNullOrEmpty(accountableDocumentRefs.FirstOrDefault()?.IssueDate) ? null : GetIssueInfo(accountableDocumentRefs.FirstOrDefault().IssueDate);

            return responseTypeTicketDocInfo;
        }

        private TicketIssuanceInfoType GetIssueInfo(string dateOfIssue)
        {
            return new TicketIssuanceInfoType()
            {
                IssueDateSpecified = true,
                IssueDate = Convert.ToDateTime(dateOfIssue),
                IssuingCarrier = new CarrierType()
                {
                    AirlineDesigCode = Constants.Application.OwnerAirline
                }
            };
        }

        private CouponType[] GetCouponInfo(AccountableDocumentRef accountableDoc, IEnumerable<SegmentDetails> segmentDetails, string segmentID)
        {
            return segmentDetails.SelectMany(segment => accountableDoc.Coupons.Where(coupon =>
                    (accountableDoc.DocumentType != AccountableDocumentRef.DocumentTypes.YEnum && Convert.ToInt32(coupon.SegmentRefId) == Convert.ToInt32(segment.SegmentId))
                    || (accountableDoc.DocumentType == AccountableDocumentRef.DocumentTypes.YEnum && Convert.ToInt32(segmentID) == Convert.ToInt32(segment.SegmentId)))
                        .Select(coupon =>
                        {
                            var couponCounter = 0;
                            return new CouponType
                            {
                                CouponNumber = Convert.ToDecimal(((accountableDoc?.AllFlown == true || accountableDoc?.AllFlown == null) && string.IsNullOrEmpty(coupon?.CouponNumber)) ? GetCouponNumberForAllFlown(ref couponCounter)
                                              : ((string.IsNullOrEmpty(coupon?.CouponNumber) && accountableDoc?.DocumentType == AccountableDocumentRef.DocumentTypes.YEnum) ? "1" : coupon.CouponNumber)),
                                FareBasisCode = coupon?.FareBasis ?? null,
                                CouponStatusCode = GetCouponStatus(coupon?.CouponStatus),
                                SoldAirlineInfo = new string[] { GetOrderSearchPaxSegmentId(segment) },
                                RFIC = accountableDoc?.ReasonForIssuance?.RFIC ?? null,
                                RFISC = accountableDoc?.ReasonForIssuance?.RFISC ?? null
                            };
                        })).ToArray();
        }

        private static string GetCouponNumberForAllFlown(ref int couponNumber)
        {
            return couponNumber == 4 ? "1" : couponNumber++.ToString();
        }

        private static string GetCouponStatus(string couponStatus)
        {
            return couponStatus switch
            {
                Constants.Application.SearchOpenCouponStatus => Constants.Application.CouponStatusCodeOpen,
                Constants.Application.SearchFlownCouponStatus => Constants.Application.CouponStatusCodeFlownorused,
                Constants.Application.SearchCheckedInCouponStatus => Constants.Application.CouponStatusCodeCheckedIn,
                Constants.Application.SearchEMDOpenCouponStatus => Constants.Application.CouponStatusCodeOpen,
                _ => couponStatus,
            };
        }

        private string GetSearchPaxId(Customer customer)
        {
            if (PaxList.Any(y => y.Individual.GivenName.First() == customer.FirstName && y.Individual.Surname == customer.LastName))
                return PaxList.First(y => y.Individual.GivenName.First() == customer.FirstName && y.Individual.Surname == customer.LastName).PaxID;
            else
                return null;
        }

        private string GetOrderItemStatus(OrderedOrderItemType gdsOrderItem)
        {
            int removedServices = 0;
            foreach (var service in gdsOrderItem.Service)
            {
                if (service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeRequested || service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeConfirmed || service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeReadyToProceed || service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeReadyToDeliver)
                    return "ACTIVE";

                else if (service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeRemoved)
                    removedServices++;
            }

            if (removedServices == gdsOrderItem.Service.Count())
                return "CANCELLED";

            else
                return "CLOSED";
        }

        private PriceType GetTotalPrice(OrderSearchResponse orderSearchResponse)
        {
            PriceType priceType = new ();
            decimal totalAmount = decimal.Zero;
            string currency = string.Empty;

            foreach(var orderItem in orderSearchResponse.OrderItems.Where(x => x.PriceDetails != null))
            {
                var beneficiaries = new List<string>();
                foreach(var p in orderItem.PriceDetails)
                {
                    if (p.PriceComponentType == null)
                        totalAmount += Convert.ToDecimal(p.Amount.MonetaryAmount);

                    if (p.PriceComponentType == PriceComponentTypes.BasePriceEnum)
                    {
                        foreach(var s in orderItem.Services)
                        {
                            if (!beneficiaries.Any() || (!beneficiaries.Exists(b => b == s.ServiceBeneficiaries.FirstOrDefault())))
                                beneficiaries.Add(s.ServiceBeneficiaries.FirstOrDefault());
                        }
                        totalAmount += Convert.ToDecimal(p.Amount.MonetaryAmount) * beneficiaries.Count;
                        currency = p.Amount.BaseCurrency;
                    }
                }
            }

            priceType.TotalAmount = MapToAmount(totalAmount, currency);
            return priceType;
        }

        private AmountType MapToAmount(decimal monetaryAmount, string currencyCode)
        {
            if (!monetaryAmount.ToString().Equals("#.00"))
            {
                return new AmountType()
                {
                    CurCode = currencyCode,
                    Value = Convert.ToDecimal(monetaryAmount)
                };
            }
            else
            {
                var monetaryHolder = Convert.ToDecimal(monetaryAmount);
                string correctedMonetaryAmount = monetaryHolder.ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);
                return new AmountType()
                {
                    CurCode = currencyCode,
                    Value = Convert.ToDecimal(correctedMonetaryAmount)
                };
            }
        }

        private OrderedOrderItemType[] GetOrderItems(OrderSearchResponse orderSearchResponse)
        {
            var responseOrderItems = new List<OrderedOrderItemType>();
            int orderItemcount = 1;
            var orderItemId = orderSearchResponse.OrderItems.First().OrderItemId[..^2];

            foreach(var ptc in PaxList.GroupBy(x => x.PTC).Select(x => x))
            {
                var custIds = new List<string>();
                foreach(var pt in ptc)
                {
                    custIds.Add(orderSearchResponse.CustomerDetails.First(x => x.FirstName == pt.Individual.GivenName.First() && x.LastName == pt.Individual.Surname).CustId);
                }

                var ois = orderSearchResponse.OrderItems.Where(x => custIds.Contains(x.ServiceBeneficiaries.First()) && !x.Services.Any(x => x.ProductOrder.ProductType == Product.ProductTypes.SeatEnum));

                responseOrderItems.Add(new OrderedOrderItemType()
                {
                    OrderItemID = $"{orderItemId}-{orderItemcount}",
                    OwnerCode = ois.First().OwnerAirline,
                    Price = GetPrice(ois.Where(x => x.PriceDetails != null).SelectMany(x => x.PriceDetails), orderSearchResponse.TotalPrice.BaseCurrency),
                    Service = GetServices(orderSearchResponse, ois)
                });
                orderItemcount++;
            }

            foreach(var seatOrderItem in orderSearchResponse.OrderItems.Where(x => x.Services.Any(x => x.ProductOrder.ProductType == Product.ProductTypes.SeatEnum)))
            {
                var paxSegId = PaxSegmentList.FirstOrDefault(x => new string(x.PaxSegmentID.Where(Char.IsDigit).ToArray()) == seatOrderItem.Services.First().ProductSegmentAssociation.FlightSegmentIds.First().TrimStart('0'))?.PaxSegmentID;

                if(paxSegId == null)
                {
                    var origin = orderSearchResponse.SegmentDetails.First(x => seatOrderItem.Services.First().ProductSegmentAssociation.FlightSegmentIds.First() == x.SegmentNumber).OriginAirportCode;
                    var dest = orderSearchResponse.SegmentDetails.First(x => seatOrderItem.Services.First().ProductSegmentAssociation.FlightSegmentIds.First() == x.SegmentNumber).DestinationAirportCode;
                    paxSegId = PaxSegmentList.First(x => x.DatedMarketingSegmentRefId == DatedMarketingSegmentList.First(x => x.Dep.IATA_LocationCode == origin && x.Arrival.IATA_LocationCode == dest).DatedMarketingSegmentId).PaxSegmentID;
                }

                responseOrderItems.Add(new OrderedOrderItemType()
                {
                    OrderItemID = $"{orderItemId}-{orderItemcount}",
                    OwnerCode = seatOrderItem.OwnerAirline,
                    Price = GetPrice(seatOrderItem.PriceDetails, orderSearchResponse.TotalPrice.BaseCurrency),
                    Service = GetSeatServices(orderSearchResponse.CustomerDetails.FirstOrDefault(customer => customer.CustId == seatOrderItem.Services.FirstOrDefault()?.ServiceBeneficiaries?.FirstOrDefault()), paxSegId,orderSearchResponse, seatOrderItem, orderItemcount)
                });
                orderItemcount++;
            }

            return responseOrderItems.ToArray();
        }

        private DataListsType GetDataList(OrderSearchResponse orderSearchResponse)
        {
            return new DataListsType
            {
                BaggageAllowanceList = BaggageAllowanceList.Any() ? BaggageAllowanceList.ToArray() : null ,
                PriceClassList = PriceClassList.Any() ? PriceClassList.ToArray() : null,
                PaxList = PaxList.Any() ? PaxList.ToArray() : null,
                PaxSegmentList = PaxSegmentList.Any() ? PaxSegmentList.ToArray() : null,
                ContactInfoList = ContactInfoList.Any() ? ContactInfoList.ToArray() : null,
                DatedMarketingSegmentList = DatedMarketingSegmentList.Any() ? DatedMarketingSegmentList.ToArray() : null,
                DatedOperatingSegmentList = DatedOperatingSegmentList.Any() ? DatedOperatingSegmentList.ToArray() : null,
                DatedOperatingLegList = DatedOperatingLegList.Any() ? DatedOperatingLegList.ToArray() : null,
                OriginDestList = OriginDestList.Any() ? OriginDestList.ToArray() : null,
                PaxJourneyList = PaxJourneyList.Any() ? PaxJourneyList.ToArray() : null,
                ServiceDefinitionList = ServiceDefinitionList.Any() ? ServiceDefinitionList.ToArray() : null,
            };
        }

        private void AddPaxList(Customer customer ,string departureDate, ref int adultCustomerIdCount, ref int youngAdultCustomerIdCount, ref int childCustomerIdCount, ref int infantCustomerIdCount, ref int paxCustomerIdCount)
        {
            if (!PaxList.Any(y => y.Individual.GivenName.First() == customer.FirstName && y.Individual.Surname == customer.LastName))
            {
                var passenger = GetPtc(customer, departureDate, ref adultCustomerIdCount, ref youngAdultCustomerIdCount, ref childCustomerIdCount, ref infantCustomerIdCount, ref paxCustomerIdCount);
                PaxList = PaxList.Append(new PaxType()
                {
                    Individual = GetOrderSearchIndividual(customer),
                    PaxID = passenger.ElementAt(1),
                    PTC = passenger.ElementAt(0),
                    IdentityDoc = (customer.TravelDocuments != null) ? GetIdentityDocuments(customer.TravelDocuments) : null,
                    RedressCase = (customer.TravelDocuments != null) ? GetRedressInfo(customer.TravelDocuments) : null,
                    LoyaltyProgramAccount = customer.LoyaltyMemberDetails != null ? new List<LoyaltyProgramAccountType>() {
                            new() {
                                AccountNumber = customer?.LoyaltyMemberDetails?.MemberNumber,
                                LoyaltyProgram = new LoyaltyProgramType()
                                {
                                    ProgramName = GetProgramName(customer.LoyaltyMemberDetails.ProgramCode),
                                    ProgramCode = customer?.LoyaltyMemberDetails?.ProgramCode
                                }
                            }
                        }.ToArray() : null
                });
            }
        }

        private IEnumerable<string> GetPtc(Customer customer, string departureDate, ref int adultCustomerIdCount, ref int youngAdultCustomerIdCount, ref int childCustomerIdCount, ref int infantCustomerIdCount, ref int paxCustomerIdCount)
        {
            if (!string.IsNullOrEmpty(customer?.DateOfBirth))
            {
                string ptc = string.Empty;
                DateTime deptDate = Convert.ToDateTime(departureDate);
                DateTime birthDate = Convert.ToDateTime(customer.DateOfBirth);
                var age = DateTimeHelper.CalculateAge(birthDate, deptDate);

                if (age < 2 && age >= 0) ptc = Constants.Application.InfantPTCCode;
                else if (age < 12 && age >= 2) ptc = Constants.Application.ChildPTCCode;
                else if (age < 16 && age >= 12) ptc = Constants.Application.YoungPTCCode;
                else if (age < 100 && age >= 16) ptc = Constants.Application.AdultPTCCode;

                return new List<string>() { ptc, GetCustomerID(ptc, ref adultCustomerIdCount, ref youngAdultCustomerIdCount, ref childCustomerIdCount, ref infantCustomerIdCount, ref paxCustomerIdCount) };
            }
            else
            {
                return new List<string>() { Constants.Application.PAX, GetCustomerID(Constants.Application.PAX, ref adultCustomerIdCount, ref youngAdultCustomerIdCount, ref childCustomerIdCount, ref infantCustomerIdCount, ref paxCustomerIdCount) };
            }
        }

        private static string GetCustomerID(string pTC, ref int adultCustomerIdCount, ref int youngAdultCustomerIdCount, ref int childCustomerIdCount, ref int infantCustomerIdCount, ref int paxCustomerIdCount)
        {
            string customerID = string.Empty;
            switch (pTC)
            {
                case Constants.Application.AdultPTCCode:
                    customerID = Constants.Application.AdultCustomerID + adultCustomerIdCount;
                    adultCustomerIdCount++;
                    break;
                case Constants.Application.YoungPTCCode:
                    customerID = Constants.Application.YoungAdultCustomerID + youngAdultCustomerIdCount;
                    youngAdultCustomerIdCount++;
                    break;
                case Constants.Application.ChildPTCCode:
                    customerID = Constants.Application.ChildCustomerID + childCustomerIdCount;
                    childCustomerIdCount++;
                    break;
                case Constants.Application.InfantPTCCode:
                    customerID = Constants.Application.InfantCustomerID + infantCustomerIdCount;
                    infantCustomerIdCount++;
                    break;
                case Constants.Application.PAX:
                    customerID = Constants.Application.PassengerCustomerID + paxCustomerIdCount;
                    paxCustomerIdCount++;
                    break;
            }

            return customerID;
        }

        private IndividualType GetOrderSearchIndividual(Customer customer)
        {
            return new IndividualType()
            {
                Birthdate = string.IsNullOrEmpty(customer?.DateOfBirth) ? default : Convert.ToDateTime(customer.DateOfBirth),
                BirthdateSpecified = !string.IsNullOrEmpty(customer?.DateOfBirth),
                GenderCode = !string.IsNullOrEmpty(customer?.Gender) ? new string[] {"M","F","U"}.Contains(customer.Gender) ? Enum.Parse<GenderCodeType>(customer.Gender) : GenderCodeType.X : default,
                GenderCodeSpecified = !string.IsNullOrEmpty(customer?.Gender),
                TitleName = customer.Title,
                GivenName = new string[] { customer.FirstName },
                Surname = customer.LastName
            };
        }

        private RedressCaseType[] GetRedressInfo(List<TravelDocument> travelDocuments)
        {
            return travelDocuments.Where(x => x.DocumentType == "Redress" || x.DocumentType == Constants.Application.AEA)?.Select(travelDoc =>
                 new RedressCaseType
                 {
                     CountryCode = travelDoc?.CountryOfIssue,
                     RedressCaseID = travelDoc?.DocumentNumber
                 }
            ).ToArray();
        }

        private IdentityDocType[] GetIdentityDocuments(List<TravelDocument> travelDocuments)
        {
            List<IdentityDocType> identityDocuments = new();

            foreach(var travelDocument in travelDocuments.Where(x => x.DocumentType == Constants.Application.passportDocumentType || x.DocumentType == Constants.Application.greenCardDocumentType || x.DocumentType == Constants.Application.PT || x.DocumentType ==Constants.Application.GC))
            {
                identityDocuments.Add(new IdentityDocType
                {
                    Birthdate = Convert.ToDateTime(travelDocument.DateOfBirth),
                    BirthdateSpecified = travelDocument.DateOfBirth != null,
                    GenderCode = GetGenderCode(travelDocument.Gender),
                    GenderCodeSpecified = travelDocument.Gender != null,
                    CitizenshipCountryCode = travelDocument.CountryOfResidence,
                    ExpiryDate = Convert.ToDateTime(travelDocument.ExpirationDate),
                    ExpiryDateSpecified = travelDocument.ExpirationDate != null,
                    GivenName = new string[] { travelDocument.PersonName?.FirstName },
                    IdentityDocID = travelDocument.DocumentNumber,
                    IdentityDocTypeCode = (travelDocument.DocumentType.Equals(Constants.Application.passportDocumentType) || travelDocument.DocumentType.Equals(Constants.Application.PT)) ? Constants.Application.PT : Constants.Application.GC,
                    IssueDate = Convert.ToDateTime(travelDocument.IssueDate),
                    IssueDateSpecified = (travelDocument.IssueDate != null),
                    IssuingCountryCode = travelDocument.CountryOfIssue,
                    MiddleName = new string[] { travelDocument.PersonName?.MiddleName },
                    ResidenceCountryCode = travelDocument.CountryOfResidence,
                    Surname = travelDocument.PersonName?.LastName,
                    TitleName = travelDocument.PersonName?.Title,
                    Visa = (travelDocument.DocumentType.Equals(Constants.Application.passportDocumentType) || travelDocument.DocumentType.Equals(Constants.Application.PT)) ? GetVisaDocuments(travelDocuments) : null
                });
            }

            foreach (var travelDoc in travelDocuments.Where(doc => doc.DocumentType.Equals(Constants.Application.knownTravellerDocumentType) || doc.DocumentType.Equals(Constants.Application.CR)))
            {
                identityDocuments.Add(new IdentityDocType
                {
                    IdentityDocID = travelDoc.DocumentNumber,
                    IdentityDocTypeCode = Constants.Application.CR,
                    IssuingCountryCode = travelDoc.CountryOfIssue,
                    GivenName = new string[] { travelDoc.PersonName?.FirstName },
                    Surname = travelDoc.PersonName?.LastName,
                    TitleName = travelDoc.PersonName?.Title,
                });
            }

            return identityDocuments.ToArray();
        }

        private VisaType[] GetVisaDocuments(List<TravelDocument> travelDocuments)
        {
            return travelDocuments.Where(x => x.DocumentType == Constants.Application.visaDocumentType || x.DocumentType == Constants.Application.VI)?.Select(travelDocument => new VisaType
            {
                ExpiryDate = Convert.ToDateTime(travelDocument.ExpirationDate),
                ExpiryDateSpecified = travelDocument.ExpirationDate != null,
                HostCountryCode = travelDocument.CountryOfResidence,
                IssuingCountryCode = travelDocument.CountryOfIssue,
                IssueDate = Convert.ToDateTime(travelDocument.IssueDate),
                IssueDateSpecified = travelDocument.IssueDate != null,
                VisaTypeCode = null,
                VisaID = travelDocument.DocumentNumber
            }).ToArray();
        }

        private GenderCodeType GetGenderCode(string gender)
        {
            return (gender?.ToUpper()) switch
            {
                "M" => GenderCodeType.M,
                "F" => GenderCodeType.F,
                "U" => GenderCodeType.U,
                _ => GenderCodeType.X,
            };
        }

        private string GetProgramName(string code)
        {
            return code switch
            {
                "VS" => "Virgin Atlantic / Flying Club",
                "DL" => "Delta Air Lines / SkyMiles",
                "VA" => "Virgin Australia / Velocity",
                "VX" => "Virgin America / Elevate",
                "CA" => "Air China / PhoenixMiles",
                "NZ" => "Air New Zealand / AirPoints",
                "NH" => "All Nippon Airways / ANA Mileage Club",
                "HA" => "Hawaiian Airlines / Hawaiian Miles",
                "MH" => "Malaysia Airlines / Enrich",
                "SQ" => "Singapore Airlines / Kris Flyer ",
                "SA" => "South African Airways / Voyager",
                "KL" => "KLM / Flying Blue",
                "AF" => "Air France / Flying Blue",
                _ => default
            };
        }

        private string GetOrderSearchPaxJourneyId(IEnumerable<SegmentDetails> flightSegments, IEnumerable<SegmentDetails> segmentDetails)
        {
            var segments = new List<string>();
            foreach (var fSeg in flightSegments)
            {
                segments.Add(GetOrderSearchPaxSegmentId(segmentDetails.First(seg => fSeg.OriginAirportCode == seg.OriginAirportCode && fSeg.DestinationAirportCode == seg.DestinationAirportCode)));
            }
            var paxJourneyId = GetSearchPaxJourneyId(segments);

            return paxJourneyId;
        }

        private string GetSearchPaxJourneyId(IEnumerable<string> segments)
        {
            if (PaxJourneyList.Any(x => x.PaxSegmentRefID.SequenceEqual(segments)))
            {
                var journeyId = PaxJourneyList.Where(x => x.PaxSegmentRefID.SequenceEqual(segments)).Select(x => x.PaxJourneyID).First();
                return journeyId;
            }
            else
            {
                var paxJourneyType = new PaxJourneyType()
                {
                    PaxJourneyID = IdCreator.CreatePaxJourneyID(PaxJourneyList.Count()),
                    PaxSegmentRefID = segments.ToArray()
                };

                PaxJourneyList = PaxJourneyList.Append(paxJourneyType);
                return paxJourneyType.PaxJourneyID;
            }
        }

        private string GetOrderSearchPaxSegmentId(SegmentDetails flightSegment)
        {
            foreach(var seg in PaxSegmentList)
            {
                var dms = DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId == seg.DatedMarketingSegmentRefId).FirstOrDefault();
                if (dms.CarrierDesigCode == flightSegment.MarketingCarrier && dms.MarketingCarrierFlightNumberText == flightSegment.MarketingFlightNumber && DateTime.Equals(dms.Dep.AircraftScheduledDateTime.ToUniversalTime(), Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime).ToUniversalTime()))
                    return seg.PaxSegmentID;
            }

            var paxSegmentType = CreateOrderSearchPaxSegmentType(flightSegment);
            PaxSegmentList = PaxSegmentList.Append(paxSegmentType);

            return paxSegmentType.PaxSegmentID;
        }

        private PaxSegmentType CreateOrderSearchPaxSegmentType(SegmentDetails flightSegment)
        {
            return new PaxSegmentType()
            {
                PaxSegmentID = IdCreator.CreatePaxSegmentID(PaxSegmentList.Count()),
                DatedMarketingSegmentRefId = GetOrderSearchDatedMarketingSegmentID(flightSegment, DatedMarketingSegmentList.Count()),
                CabinTypeAssociationChoice = new CabinTypeAssociationChoiceType
                {
                    Item = new CabinTypeType
                    {
                        CabinTypeCode = flightSegment?.CabinType?.CabinCode,
                        CabinTypeName = flightSegment?.CabinType?.CabinName
                    }
                },
                MarketingCarrierRBD_Code = flightSegment.ClassOfService
            };
        }

        private string GetOrderSearchDatedMarketingSegmentID(SegmentDetails flightSegment, int count)
        {
            if (DatedMarketingSegmentList.Any(x => x.CarrierDesigCode == flightSegment.MarketingCarrier && x.MarketingCarrierFlightNumberText == flightSegment.MarketingFlightNumber && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime).ToUniversalTime())))
            {
                return DatedMarketingSegmentList.Where(x => x.CarrierDesigCode == flightSegment.MarketingCarrier && x.MarketingCarrierFlightNumberText == flightSegment.MarketingFlightNumber && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime).ToUniversalTime())).Select(x => x.DatedMarketingSegmentId).First();
            }
            else
            {
                var datedMarketingSegment = CreateOrderSearchDatedMarketingSegment(flightSegment, count);
                DatedMarketingSegmentList = DatedMarketingSegmentList.Append(datedMarketingSegment);
                return datedMarketingSegment.DatedMarketingSegmentId;
            }
        }

        private DatedMarketingSegmentType CreateOrderSearchDatedMarketingSegment(SegmentDetails flightSegment, int count)
        {
            return new DatedMarketingSegmentType
            {
                DatedMarketingSegmentId = IdCreator.CreateDatedMarketingSegmentID(count),
                DatedOperatingSegmentRefId = GetOrderSearchDatedOperatingSegmentID(flightSegment, DatedOperatingSegmentList.Count()),
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = flightSegment.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = flightSegment.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierDesigCode = flightSegment.MarketingCarrier,
                CarrierName = GetCarrierName(flightSegment.MarketingCarrier),
                MarketingCarrierFlightNumberText = flightSegment.MarketingFlightNumber
            };
        }

        private string GetOrderSearchDatedOperatingSegmentID(SegmentDetails flightSegment, int count)
        {
            if (DatedOperatingSegmentList.Any(x => x.CarrierDesigCode == flightSegment.MarketingCarrier && (!string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? flightSegment.OperatingFlightNumber == x.OperatingCarrierFlightNumberText : flightSegment.MarketingFlightNumber == x.OperatingCarrierFlightNumberText)))
            {
                return DatedOperatingSegmentList.Where(x => x.CarrierDesigCode == flightSegment.MarketingCarrier && (!string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? flightSegment.OperatingFlightNumber == x.OperatingCarrierFlightNumberText : flightSegment.MarketingFlightNumber == x.OperatingCarrierFlightNumberText)).Select(x => x.DatedOperatingSegmentId).First();
            }
            else
            {
                var datedOperatingSegment = CreateOrderSearchDatedOperatingSegment(flightSegment, count);
                DatedOperatingSegmentList =  DatedOperatingSegmentList.Append(datedOperatingSegment);
                return datedOperatingSegment.DatedOperatingSegmentId;
            }
        }

        private DatedOperatingSegmentType CreateOrderSearchDatedOperatingSegment(SegmentDetails flightSegment, int count)
        {
            return new DatedOperatingSegmentType
            {
                DatedOperatingSegmentId = IdCreator.CreateDatedOperatingSegmentID(count),
                CarrierDesigCode = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                CarrierName = flightSegment.OperatingCarrier == null ? GetCarrierName(flightSegment.MarketingCarrier) : GetCarrierName(flightSegment.OperatingCarrier),
                OperatingCarrierFlightNumberText = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                Duration = !string.IsNullOrEmpty(flightSegment?.FlightDuration) ? GetOrderSearchDuration(flightSegment.FlightDuration) : null,
                DatedOperatingLegRefID = flightSegment.Legs?.Select(x => GetDatedOperatingLegRefID(x, flightSegment?.CassEquipment)).ToArray()
            };
        }

        public string GetDatedOperatingLegRefID(Legs flightLeg, string cassEquipment)
        {
            if (DatedOperatingLegList.Any(x => x.Arrival.IATA_LocationCode == flightLeg.Destination_Airport_Code && x.Dep.IATA_LocationCode == flightLeg.OriginAirportCode && x.OnGroundDuration == GetOrderSearchDuration(flightLeg.LegDuration)))
            {
                return DatedOperatingLegList.First(x => x.Arrival.IATA_LocationCode == flightLeg.Destination_Airport_Code && x.Dep.IATA_LocationCode == flightLeg.OriginAirportCode && x.OnGroundDuration == GetOrderSearchDuration(flightLeg.LegDuration)).DatedOperatingLegID;
            }
            else
            {
                var datedOperatingLeg = CreateDatedOperatingLeg(flightLeg, cassEquipment);
                DatedOperatingLegList = DatedOperatingLegList.Append(datedOperatingLeg);
                return datedOperatingLeg.DatedOperatingLegID;
            }
        }

        public DatedOperatingLegType CreateDatedOperatingLeg(Legs flightLeg, string cassEquipment)
        {
            return new DatedOperatingLegType
            {
                DatedOperatingLegID = IdCreator.CreateDatedOperatingLegID(DatedOperatingLegList.Count()),
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = flightLeg.Destination_Airport_Code,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightLeg.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = flightLeg.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightLeg.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                OnGroundDuration = flightLeg.LegDuration == null ? null : GetOrderSearchDuration(flightLeg.LegDuration),
                IATA_AircraftType = new IATA_AircraftTypeType() { IATA_AircraftTypeCode = cassEquipment },
            };
        }

        private string GetOrderSearchDuration(string timeSpan)
        {
            return timeSpan == null ? null : $"{Constants.Application.PT}{timeSpan.Split(':').First()}{Constants.Application.H}{timeSpan.Split(':').Last()}{Constants.Application.M}";
        }

        private string GetCarrierName(string code)
        {
            return code switch
            {
                "VS" => "Virgin Atlantic Airways",
                "DL" => "Delta Air Lines",
                "CA" => "Air China",
                "SQ" => "Singapore Airlines",
                "NZ" => "Air New Zealand",
                "VA" => "Virgin Australia",
                "AM" => "Aeromexico",
                "AF" => "Air France",
                "KL" => "KLM",
                "WS" => "WestJet",
                _ => default
            };
        }

        private void AddOrderSearchOriginDestination(Infrastructure.Model.Air4OrderSearchMS.OriginDestination originDest, string journeyId)
        {
            if (originDest != null && !OriginDestList.Any(x => x.DestCode == originDest.DestinationAirport && x.OriginCode == originDest.OriginAirport))
            {
                OriginDestList = OriginDestList.Append(new OriginDestType()
                {
                    OriginDestID = IdCreator.CreateOriginDestID(OriginDestList.Count() + 1),
                    OriginCode = originDest.OriginAirport,
                    DestCode = originDest.DestinationAirport,
                    PaxJourneyRefID = new string[] { journeyId }
                });
            }
        }

        private PriceType GetPrice(IEnumerable<Price> priceDetails, string baseCurrency)
        {
            var priceType = new PriceType();
            var taxBreakUpCodes = new List<string>();
            decimal totalAmount = decimal.Zero;
            decimal baseAmount = decimal.Zero;
            decimal totalTaxAmount = decimal.Zero;
            List<TaxType> taxTypeList = new();

            if (priceDetails != null)
            {
                foreach(var price in priceDetails)
                {
                    if(price.PriceComponentType == PriceComponentTypes.BasePriceEnum)
                        totalAmount += Convert.ToDecimal(price.Amount.MonetaryAmount);

                    if (price.PriceComponentType == PriceComponentTypes.BaseFareEnum)
                        baseAmount += Convert.ToDecimal(price.Amount.MonetaryAmount);

                    if (price.PriceComponentType == PriceComponentTypes.TotalTaxEnum && string.IsNullOrWhiteSpace(price.PriceComponentName))
                        totalTaxAmount += Convert.ToDecimal(price.Amount.MonetaryAmount);

                    if (price.PriceComponentType == PriceComponentTypes.TotalTaxEnum && !string.IsNullOrWhiteSpace(price.PriceComponentName))
                        taxBreakUpCodes.Add(price.PriceComponentName);
                }

                priceType.TotalAmount = (AmountType)new EAPI.Domain.Virgin.Models.Common.CurrencyAmount() { BaseCurrency = baseCurrency, MonetaryAmount = totalAmount.ToString() };
                priceType.BaseAmount = (AmountType)new EAPI.Domain.Virgin.Models.Common.CurrencyAmount() { BaseCurrency = baseCurrency, MonetaryAmount = baseAmount.ToString() };

                foreach(var tax in taxBreakUpCodes.GroupBy(x => x))
                {
                    var price = priceDetails.Where(x => x.PriceComponentName == tax.First());
                    var sum = price.Sum(x => Convert.ToDecimal(x.Amount.MonetaryAmount));
                    taxTypeList.Add((TaxType)new Price() { Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount() { MonetaryAmount = sum.ToString() }, PriceComponentName = price.First().PriceComponentName });
                }

                if (taxTypeList.Any() && totalTaxAmount != decimal.Zero)
                {
                    priceType.TaxSummary = new TaxSummaryType[1]
                    {
                        new()
                        {
                            Tax = taxTypeList.ToArray(),
                            TotalTaxAmount = (AmountType)new EAPI.Domain.Virgin.Models.Common.CurrencyAmount() { BaseCurrency = baseCurrency , MonetaryAmount = totalTaxAmount.ToString()}
                        }
                    };
                }
            }
            else
            {
                priceType.TotalAmount = new AmountType()
                {
                    CurCode = baseCurrency,
                    Value = Convert.ToDecimal(0)
                };
            }

            return priceType;
        }

        private OrderedServiceType[] GetServices(OrderSearchResponse orderRetrieveResponse, IEnumerable<OrderItems> orderItems)
        {
            var services = new List<OrderedServiceType>();
            int count = 1;

            foreach (var orderItem in orderItems)
            {
                foreach(var seg in PaxSegmentList.Select(x => x.PaxSegmentID))
                {
                    services.Add(GetServiceList(orderRetrieveResponse.CustomerDetails.FirstOrDefault(customer => customer.CustId == orderItem.Services.First()?.ServiceBeneficiaries?.First()), seg, orderRetrieveResponse, orderItem , count));
                    count++;
                }
            }

            return services.ToArray();
        }

        private OrderedServiceType GetServiceList(Customer customer, string paxSegment,  OrderSearchResponse orderRetrieveResponse, OrderItems orderItem, int count)
        {
            return new OrderedServiceType
            {
                ServiceID = IdCreator.CreateServiceID(count),
                PaxRefID = GetSearchPaxId(customer),
                BookingRef = GetBookingRefType(orderRetrieveResponse.BookingReferences),
                StatusCode = GetStatusCode(orderItem, orderRetrieveResponse, paxSegment),
                OrderServiceAssociation = new OrderServiceAssociationType()
                {
                    Item = new PaxSegmentRefType()
                    {
                        PaxSegmentRefID = paxSegment
                    }
                },
                DeliveryStatusCode = GetDeliveryStatusCode(orderItem, orderRetrieveResponse)
            };
        }

        private OrderedServiceType[] GetSeatServices(Customer customer, string pax,  OrderSearchResponse orderSearchResponse, OrderItems orderItem, int count)
        {
            return
            [
                new()
                {
                    ServiceID = IdCreator.CreateServiceID(1),
                    PaxRefID = GetSearchPaxId(customer),
                    BookingRef = GetBookingRefType(orderSearchResponse.BookingReferences),
                    OrderServiceAssociation = GetSeatServiceAssociations(orderItem.Services.FirstOrDefault(), pax),
                    StatusCode = GetStatusCode(orderItem, orderSearchResponse, pax),
                    DeliveryStatusCode = GetDeliveryStatusCode(orderItem, orderSearchResponse) 
                }
            ];
        }

        private OrderServiceAssociationType GetSeatServiceAssociations(Service service, string paxSegment)
        {
            return new OrderServiceAssociationType()
            {
                Item = new SeatOnLegType()
                {
                    Seat = new SeatType()
                    {
                        RowNumber = service.CurrentSeatAssignment[..^1],
                        ColumnID = service.CurrentSeatAssignment[^1].ToString()
                    },
                    SeatAssignmentAssociations = new SeatAssignmentAssociationsType()
                    {
                        Item = new PaxSegmentRefType()
                        {
                            PaxSegmentRefID = paxSegment
                        }
                    }
                }
            };
        }

        private BookingRefType[] GetBookingRefType(BookingReference bookingReferences)
        {
            return new BookingRefType[]
            {
                new()
                {
                    BookingID = bookingReferences?.RLOC,
                    BookingEntity = new ()
                    {
                        Item = new CarrierType
                        {
                            AirlineDesigCode = bookingReferences?.AirlineDesignator
                        }
                    }
                }
            };
        }

        private string GetStatusCode(OrderItems orderItem, OrderSearchResponse orderSearchResponse, string paxSegment)
        {
            string statusCode = string.Empty;
            var ticketInfo = orderItem.AccountableDocumentRefs?.FirstOrDefault(doc => doc.Coupons.First().SegmentRefId == orderSearchResponse.SegmentDetails.First().SegmentId);

            if (ticketInfo != null)
            {
                if (Constants.Application.FlightStatusCodeRemoved == orderSearchResponse?.SegmentDetails?.FirstOrDefault()?.Status.Code && (ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusVoid || ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusRefunded))
                    statusCode = Constants.Application.ServiceStatusCodeCancelled;
            }

            if (Constants.Application.FlightServiceWLStatusCode.Contains(orderSearchResponse?.SegmentDetails?.FirstOrDefault()?.Status.Code))
                statusCode = Constants.Application.ServiceStatusCodeWaitlisted;

            else if (Constants.Application.FlightStatusCode.Contains(orderSearchResponse?.SegmentDetails?.FirstOrDefault()?.Status.Code))
                statusCode = Constants.Application.DeliveryStatusCodeConfirmed;

            else if (Constants.Application.FlightRequestedStatusCode.Contains(orderSearchResponse?.SegmentDetails?.FirstOrDefault()?.Status.Code))
                statusCode = Constants.Application.DeliveryStatusCodeRequested;

            else
                statusCode = orderSearchResponse.SegmentDetails.FirstOrDefault(seg => paxSegment == string.Format("{0}{1}", Constants.Application.SEG, Convert.ToInt16(seg.SegmentId)))?.Status?.Description;

            return statusCode;
        }

        private string GetDeliveryStatusCode(OrderItems orderItem, OrderSearchResponse orderSearchResponse)
        {
            if (orderItem.AccountableDocumentRefs is null && Constants.Application.FlightStatusCode.Contains(orderSearchResponse.SegmentDetails.FirstOrDefault().Status.Code))
                return Constants.Application.DeliveryStatusCodeConfirmed;

            var ticketInfo = orderItem.AccountableDocumentRefs.FirstOrDefault(doc => doc.Coupons.First().SegmentRefId == orderSearchResponse.SegmentDetails.First().SegmentId);

            if (ticketInfo != null)
            {
                var ticketExpirationDate = ticketInfo.Coupons.First().TicketExpirationDate is null ? DateTime.Now : DateTime.ParseExact(ticketInfo.Coupons.First().TicketExpirationDate, "ddMMMyy", CultureInfo.InvariantCulture);

                if (Constants.Application.FlightRequestedStatusCode.Contains(orderSearchResponse.SegmentDetails.FirstOrDefault().Status.Code))
                    return Constants.Application.DeliveryStatusCodeRequested;

                else if ((Constants.Application.FlightStatusCode.Contains(orderSearchResponse.SegmentDetails.FirstOrDefault().Status.Code) || orderSearchResponse.SegmentDetails.FirstOrDefault().Status.Code == Constants.Application.FlightStatusCodeRemoved) && ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusOpen)
                    return Constants.Application.DeliveryStatusCodeReadyToProceed;

                else if (Constants.Application.FlightStatusCode.Contains(orderSearchResponse.SegmentDetails.FirstOrDefault().Status.Code) && Constants.Application.CouponStatusReadyToDeliver.Contains(ticketInfo.Coupons.First().CouponStatus))
                    return Constants.Application.DeliveryStatusCodeReadyToDeliver;

                else if (Constants.Application.FlightStatusCode.Contains(orderSearchResponse.SegmentDetails.FirstOrDefault().Status.Code) && ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusFlwn)
                    return Constants.Application.DeliveryStatusCodeDelivered;

                else if (ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusVoid || ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusRefunded)
                    return Constants.Application.DeliveryStatusCodeRemoved;

                else if (ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusVoid || ticketInfo.Coupons.First().CouponStatus == Constants.Application.CouponStatusRefunded || DateTime.Now > ticketExpirationDate)
                    return Constants.Application.DeliveryStatusCodeRemoved;
            }
            else if (orderSearchResponse.SegmentDetails.FirstOrDefault().Status.Code == Constants.Application.FlightStatusCodeRemoved)
                return Constants.Application.DeliveryStatusCodeRemoved;

            return null;
        }

        public OrderSearchResponse AddSeatOrderItems(OrderChangeResponse orderChangeResponse, OrderSearchResponse orderSearchResponse, IEnumerable<SeatDetails> seatDetailsForGds , IEnumerable<SeatTransactionId> transactionIds , IEnumerable<TransactionIdStatus> transactionStatus, PaymentRS paymentRS)
        {
            var orderIDCount = orderSearchResponse?.OrderItems?.Count();

            foreach (var orderItem in orderChangeResponse?.OrderDetails?.OrderItems)
            {
                var seatDetails = seatDetailsForGds.FirstOrDefault(x => x.AssociatedSegment.SegmentId == orderItem.SegmentRefId && x.SeatNumber == orderItem.CurrentSeatAssignment);

                var existingSeatOrderItem = orderSearchResponse.OrderItems.FirstOrDefault(x => x.Services.First().ProductOrder.ProductType == Product.ProductTypes.SeatEnum && x.Services.First().ServiceBeneficiaries.First() == seatDetails.Beneficiary.CustId && x.Services.First().ProductSegmentAssociation.FlightSegmentIds.First() == orderItem.SegmentRefId);

                if (existingSeatOrderItem != null)
                    orderSearchResponse.OrderItems = orderSearchResponse.OrderItems.Where(x => x.OrderItemId != existingSeatOrderItem.OrderItemId);

                orderSearchResponse.OrderItems = orderSearchResponse.OrderItems.Append(new()
                {
                    OrderItemId = $"{orderSearchResponse?.OrderNumber?.OrderId}-{++orderIDCount}",
                    OwnerAirline = Constants.Application.OwnerAirline,
                    Services =
                    [
                        new ()
                        {
                            ServiceId = Constants.Application.DefaultServiceID,
                            ProductOrder = new()
                            {
                                ProductOwner = Constants.Application.OwnerAirline,
                                ProductType = Product.ProductTypes.SeatEnum
                            },
                            ProductSegmentAssociation = new ()
                            {
                                FlightSegmentIds = [orderItem.SegmentRefId],
                            },
                            CurrentSeatAssignment = orderItem.CurrentSeatAssignment,
                            ServiceBeneficiaries = [seatDetails.Beneficiary.CustId]
                        }
                    ],
                    ServiceBeneficiaries = [seatDetails.Beneficiary.CustId],
                    AccountableDocumentRefs = !orderItem.AccountableDocuments.First().DocumentNumber.IsNullOrEmpty() ? new List<AccountableDocumentRef>()
                    {
                        new()
                        {
                            DocumentType = AccountableDocumentRef.DocumentTypes.YEnum,
                            DocumentNumber = orderItem.AccountableDocuments.First().DocumentNumber,
                            CustomerRefId = seatDetails?.Beneficiary?.CustId,
                            Coupons =
                            [
                                new ()
                                {
                                    CouponStatus = Constants.Application.CouponStatusOpen
                                }
                            ],
                            ReasonForIssuance = new ReasonForIssuance()
                            {
                                RFIC = Constants.Application.SeatRficCode,
                                RFISC = Constants.Application.SeatRfiscCode
                            }
                        }
                    } : null,
                    PriceDetails =
                    [
                        new ()
                        {
                            PriceComponentType = PriceComponentTypes.BasePriceEnum,
                            Amount = new ()
                            {
                                MonetaryAmount = seatDetails?.SeatPrice,
                                BaseCurrency = seatDetails?.Currency,
                            }
                        }
                    ],
                    PaymentDetails = paymentRS == null ? null :
                    [
                        new()
                        {
                            Amount = new ()
                            {
                                BaseCurrency = seatDetails.Currency,
                                MonetaryAmount = transactionStatus.FirstOrDefault(t => t.TransactionId == transactionIds.FirstOrDefault(td => td.SegmentId == orderItem.SegmentRefId).TransactionId).Amount.ToString()
                            },
                            PaymentReferenceId = transactionIds.FirstOrDefault(t =>t.TransactionId == transactionIds.FirstOrDefault(td => td.SegmentId == orderItem.SegmentRefId)?.TransactionId).TransactionId,
                            PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                            CardMethod = new()
                            {
                                CardBrandCode = paymentRS.PaymentDocument.CardType,
                                CardNumber = paymentRS.PaymentDocument.MaskedCardNumber,
                                ExpiryDate = paymentRS.PaymentDocument.ExpiryDate
                            }
                        }
                    ]
                });
            }

            return orderSearchResponse;
        }

        public static WarningType[] GetWarnings(IEnumerable<NDCWarning> warnings)
        {
            if (warnings != null && warnings.Any())
            {
                return warnings.Select(x => {
                    return new WarningType
                    {
                        Code = Convert.ToString(x.Code),
                        DescText = x.ShortText,
                        StatusText = x.Status,
                        TagText = x.TagText
                    };
                }).ToArray();
            }

            return null;
        }
    }
}
