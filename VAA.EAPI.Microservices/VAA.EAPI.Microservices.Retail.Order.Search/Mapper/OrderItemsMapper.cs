using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public class OrderItemsMapper
    {
        public static Dictionary<int, string> dtiSegmentInfo = null;
        public static List<OrderItems> Air4ToOrderSearchResponseMapper(RetrieveResponseType response, IEnumerable<Passenger> passengers, string orderId, List<OrderInstructions> orderInstructions, IProductName productName, List<OrderSearchSegmentInfo> orderSearchSegmentInfos, List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketingResponseList, bool isGDSbooking)
        {
            int orderItemId = 1;

            int ptcCount = 0;
            var orderItems = new List<OrderItems>();
            var services = new List<Service>();

            foreach (var orderItem in passengers)
            {
                var dti = response.TeardownContextResponse.Pnr?.Dti != null ? response.TeardownContextResponse.Pnr.Dti?.FirstOrDefault(x => x.ExpandedPassengerTypeCode == ExpandedPassengerTypeCode(ptcCount, response)) : null;
                orderItems.Add(new()
                {
                    AccountableDocumentRefs = GetAccountableDocumentRefs(response, orderItem, dti, orderSearchSegmentInfos, ticketingResponseList),
                    OwnerAirline = Constants.Application.VSFlightCode,
                    OrderItemId = $"{orderId}-{orderItemId++}",
                    ItemStatus = new ItemStatus()
                    {
                        Status = string.Empty,
                        TimeStamp = string.Empty
                    },
                    OfferIdReference = string.Empty,
                    OfferItemIdReference = null,
                    OrderItemPenalties = null,
                    PaymentDetails = null,
                    PNRReferences = null,
                    PriceDetails = GetPriceDetails(dti, response, ptcCount, ticketingResponseList, isGDSbooking, orderItem),
                    Services = GetServices(response, orderItem.CustomerId, null, null, orderItem.LastNameNumber, orderInstructions, productName, orderSearchSegmentInfos, isGDSbooking, ticketingResponseList),
                    ServiceBeneficiaries = new List<string>() { orderItem.CustomerId },
                    PaymentReferenceIDs = new List<string>()
                });
                ptcCount++;
            }

            if (response.TeardownContextResponse?.Pnr?.SeatDl != null)
            {
                foreach (var seat in response.TeardownContextResponse.Pnr.SeatDl)
                {
                    foreach (var passenger in passengers)
                    {
                        services = new List<Service>();
                        string passengerFullName = passenger.LastName + "/" + passenger.FirstName;
                        if (passengerFullName.Equals(seat.PassengerName))
                        {
                            orderItems.Add(new()
                            {
                                AccountableDocumentRefs = null,
                                OwnerAirline = Constants.Application.VSFlightCode,
                                OrderItemId = $"{orderId}-{orderItemId++}",
                                ItemStatus = new ItemStatus()
                                {
                                    Status = string.Empty,
                                    TimeStamp = string.Empty
                                },
                                OfferIdReference = string.Empty,
                                OfferItemIdReference = null,
                                OrderItemPenalties = null,
                                PaymentDetails = null,
                                PNRReferences = null,
                                PriceDetails = null,
                                Services = GetServices(response, passenger.CustomerId, seat.CurrentSeatAssignment, seat.SegmentNumber, passenger.LastNameNumber, orderInstructions, productName, orderSearchSegmentInfos, isGDSbooking, ticketingResponseList),
                                ServiceBeneficiaries = new List<string>() { passenger.CustomerId },
                                PaymentReferenceIDs = new List<string>()
                            });
                        }

                    }

                }
            }
            return orderItems;
        }

        private static string ExpandedPassengerTypeCode(int ptcCount, RetrieveResponseType response)
        {
            string ptc = response.TeardownContextResponse.Pnr.Ptc != null ? response.TeardownContextResponse.Pnr.Ptc.Any(x => x.ApplyToAllFlag == "Y") ? response.TeardownContextResponse.Pnr.Ptc.First().PassengerTypeCode : response.TeardownContextResponse.Pnr.Ptc[ptcCount].PassengerTypeCode : string.Empty;
            return ptc switch
            {
                "A" => PtcEnum.ADT.ToString(),
                "Y" => PtcEnum.GBE.ToString(),
                "B" => PtcEnum.INF.ToString(),
                "C" => PtcEnum.CNN.ToString(),
                _ => null,
            };

        }

        private static List<AccountableDocumentRef> GetAccountableDocumentRefs(RetrieveResponseType response, Passenger passenger, Dti dti, List<OrderSearchSegmentInfo> orderSearchSegmentInfos, List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketingResponseList)
        {
            var documents = new List<AccountableDocumentRef>();
            var coupons = new List<Coupon>();
            var ticketData = response.TeardownContextResponse.Pnr.TicketData != null ? response.TeardownContextResponse.Pnr.TicketData.Any(x => x.LastNameNumber == passenger.LastNameNumber) ? response.TeardownContextResponse.Pnr.TicketData.Count(x => x.LastNameNumber == passenger.LastNameNumber) > 1 &&  response.TeardownContextResponse.Pnr.TicketData.Any(x => x.LastNameNumber == passenger.LastNameNumber && x.FirstNameNumber == passenger.FirstNameNumber) ? response.TeardownContextResponse.Pnr.TicketData.First(x => x.LastNameNumber == passenger.LastNameNumber && x.FirstNameNumber == passenger.FirstNameNumber) : response.TeardownContextResponse.Pnr.TicketData.First(x => x.LastNameNumber == passenger.LastNameNumber) : null : null;
            var ticketInformation = response.TeardownContextResponse.Pnr.TicketInformation != null ? response.TeardownContextResponse.Pnr.TicketInformation.Any(x => x.LastNameNumber == passenger.LastNameNumber) ? response.TeardownContextResponse.Pnr.TicketInformation.Count(x => x.LastNameNumber == passenger.LastNameNumber) > 1 && response.TeardownContextResponse.Pnr.TicketInformation.Any(x => x.LastNameNumber == passenger.LastNameNumber && x.FirstNameNumber == passenger.FirstNameNumber) ? response.TeardownContextResponse.Pnr.TicketInformation.First(x => x.LastNameNumber == passenger.LastNameNumber && x.FirstNameNumber == passenger.FirstNameNumber) : response.TeardownContextResponse.Pnr.TicketInformation.First(x => x.LastNameNumber == passenger.LastNameNumber) : null : null;
            var originalTicketInfo = response.TeardownContextResponse.Pnr.TicketNumberData1 != null ? response.TeardownContextResponse.Pnr.TicketNumberData1.Any(x => x.LastNameNumber == passenger.LastNameNumber) ? response.TeardownContextResponse.Pnr.TicketNumberData1.Count(x => x.LastNameNumber == passenger.LastNameNumber) > 1 && response.TeardownContextResponse.Pnr.TicketNumberData1.Any(x => x.LastNameNumber == passenger.LastNameNumber && x.FirstNameNumber == passenger.FirstNameNumber) ? response.TeardownContextResponse.Pnr.TicketNumberData1.First(x => x.LastNameNumber == passenger.LastNameNumber && x.FirstNameNumber == passenger.FirstNameNumber) : response.TeardownContextResponse.Pnr.TicketNumberData1.First(x => x.LastNameNumber == passenger.LastNameNumber) : null : null;

            if (ticketData != null)
            {
                if (orderSearchSegmentInfos != null && orderSearchSegmentInfos.Select(s => s.DtiSegNo).Distinct().Count() <= 4)
                {
                    foreach (var segmentInfo in orderSearchSegmentInfos)
                    {
                        string segmentNo = segmentInfo.SegDlSegNo;
                        var segmentDl = response.TeardownContextResponse.Pnr.SegmentDl?.FirstOrDefault(seg => seg.SegmentNumber == segmentNo);
                        var segmentOa = response.TeardownContextResponse.Pnr.SegmentOa?.FirstOrDefault(seg => seg.SegmentNumber == segmentNo);
                        GetCouponInfo(response, passenger, dti, segmentDl, coupons, segmentOa, orderSearchSegmentInfos, ticketingResponseList, ticketData.TicketNumber);
                    }
                    documents.Add(new AccountableDocumentRef()
                    {
                        DocumentNumber = ticketData.TicketNumber ?? string.Empty,
                        DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                        CustomerRefId = passenger?.CustomerId,
                        AllFlown = !string.IsNullOrEmpty(ticketInformation?.TypeDescription) && ticketInformation.TypeDescription == Constants.Application.FLWNTypeDescription ? true : false,
                        DocumentIssuer = Constants.Application.VSFlightCode,
                        Coupons = coupons,
                        IssueDate = response.TeardownContextResponse.Pnr.TicketData.Any(x => x.LastNameNumber == passenger.LastNameNumber) ? DateTimeHelper.ConvertDateTimeFormat(response.TeardownContextResponse.Pnr.TicketData.First(x => x.LastNameNumber == passenger.LastNameNumber).TicketIssueDate, "ddMMMyy", string.Empty)?.ToString(Constants.Application.DateFormat) : null,
                        FareCalculation = response.TeardownContextResponse.Pnr?.FareCalc != null ? response.TeardownContextResponse.Pnr.FareCalc.Any(x => x.FreeformText.StartsWith(dti.PassengerTypeCode)) ? response.TeardownContextResponse.Pnr.FareCalc.First(x => x.FreeformText.StartsWith(dti.PassengerTypeCode)).FreeformText.Remove(0, 2) : null : null,
                        OriginalTicketDocInfo = new OriginalTicketDocInfo
                        {
                            DocumentNumber = originalTicketInfo?.OriginalTicketNumber.Split('-')[0],
                            IssueDate = DateTimeHelper.ConvertDateTimeFormat(originalTicketInfo?.OriginalTicketDate, "ddMMMyy", string.Empty)?.ToString(Constants.Application.DateFormat)
                        }
                    });

                    return documents;
                }
                else if (orderSearchSegmentInfos.Count() > 4)
                {
                    int couponCount = 0;
                    var primaryCoupons = new List<Coupon>();
                    foreach (var segmentInfo in orderSearchSegmentInfos)
                    {
                        bool isScSeg = false;
                        string segmentNo = segmentInfo.SegDlSegNo;
                        var segmentDl = response.TeardownContextResponse.Pnr.SegmentDl?.FirstOrDefault(seg => seg.SegmentNumber == segmentNo);
                        var segmentOa = response.TeardownContextResponse.Pnr.SegmentOa?.FirstOrDefault(seg => seg.SegmentNumber == segmentNo);
                        isScSeg = segmentDl?.CurrentActionCode == Constants.Application.ScheduleChangeStatusCode || segmentOa?.CurrentActionCode == Constants.Application.ScheduleChangeStatusCode ? true : false;
                        if (!isScSeg)
                        {
                            if (couponCount < 4)
                            {
                                couponCount++;
                                GetCouponInfo(response, passenger, dti, segmentDl, primaryCoupons, segmentOa, orderSearchSegmentInfos, ticketingResponseList, ticketData.TicketNumber);
                            }
                            else
                            {
                                couponCount++;
                                GetCouponInfo(response, passenger, dti, segmentDl, coupons, segmentOa, orderSearchSegmentInfos, ticketingResponseList, ticketData.TicketNumber);
                            }
                        }

                    }

                    documents.Add(new AccountableDocumentRef()
                    {
                        DocumentNumber = ticketData.TicketNumber ?? string.Empty,
                        DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                        Primary = true,
                        CustomerRefId = passenger?.CustomerId,
                        AllFlown = !string.IsNullOrEmpty(ticketInformation?.TypeDescription) && ticketInformation.TypeDescription == Constants.Application.FLWNTypeDescription ? true : false,
                        DocumentIssuer = Constants.Application.VSFlightCode,
                        Coupons = primaryCoupons,
                        IssueDate = response.TeardownContextResponse.Pnr.TicketData.Any(x => x.LastNameNumber == passenger.LastNameNumber) ? DateTimeHelper.ConvertDateTimeFormat(response.TeardownContextResponse.Pnr.TicketData.First(x => x.LastNameNumber == passenger.LastNameNumber).TicketIssueDate, "ddMMMyy", string.Empty)?.ToString(Constants.Application.DateFormat) : null,
                        FareCalculation = response.TeardownContextResponse.Pnr?.FareCalc != null ? response.TeardownContextResponse.Pnr.FareCalc.Any(x => x.FreeformText.StartsWith(dti.PassengerTypeCode)) ? response.TeardownContextResponse.Pnr.FareCalc.First(x => x.FreeformText.StartsWith(dti.PassengerTypeCode)).FreeformText.Remove(0, 2) : null : null,
                        OriginalTicketDocInfo = new OriginalTicketDocInfo
                        {
                            DocumentNumber = originalTicketInfo.OriginalTicketNumber,
                            IssueDate = DateTimeHelper.ConvertDateTimeFormat(originalTicketInfo.OriginalTicketDate, "ddMMMyy", string.Empty)?.ToString(Constants.Application.DateFormat)
                        }
                    });

                    string ssrRemarkInfo = null;

                    if (response.TeardownContextResponse.Pnr.SpecialServiceRequest != null)
                    {
                        ssrRemarkInfo = response.TeardownContextResponse.Pnr.SpecialServiceRequest.Any(ssr => ssr.LastNameNumber == passenger.LastNameNumber && ssr.FirstNameNumber == passenger.FirstNameNumber && ssr.RequestCode == Constants.Application.TicketRemarkCode && ssr.Remarks.Contains('/') && !ssr.Remarks.Contains(ticketData.TicketNumber)) ? response.TeardownContextResponse.Pnr.SpecialServiceRequest?.FirstOrDefault(ssr => ssr.LastNameNumber == passenger.LastNameNumber && ssr.FirstNameNumber == passenger.FirstNameNumber && ssr.RequestCode == "TKNE" && ssr.Remarks.Contains('/') && !ssr.Remarks.Contains(ticketData.TicketNumber)).Remarks.Substring(1, 13) : null;
                    }

                    documents.Add(new AccountableDocumentRef()
                    {
                        DocumentNumber = ssrRemarkInfo ?? ticketData.TicketNumber,
                        DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                        CustomerRefId = passenger?.CustomerId,
                        AllFlown = !string.IsNullOrEmpty(ticketInformation?.TypeDescription) && ticketInformation.TypeDescription == Constants.Application.FLWNTypeDescription ? true : false,
                        DocumentIssuer = Constants.Application.VSFlightCode,
                        Coupons = coupons,
                        IssueDate = response.TeardownContextResponse.Pnr.TicketData.Any(x => x.LastNameNumber == passenger.LastNameNumber) ? DateTimeHelper.ConvertDateTimeFormat(response.TeardownContextResponse.Pnr.TicketData.First(x => x.LastNameNumber == passenger.LastNameNumber).TicketIssueDate, "ddMMMyy", string.Empty)?.ToString(Constants.Application.DateFormat) : null,
                        FareCalculation = response.TeardownContextResponse.Pnr?.FareCalc != null ? response.TeardownContextResponse.Pnr.FareCalc.Any(x => x.FreeformText.StartsWith(dti.PassengerTypeCode)) ? response.TeardownContextResponse.Pnr.FareCalc.First(x => x.FreeformText.StartsWith(dti.PassengerTypeCode)).FreeformText.Remove(0, 2) : null : null
                    });

                    return documents;
                }
                else
                    return null;
            }
            else
                return null;
        }

        private static void GetCouponInfo(RetrieveResponseType response, Passenger passenger, Dti dti, SegmentDl segmentDl, List<Coupon> coupons, SegmentOa segmentOa, List<OrderSearchSegmentInfo> orderSearchSegmentInfos, List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketingResponseList, string ticketNumber)
        {
            TicketCouponInfo ticketCouponInfo = null;
            string farebasisFareclassCode = null;
            string segmentNumber = string.Empty;
            string segmentRefId = string.Empty;
            string couponStatus = string.Empty;

            if (segmentDl != null)
            {
                segmentRefId = orderSearchSegmentInfos.First(s => s.SegDlSegNo == segmentDl.SegmentNumber).SegDlSegNo;
                segmentNumber = orderSearchSegmentInfos.First(s => s.SegDlSegNo == segmentDl.SegmentNumber).DtiSegNo;

                if (response.TeardownContextResponse.Pnr.TicketCouponInfo != null)
                {
                    ticketCouponInfo = response.TeardownContextResponse.Pnr.TicketCouponInfo.Any(x => x.ArrivalPoint == segmentDl.ArrivalPoint && x.DeparturePoint == segmentDl.DeparturePoint && x.LastNameNumber == passenger.LastNameNumber && x.TravelDate == segmentDl.DepartureDate + segmentDl.SegmentDepartureYear) ? response.TeardownContextResponse.Pnr.TicketCouponInfo.First(x => x.ArrivalPoint == segmentDl.ArrivalPoint && x.DeparturePoint == segmentDl.DeparturePoint && x.LastNameNumber == passenger.LastNameNumber && x.TravelDate == segmentDl.DepartureDate + segmentDl.SegmentDepartureYear) : null;
                }
            }
            else
            {
                segmentRefId = orderSearchSegmentInfos.First(s => s.SegDlSegNo == segmentOa.SegmentNumber).SegDlSegNo;
                segmentNumber = orderSearchSegmentInfos.First(s => s.SegDlSegNo == segmentOa.SegmentNumber).DtiSegNo;

                if (response.TeardownContextResponse.Pnr.TicketCouponInfo != null)
                {
                    ticketCouponInfo = response.TeardownContextResponse.Pnr.TicketCouponInfo.Any(x => x.ArrivalPoint == segmentOa.ArrivalPoint && x.DeparturePoint == segmentOa.DeparturePoint && x.LastNameNumber == passenger.LastNameNumber && x.TravelDate == segmentOa.DepartureDate + segmentOa.SegmentDepartureYear) ? response.TeardownContextResponse.Pnr.TicketCouponInfo.First(x => x.ArrivalPoint == segmentOa.ArrivalPoint && x.DeparturePoint == segmentOa.DeparturePoint && x.LastNameNumber == passenger.LastNameNumber && x.TravelDate == segmentOa.DepartureDate + segmentOa.SegmentDepartureYear) : null;
                }

            }

            var dtiSegmentInfo = response.TeardownContextResponse.Pnr.DtiSegmentInfo != null ? response.TeardownContextResponse.Pnr.DtiSegmentInfo.FirstOrDefault(x => x.ExpandedPassengerTypeCode == dti.ExpandedPassengerTypeCode && x.SegmentNumber == segmentNumber) : null;
            farebasisFareclassCode = !string.IsNullOrEmpty(dtiSegmentInfo?.FarebasisFareclassCode) ? dtiSegmentInfo.FarebasisFareclassCode : null;

            //For a GDS booking flow
            if (!string.IsNullOrEmpty(response.TeardownContextResponse.Pnr.RecordLocator.First().OutsideSystemRecordLocator))
            {
                if (segmentDl is null)
                {
                    farebasisFareclassCode = ticketingResponseList.SelectMany(ticketRes => ticketRes.TicketRequestResponse.First().TicketResponse.First().TicketCoupon).FirstOrDefault(coupon =>
                                        coupon.TicketCouponDestinationAirport == segmentOa?.ArrivalPoint && coupon.TickeCouponOriginatingAirport == segmentOa?.DeparturePoint &&
                                        coupon.TicketLegDepartureDate == $"{segmentOa?.DepartureDate}{segmentOa?.SegmentDepartureYear}")?.FarebasisCode;
                    segmentRefId = orderSearchSegmentInfos.FirstOrDefault(s => s.SegDlSegNo == segmentOa?.SegmentNumber)?.SegDlSegNo;
                }
                else
                {
                    farebasisFareclassCode = ticketingResponseList.SelectMany(ticketRes => ticketRes.TicketRequestResponse.First().TicketResponse.First().TicketCoupon).FirstOrDefault(coupon =>
                                        coupon.TicketCouponDestinationAirport == segmentDl.ArrivalPoint && coupon.TickeCouponOriginatingAirport == segmentDl.DeparturePoint &&
                                        coupon.TicketLegDepartureDate == $"{segmentDl.DepartureDate}{segmentDl.SegmentDepartureYear}")?.FarebasisCode;
                    segmentRefId = orderSearchSegmentInfos.FirstOrDefault(s => s.SegDlSegNo == segmentDl.SegmentNumber)?.SegDlSegNo;
                }
            }

            if (ticketCouponInfo == null)
            {
                var ticketHistory = ticketingResponseList.FirstOrDefault(tktResponse => tktResponse?.TicketRequestResponse?.FirstOrDefault()?.TicketRequest?.ElectronicDocumentNumber == ticketNumber)?.TicketRequestResponse?.FirstOrDefault()?.TicketResponse?.FirstOrDefault()?.AlternateCouponFltHistory;
                var ticketHistoryCouponInfo = ticketHistory?.FirstOrDefault(x => x?.FlightDestinationAirport == (segmentDl?.ArrivalPoint ?? segmentOa.ArrivalPoint) && x?.CityCode == (segmentDl?.DeparturePoint ?? segmentOa.DeparturePoint) && x?.FlightDepartureDate == segmentDl.DepartureDate + segmentDl.SegmentDepartureYear);

                if (ticketHistoryCouponInfo?.CouponStatus != null)
                {
                    if (ticketHistoryCouponInfo.StatusCode.Contains(Constants.Application.CodeShareCouponPrefix))
                        couponStatus = Constants.Application.OpenStatus;
                    else
                        couponStatus = ticketHistoryCouponInfo.StatusCode;
                }

                coupons.Add(new Coupon()
                {
                    TicketExpirationDate = string.IsNullOrEmpty(couponStatus) ? null : ticketingResponseList.SelectMany(trl => trl.TicketRequestResponse).SelectMany(trr => trr.TicketResponse).Where(x => x.AdditionalTicketLevelData != null).SelectMany(tr => tr.AdditionalTicketLevelData)?.FirstOrDefault(atd => atd.CustomerId == passenger.CustomerId)?.TicketExpirationDate,
                    CouponNumber = ticketHistoryCouponInfo?.CouponSequenceNumber ?? string.Empty,
                    CouponStatus = couponStatus,
                    SegmentRefId = segmentRefId,
                    FareBasis = farebasisFareclassCode != null ? farebasisFareclassCode.Contains("/") ? farebasisFareclassCode.Split("/")[0] : farebasisFareclassCode : null,
                    Designator = farebasisFareclassCode != null ? farebasisFareclassCode.Contains("/") ? farebasisFareclassCode.Split("/")[1] : null : null,
                });
            }
            else
            {
                if (ticketCouponInfo?.CouponStatus != null)
                {
                    if (ticketCouponInfo.CouponStatus.Contains(Constants.Application.CodeShareCouponPrefix))
                        couponStatus = Constants.Application.OpenStatus;
                    else
                        couponStatus = ticketCouponInfo.CouponStatus;
                }

                coupons.Add(new Coupon()
                {
                    TicketExpirationDate = string.IsNullOrEmpty(couponStatus) ? null : ticketingResponseList.SelectMany(trl => trl.TicketRequestResponse).SelectMany(trr => trr.TicketResponse).Where(x => x.AdditionalTicketLevelData != null).SelectMany(tr => tr.AdditionalTicketLevelData)?.FirstOrDefault(atd => atd.CustomerId == passenger.CustomerId)?.TicketExpirationDate,
                    CouponNumber = ticketCouponInfo?.CouponNumber ?? string.Empty,
                    CouponStatus = couponStatus,
                    SegmentRefId = segmentRefId,
                    CheckDigit = ticketCouponInfo?.CheckDigit ?? string.Empty,
                    FareBasis = farebasisFareclassCode != null ? farebasisFareclassCode.Contains("/") ? farebasisFareclassCode.Split("/")[0] : farebasisFareclassCode : null,
                    Designator = farebasisFareclassCode != null ? farebasisFareclassCode.Contains("/") ? farebasisFareclassCode.Split("/")[1] : null : null,
                });
            }
        }

        private static List<Service> GetServices(RetrieveResponseType response, string customerId, string currentSeatAssignment, string segNo, string Lastnamenumber, List<OrderInstructions> orderInstructions, IProductName productName, List<OrderSearchSegmentInfo> orderSearchSegmentInfos, bool isGDSbooking, List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketingResponseList)
        {
            var services = new List<Service>();
            int serviceId = 1;
            string fareBasisCode = "";
            string compartmentCode = "";

            if (response.TeardownContextResponse.Pnr.SegmentDl != null || response.TeardownContextResponse.Pnr.SegmentOa != null)
            {
                if (currentSeatAssignment == null)
                {
                    if (response.TeardownContextResponse.Pnr.DtiSegmentInfo != null)
                        dtiSegmentInfo = GetAllDtiSegmentDetails(response.TeardownContextResponse.Pnr.DtiSegmentInfo.GroupBy(item => item.SegmentNumber).Select(q => q.FirstOrDefault()).Where(item => item.SegmentNumber != null));
                    foreach (var segmentInfo in orderSearchSegmentInfos)
                    {
                        string segmentNumber = segmentInfo.SegDlSegNo;
                        var segmentDl = response.TeardownContextResponse.Pnr.SegmentDl?.FirstOrDefault(seg => seg.SegmentNumber == segmentNumber);
                        var segmentOa = response.TeardownContextResponse.Pnr.SegmentOa?.FirstOrDefault(seg => seg.SegmentNumber == segmentNumber);

                        List<string> orderInstructionsRef = new();
                        if (segmentDl != null)
                        {
                            orderInstructionsRef = segmentDl.CurrentActionCode == Constants.Application.ScheduleChangeStatusCode ? null : GetOrderInstructionRefs(response, Lastnamenumber, segmentDl.FlightNumber, orderInstructions);
                            if (isGDSbooking)
                            {
                                fareBasisCode = ticketingResponseList.SelectMany(ticketRes => ticketRes.TicketRequestResponse.First().TicketResponse.First().TicketCoupon).FirstOrDefault(coupon =>
                                    coupon.TicketCouponDestinationAirport == segmentDl.ArrivalPoint && coupon.TickeCouponOriginatingAirport == segmentDl.DeparturePoint &&
                                    coupon.TicketLegDepartureDate == $"{segmentDl.DepartureDate}{segmentDl.SegmentDepartureYear}")?.FarebasisCode;
                                compartmentCode = ticketingResponseList.First().TicketRequestResponse.First().TicketResponse.First()?.AdditionalCouponLevel?.FirstOrDefault().CompartmentCode;
                            }
                        }
                        else
                        {
                            orderInstructionsRef = segmentOa.CurrentActionCode == Constants.Application.ScheduleChangeStatusCode ? null : GetOrderInstructionRefs(response, Lastnamenumber, segmentOa.FlightNumber, orderInstructions);

                        }
                        services.Add(new()
                        {
                            ServiceId = serviceId++.ToString(),
                            ProductOrder = new ProductOrder()
                            {
                                ProductOwner = Constants.Application.VSFlightCode,
                                ProductCode = string.Empty,
                                ProductName = segmentDl != null && segmentDl.CurrentActionCode != Constants.Application.ScheduleChangeStatusCode ? GetProductName(segmentInfo.DtiSegNo, segmentDl.CompartmentNumber, productName, fareBasisCode, isGDSbooking, compartmentCode) : string.Empty,
                                ProductType = ProductOrder.ProductTypes.FlightEnum
                            },
                            PriceDetails = null,
                            ProductSegmentAssociation = new ProductSegmentAssociation()
                            {
                                FlightSegmentIds = new List<string>() { segmentNumber }
                            },
                            ServiceBeneficiaries = new List<string>() { customerId },
                            OrderInstructionRefs = orderInstructionsRef,
                            ServiceStatus = null,
                        });
                    }
                }
                else if (currentSeatAssignment != null)
                {
                    services.Add(new()
                    {
                        ServiceId = serviceId.ToString(),
                        ProductOrder = new ProductOrder()
                        {
                            ProductOwner = Constants.Application.VSFlightCode,
                            ProductCode = string.Empty,
                            ProductName = string.Empty,
                            ProductType = ProductOrder.ProductTypes.SeatEnum,
                        },
                        PriceDetails = null,
                        ProductSegmentAssociation = new ProductSegmentAssociation()
                        {
                            FlightSegmentIds = new List<string>() { segNo }
                        },
                        CurrentSeatAssignment = currentSeatAssignment,
                        ServiceBeneficiaries = new List<string>() { customerId },
                        ServiceStatus = null,
                    });
                }
                return services;
            }
            else
                return new();
        }

        private static List<PriceDetail> GetPriceDetails(Dti dti, RetrieveResponseType response, int ptcCount, List<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType> ticketingResponseList, bool isGDSbooking, Passenger passenger)
        {
            if (dti != null)
            {
                List<PriceDetail> priceDetails = new()
                {
                new PriceDetail()
                {
                    Amount = new Amount()
                    {
                        BaseCurrency = string.IsNullOrEmpty(dti?.EquivalentFarePaidCurrency) ? dti?.BaseCurrency : dti?.EquivalentFarePaidCurrency,
                        MonetaryAmount = string.IsNullOrEmpty(dti?.EquivalentFarePaidAmount) ?  dti?.BaseAmount :dti?.EquivalentFarePaidAmount
                    },
                    BaseCurrencyAmount = new BaseCurrencyAmount()
                    {
                        Amount = dti?.BaseAmount,
                        Currency = dti?.BaseCurrency
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = PriceDetail.PriceComponentTypes.BaseFareEnum
                },
                new PriceDetail()
                {
                    Amount = new Amount()
                    {
                        BaseCurrency = dti.TotalFareCurrency,
                        MonetaryAmount = dti.TotalTax
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = PriceDetail.PriceComponentTypes.TotalTaxEnum
                },
                new PriceDetail()
                {
                    Amount = new Amount()
                    {
                         BaseCurrency = dti.TotalFareCurrency,
                         MonetaryAmount = dti.TotalFareAmount
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = PriceDetail.PriceComponentTypes.BasePriceEnum
                }
            };

                foreach (var dtis in response.TeardownContextResponse.Pnr.Dti.Where(x => x.ExpandedPassengerTypeCode == ExpandedPassengerTypeCode(ptcCount, response)))
                {
                    if (dtis.Tax1Amount != null)
                    {
                        priceDetails.Add(new PriceDetail()
                        {
                            PriceComponentType = PriceDetail.PriceComponentTypes.TotalTaxEnum,
                            PriceComponentName = dtis.Tax1Type,
                            Amount = new Amount()
                            {
                                BaseCurrency = string.IsNullOrEmpty(dti?.EquivalentFarePaidCurrency) ? dti?.BaseCurrency : dti?.EquivalentFarePaidCurrency,
                                MonetaryAmount = dtis.Tax1Amount
                            },

                        });
                    }
                    if (dtis.Tax2Amount != null)
                    {
                        priceDetails.Add(new PriceDetail()
                        {
                            PriceComponentType = PriceDetail.PriceComponentTypes.TotalTaxEnum,
                            PriceComponentName = dtis.Tax2Type,
                            Amount = new Amount()
                            {
                                BaseCurrency = string.IsNullOrEmpty(dti?.EquivalentFarePaidCurrency) ? dti?.BaseCurrency : dti?.EquivalentFarePaidCurrency,
                                MonetaryAmount = dtis.Tax2Amount
                            },

                        });
                    }
                    if (dtis.Tax3Amount != null)
                    {
                        priceDetails.Add(new PriceDetail()
                        {
                            PriceComponentType = PriceDetail.PriceComponentTypes.TotalTaxEnum,
                            PriceComponentName = dtis.Tax3Type,
                            Amount = new Amount()
                            {
                                BaseCurrency = string.IsNullOrEmpty(dti?.EquivalentFarePaidCurrency) ? dti?.BaseCurrency : dti?.EquivalentFarePaidCurrency,
                                MonetaryAmount = dtis.Tax3Amount
                            },

                        });
                    }
                }

                return priceDetails;
            }

            if (isGDSbooking && ticketingResponseList.Any())
            {
                var ticketNumber = response.TeardownContextResponse.Pnr.TicketData.First(ticket => ticket.FirstNameNumber == passenger.FirstNameNumber && ticket.LastNameNumber == passenger.LastNameNumber).TicketNumber;
                var passengerTicket = ticketingResponseList.First(ticketRetrieve => ticketRetrieve.TicketRequestResponse.First().TicketRequest.ElectronicDocumentNumber == ticketNumber).TicketRequestResponse.First().TicketResponse.First();

                List<PriceDetail> priceDetails = new()
                {
                    new PriceDetail()
                    {
                        Amount = new Amount()
                        {
                            BaseCurrency = string.IsNullOrEmpty(passengerTicket?.TicketInformation?.CurrencyCodeOfEquivalentFare) ? passengerTicket?.TicketInformation?.CurrencyCodeOfBaseFare : passengerTicket?.TicketInformation?.CurrencyCodeOfEquivalentFare,
                            MonetaryAmount = string.IsNullOrEmpty(passengerTicket?.TicketInformation?.EquivalentFare) ?  passengerTicket?.TicketInformation?.BaseFare : passengerTicket?.TicketInformation?.EquivalentFare
                        },
                        BaseCurrencyAmount = new BaseCurrencyAmount()
                        {
                            Amount =  passengerTicket?.TicketInformation?.BaseFare,
                            Currency = passengerTicket?.TicketInformation?.CurrencyCodeOfBaseFare
                        },
                        PriceComponentName = string.Empty,
                        PriceComponentType = PriceDetail.PriceComponentTypes.BaseFareEnum
                    },
                    new PriceDetail()
                    {
                        Amount = new Amount()
                        {
                            BaseCurrency = passengerTicket?.TotalTaxAmount?.TotalTaxCurrencyCode,
                            MonetaryAmount = passengerTicket?.TotalTaxAmount?.TotalTaxAmount
                        },
                        PriceComponentName = string.Empty,
                        PriceComponentType = PriceDetail.PriceComponentTypes.TotalTaxEnum
                    },
                    new PriceDetail()
                    {
                        Amount = new Amount()
                        {
                            BaseCurrency = passengerTicket?.TicketInformation?.CurrencyCodeOfTotalFare,
                            MonetaryAmount = passengerTicket?.TicketInformation?.TotalFare
                        },
                        PriceComponentName = string.Empty,
                        PriceComponentType = PriceDetail.PriceComponentTypes.BasePriceEnum
                    }
                };

                foreach (var dtis in passengerTicket?.DTITaxItem)
                {
                    priceDetails.Add(new PriceDetail()
                    {
                        PriceComponentType = PriceDetail.PriceComponentTypes.TotalTaxEnum,
                        PriceComponentName = dtis.TaxCode,
                        Amount = new Amount()
                        {
                            BaseCurrency = dtis.TaxAmountCurrencyCode,
                            MonetaryAmount = dtis.TaxAmount
                        },
                    });
                }

                return priceDetails;
            }

            return null;
        }

        private static List<string> GetOrderInstructionRefs(RetrieveResponseType response, string LastnameNumber, string FlightNumber, List<OrderInstructions> orderInstructions)

        {
            List<string> orderInstructionRefIds = new();
            if (response.TeardownContextResponse.Pnr.SpecialServiceRequest != null)
            {
                foreach (var item in response.TeardownContextResponse.Pnr.SpecialServiceRequest.Where(item => item.LastNameNumber == LastnameNumber))
                {
                    //child case only - Item.FlighNumber will be NULL in case of Child, so using AllSegmentsFlag to validate
                    if (item.AllSegmentsFlag == Constants.Application.AllSegmentsFlag)
                    {
                        var orderInstructionId = orderInstructions.FirstOrDefault(q => q.ItemText == item.Remarks && q.ItemCode == item.RequestCode && q.Status == item.StatusCode)?.InstructionId;
                        if (!orderInstructionRefIds.Contains(orderInstructionId))
                            orderInstructionRefIds.Add(orderInstructionId);
                    }
                    //Non-child scenraio- Item.FlighNumber will not be NULL 
                    else if (item.FlightNumber != null && (item.FlightNumber == FlightNumber || item.FlightNumber.PadLeft(4, '0') == FlightNumber))
                    {
                        var orderInstructionId = orderInstructions.FirstOrDefault(q => q.ItemText == item.Remarks && q.ItemCode == item.RequestCode && q.Status == item.StatusCode)?.InstructionId;
                        if (!orderInstructionRefIds.Contains(orderInstructionId))
                            orderInstructionRefIds.Add(orderInstructionId);
                    }
                }
            }
            return orderInstructionRefIds;
        }
        private static Dictionary<int, string> GetAllDtiSegmentDetails(IEnumerable<DtiSegmentInfo> dtiSegmentInfos)
        {
            Dictionary<int, string> segmentDetails = new();
            foreach (DtiSegmentInfo item in dtiSegmentInfos)
                segmentDetails.Add(Convert.ToInt32(item.SegmentNumber), item.FarebasisFareclassCode);
            return segmentDetails;
        }
        private static string GetProductName(string segmentNumber, string compartmentNumber, IProductName productNameRef, string ticketingfareBasisCode, bool isGDSbooking, string compartmentCode)
        {
            if (dtiSegmentInfo != null)
            {
                string fareBasisCode = dtiSegmentInfo.ContainsKey(Convert.ToInt32(segmentNumber)) ? dtiSegmentInfo[Convert.ToInt32(segmentNumber)] : null;
                if (!string.IsNullOrEmpty(fareBasisCode) && fareBasisCode.Length >= Constants.Application.FareBasisCodeLength && !string.IsNullOrEmpty(compartmentNumber))
                {
                    return productNameRef.GetProductName(string.Concat(compartmentNumber, fareBasisCode[Constants.Application.FareBasisCodeCharLength]).ToUpper()).Result;
                }
            }
            if (isGDSbooking)
            {
                if (!string.IsNullOrEmpty(ticketingfareBasisCode) && ticketingfareBasisCode.Length >= Constants.Application.FareBasisCodeLength && !string.IsNullOrEmpty(compartmentNumber))
                {
                    var productName = productNameRef.GetProductName(string.Concat(compartmentNumber, ticketingfareBasisCode[Constants.Application.FareBasisCodeCharLength]).ToUpper()).Result;
                    return !string.IsNullOrEmpty(productName) ? productName : GetProductCode(compartmentCode);
                }
            }
            return null;
        }

        private static string GetProductCode(string compartmentCode)
        {
            return compartmentCode switch
            {
                Constants.Application.CompartmentCodeC => Constants.Application.UpperCabin,
                Constants.Application.CompartmentCodeJ => Constants.Application.UpperCabin,
                Constants.Application.CompartmentCodeW => Constants.Application.PremiumCabin,
                Constants.Application.CompartmentCodeY => Constants.Application.EconomyClassicCabin,
                _ => null,
            };
        }
    }
}
