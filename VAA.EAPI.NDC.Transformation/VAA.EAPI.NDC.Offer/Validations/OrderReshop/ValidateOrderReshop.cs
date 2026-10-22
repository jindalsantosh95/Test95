using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.ReshopFareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using DynamicMessagesContent = VAA.EAPI.Domain.Entities.Common.DynamicMessagesContent;
using OrderReshopRequest = VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop.OrderReshopRequest;


namespace VAA.EAPI.NDC.Offer.Validations.OrderReshop
{
    public static class ValidateOrderReshop
    {
        public static IEnumerable<ErrorDetail> ValidateOrderReshopRQCabinPreferance(OrderReshopRequest orderReshopRQ)
        {
            var errors = new List<ErrorDetail>();

            var cabinTypes = orderReshopRQ?.ReshopQueries?.FlightSpecific?.SearchCriteria?.CabinTypes?.Where(cabin => cabin.CabinCode != null && cabin.PrefLevel == null);

            if (cabinTypes != null)
            {
                errors.AddRange(cabinTypes.Select(cabin => new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "914_7",
                }).DistinctBy(x => x.ErrorCode));
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderReshopRQDepartureDate(OrderReshopRequest orderReshopRQ)
        {
            return (orderReshopRQ?.OriginDestinationAssociations).Where(originDest => string.Equals(originDest?.OriginDestination?.DepartureDate, "01/01/0001 00:00:00")).Select(originDest => new ErrorDetail
            {
                IsDynamicMessage = false,
                ErrorCode = "912_6",
            }).DistinctBy(x => x.ErrorCode);
        }

        public static IEnumerable<ErrorDetail> ValidateOrderReshopRQDistributionEntity(OrderReshopRequest orderReshopRQ)
        {
            var errors = new List<ErrorDetail>();
            string ordinal = orderReshopRQ?.Parties?.Participants?.FirstOrDefault()?.Ordinal;
            bool checkOrdinal = Convert.ToBoolean(orderReshopRQ?.Parties?.Participants?.FirstOrDefault(x => x.PartyType == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller)?.Ordinal.StartsWith('1'));

            if (orderReshopRQ?.Parties == null || orderReshopRQ.Parties.Participants == null || !orderReshopRQ.Parties.Participants.Any() || orderReshopRQ.Parties.Participants.FirstOrDefault()?.Ordinal == null ||
                orderReshopRQ.Parties.Participants.FirstOrDefault()?.PartyType == null || orderReshopRQ.Parties.Participants.FirstOrDefault()?.AgencyID == null || orderReshopRQ.Parties.Participants.FirstOrDefault()?.Name == null)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_8",
                });

                return errors;
            }

            if (string.IsNullOrEmpty(ordinal) || (ordinal.Equals("0")) || (ordinal.Any(Char.IsLetter)) || (!checkOrdinal))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "912_24",
                    IsDynamicMessage = false
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderReshopRQOrderID(string orderId)
        {
            var errors = new List<ErrorDetail>();

            if (string.IsNullOrEmpty(orderId))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "914_4",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderReshopRQOrderItemID(OrderReshopRequest orderReshopRQ)
        {
            return (orderReshopRQ?.ReshopOrder).Where(oi => string.IsNullOrEmpty(oi.OrderItemId)).Select(oi => new ErrorDetail
            {
                IsDynamicMessage = false,
                ErrorCode = "914_8",
            }).DistinctBy(x => x.ErrorCode);
        }

        public static IEnumerable<ErrorDetail> ValidateOriginDestCriteria(OrderReshopRequest orderReshopRQ)
        {
            var errors = new List<ErrorDetail>();
            foreach (var x in orderReshopRQ?.OriginDestinationAssociations)
            {
                //Validate IATA_LocationCode for OriginDepCriteria
                if (string.IsNullOrEmpty(x.OriginDestination?.OriginAirport))
                {
                    if (errors.Count > 0 && !errors.Exists(error => error.ErrorCode == "914_5"))
                        errors.Add(new ErrorDetail()
                        {
                            ErrorCode = "914_5",
                            IsDynamicMessage = false
                        });
                }

                //Validate IATA_LocationCode for DestArrivalCriteria
                if (string.IsNullOrEmpty(x.OriginDestination?.DestinationAirport))
                {
                    if (errors.Count > 0 && !errors.Exists(error => error.ErrorCode == "914_5"))
                        errors.Add(new ErrorDetail()
                        {
                            ErrorCode = "914_5",
                            IsDynamicMessage = false
                        });
                }

                // Validate OnD tolerance window
                var departureTimerange = x.OriginDestination?.DepartureTimeRange;
                if (departureTimerange != null)
                {
                    var departureDate = Convert.ToDateTime(x.OriginDestination?.DepartureDate);
                    var earliestDateTime = Convert.ToDateTime(departureTimerange.EarliestTime);
                    var latestDateTime = Convert.ToDateTime(departureTimerange.LatestTime);

                    if (departureDate.Hour > 0)
                    {
                        if ((departureDate - earliestDateTime).Hours == 0 || (latestDateTime - departureDate).Hours == 0)
                        {
                            errors.Add(new ErrorDetail()
                            {
                                ErrorCode = "102_5",
                                IsDynamicMessage = false
                            });
                        }
                        else if ((latestDateTime.Date.Day > departureDate.Date.Day) || (departureDate.Date.Day > earliestDateTime.Date.Day))
                        {
                            errors.Add(new ErrorDetail()
                            {
                                ErrorCode = "102_6",
                                IsDynamicMessage = false
                            });
                        }
                    }
                }
            }

            return errors;
        }

        public static string GetFormattedACCT(AgencyValidateResponse validationRS)
        {
            if (validationRS.KeyType == Constants.Application.KeyTypeDirect)
            {
                return string.Concat(string.Format(Constants.Application.ACCTFormat, validationRS.AgencyDetails.RequestAgencyIdType, validationRS.AgencyDetails.RequestAgencyId, validationRS.AgencyDetails.RequestOrgName, string.Empty, string.Empty).Take(Constants.Application.ACCTFormatMaxLength));
            }
            else
            {
                return string.Concat(string.Format(Constants.Application.ACCTFormat, validationRS.AgencyDetails.RequestAgencyIdType, validationRS.AgencyDetails.RequestAgencyId, validationRS.AgencyDetails.RequestOrgName, validationRS.AggregatorDetails.AgencyId, validationRS.AggregatorDetails.CompanyName).Take(Constants.Application.ACCTFormatMaxLength));
            }
        }

        public static IEnumerable<ErrorDetail> ValidateAgentIDReshop(AgencyValidateResponse validateResponse, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();
            var remarks = orderDocument?.OrderRemarks?.Select(x => x.Text);
            string acctRemark = GetFormattedACCT(validateResponse);

            if (!remarks.Contains(acctRemark))
            {
                errors.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_2",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateReshopOrderID(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            if (orderRequest?.OrderId != null && orderDocument?.OrderNumber?.OrderId != null)
            {
                if (orderDocument.OrderNumber?.OrderId != orderRequest.OrderId)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "129_1"
                    });
                    return errors;
                }
            }
            else if (string.IsNullOrEmpty(orderRequest?.OrderId))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_1"
                });
            }
            else if (orderDocument == null)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_1"
                });
                return errors;
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateReshopPNR(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            return (orderDocument?.BookingReference?.RLOC != null && orderRequest?.BookingReferences?.RLOC != null && orderDocument.BookingReference.RLOC != orderRequest.BookingReferences.RLOC) ?
                new ErrorDetail[]
                {
                    new(){
                        IsDynamicMessage = false,
                        ErrorCode = "129_2"
                    }
                } : Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateReshopCabinCode(OrderReshopRequest orderRequest, OrderDocument orderDocument, IEnumerable<ReshopFareFamily> reshopFareFamilies)
        {
            var errors = new List<ErrorDetail>();
            var reshopAllowedFareFamilies = new List<string>();
            Dictionary<string, string> cabinCodes = new()
            {
                { "2", "BUS-FIRST" },
                { "4", "COMFORT-PLUS-PREMIUM-ECON" },
                { "5", "ECON"},
            };

            var cabinList = cabinCodes.Select(x => x.Key);
            var requestCabinList = orderRequest?.ReshopQueries?.FlightSpecific?.SearchCriteria?.CabinTypes?.Select(cabin => cabin.CabinCode);
            var productCode = (from pc in orderDocument.DataList.PriceClass
                               where pc.Id == orderDocument.OrderItems.FirstOrDefault().Services.FirstOrDefault().PriceClassRefIds.FirstOrDefault()
                               select pc.ProductName).First();

            if (requestCabinList != null)
            {
                reshopAllowedFareFamilies.AddRange(reshopFareFamilies.Where(cabin => cabin.ExistingFareFamily == productCode && requestCabinList.Contains(cabin.CabinCode)).SelectMany(cabin => cabin?.ItaFareFamilies));

                bool matched = !requestCabinList.Any(cabin => !cabinList.Contains(cabin));

                if (reshopAllowedFareFamilies.Count == 0)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "12_3"
                    });

                    return errors;
                }

                if (!matched)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "12_1"
                    });

                    return errors;
                }
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSegmentStatus(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();
            var retainedServiceIdList = new List<string>();
            var orderDocServiceList = new List<OrderService>();
            var listFlownSegment = new List<string>();
            var serviceListToCheck = new List<string>();
            bool openCouponStatus = (orderDocument?.OrderItems?.Where(oi => oi.Services.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum).Where(x => x.AccountableDocumentRefs != null).SelectMany(orderItem => orderItem?.AccountableDocumentRefs).SelectMany(adref => adref?.Coupons.Where(c => c.CouponStatus == "OPEN"))).Any();

            if (!openCouponStatus)
            {
                if (orderRequest?.ReshopOrder?.FirstOrDefault()?.Services?.FirstOrDefault()?.ServiceId != null)
                {
                    retainedServiceIdList = orderRequest.ReshopOrder?.SelectMany(ro => ro.Services).Select(s => s.ServiceId).ToList();
                }

                var orderDocService = orderDocument?.OrderItems?.Where(x => x.ItemStatus.Status == Constants.Application.Current).SelectMany(x => x?.Services) ?? Enumerable.Empty<OrderService>();

                orderDocServiceList.AddRange(orderDocService);

                var flownSegmentIDs = orderDocument?.OrderItems?.Where(oi => oi.ItemStatus.Status == Constants.Application.Current && oi.Services.FirstOrDefault()?.Product != null && oi.Services.FirstOrDefault()?.Product?.ProductCode?.ToUpper() != "SEAT").SelectMany(orderItem => orderItem?.AccountableDocumentRefs).SelectMany(adref => adref?.Coupons.Where(c => c.CouponStatus != "OPEN").Select(adref => adref?.SegmentRefId)) ?? Enumerable.Empty<string>();

                listFlownSegment.AddRange(flownSegmentIDs);

                if (retainedServiceIdList != null && retainedServiceIdList.Count > 0 && listFlownSegment != null && listFlownSegment.Count > 0)
                {
                    serviceListToCheck.AddRange(orderDocServiceList.Where(service => listFlownSegment.Contains(service?.ProductSegmentAssociation?.flightSegmentRefIds.FirstOrDefault())).Select(service => service?.ServiceId));
                    var slArray = serviceListToCheck.ToArray();

                    if (!retainedServiceIdList.Any(x => slArray.Contains(x)))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "129_6"
                        });
                    }
                }
                else
                {
                    if (listFlownSegment.Count > 0 && retainedServiceIdList?.Count == 0)
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "452_1"
                        });
                    }
                }
            }
            return errors;

        }

        public static IEnumerable<ErrorDetail> ValidateFlownSegment(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            var segIds = (orderDocument?.OrderItems?.Where(oi => oi.Services.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum).Where(x => x.AccountableDocumentRefs != null).SelectMany(orderItem => orderItem?.AccountableDocumentRefs).SelectMany(adref => adref?.Coupons.Where(c => c.CouponStatus == "USED"))).Select(s => s.SegmentRefId).Distinct();

           foreach(var segment in orderDocument.DataList.SegmentDetails)
            {
                if(segIds.Any(s => s == segment.SegmentId))
                {
                    if (orderRequest?.OriginDestinationAssociations.Any(x => x.OriginDestination.OriginAirport == segment.OriginAirportCode && x.OriginDestination.DestinationAirport == segment.DestinationAirportCode) == true)
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "790_1"
                        });
                    }
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateReshopOrderItemCount(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            if (!(orderRequest?.OriginDestinationAssociations?.Count() == orderDocument?.DataList?.OriginDestinationAssociations?.Count() && !orderRequest.ReshopOrder.Any()))
            {
                var orderDocItems = orderDocument?.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum).Select(x => x.OrderItemId);
                var orderReshopItems = orderRequest?.ReshopOrder.Select(x => x.OrderItemId);

                if (orderReshopItems.Any(order => !orderDocItems.Contains(order)))
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "914_9"
                    });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateReshopDateChronology(OrderReshopRequest orderRequest, OrderDocument orderDocument, bool isReshopDisruption = false)
        {
            var errors = new List<ErrorDetail>();
            var disruptedSegment = isReshopDisruption
            ? orderDocument?.DataList?.SegmentDetails?
                .FirstOrDefault(x => x.ScheduleChangeIndicator)
            : null;

            var tkReplacementOdRefId = isReshopDisruption && disruptedSegment != null? 
                orderDocument?.DataList?.SegmentDetails?
                    .FirstOrDefault(x => !x.ScheduleChangeIndicator 
                    && x.Status?.Code == "TK" &&
                        string.Equals(
                            x.OriginAirportCode,
                            disruptedSegment.OriginAirportCode,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            x.DestinationAirportCode,
                            disruptedSegment.DestinationAirportCode,
                            StringComparison.OrdinalIgnoreCase))
                    ?.OdRefId
                : null;

            if (orderRequest?.OriginDestinationAssociations.Count() != orderDocument?.DataList?.OriginDestinationAssociations.Count())
            {
                foreach (var (requestond, requestOndValueAndIndex, ond) in from requestond in orderRequest?.OriginDestinationAssociations
                                                                           let requestOndValueAndIndex = orderDocument?.DataList?.OriginDestinationAssociations.Select((v, i) => new { value = v, index = i }).FirstOrDefault(x => x.value.OriginDestination.OriginAirport == requestond.OriginDestination.OriginAirport
                                    && x.value.OriginDestination.DestinationAirport == requestond.OriginDestination?.DestinationAirport)
                                                                           from ond in orderDocument?.DataList?.OriginDestinationAssociations?.Select((v, i) => new { value = v, index = i })
                                                                           select (requestond, requestOndValueAndIndex, ond))
                {
                    if (isReshopDisruption && string.Equals(ond.value.OdLabel, tkReplacementOdRefId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (requestOndValueAndIndex?.index > ond.index)
                        {
                        var result = DateTime.Compare(Convert.ToDateTime(requestond.OriginDestination?.DepartureDate), Convert.ToDateTime(ond.value.OriginDestination?.DepartureDate));

                        if (result < 0)
                        {
                            if (!errors.Exists(error => error.ErrorCode == "102_2"))
                            {
                                errors.Add(new ErrorDetail
                                {
                                    IsDynamicMessage = false,
                                    ErrorCode = "102_2"
                                });

                                return errors;
                            }
                        }
                    }
                    else if (requestOndValueAndIndex?.index < ond.index)
                    {
                        var result = DateTime.Compare(Convert.ToDateTime(requestond.OriginDestination?.DepartureDate), Convert.ToDateTime(ond.value.OriginDestination?.DepartureDate));

                        if (result > 0)
                        {
                            if (!errors.Exists(error => error.ErrorCode == "102_2"))
                            {
                                errors.Add(new ErrorDetail
                                {
                                    IsDynamicMessage = false,
                                    ErrorCode = "102_2"
                                });

                                return errors;
                            }
                        }
                    }
                }
            }

            ////if there are more than one ond and user is updating all the onds in the  request
            if (orderRequest?.OriginDestinationAssociations?.Count() > 1)
            {
                for (int i = 0; i <= orderRequest.OriginDestinationAssociations.Count() - 1; i++)
                {
                    if (i != orderRequest.OriginDestinationAssociations?.Count() - 1)
                    {
                        var firstDateTime = Convert.ToDateTime(orderRequest.OriginDestinationAssociations?.ElementAt(i).OriginDestination?.DepartureDate);
                        for (int j = 1; j <= orderRequest.OriginDestinationAssociations?.Count() - 1; j++)
                        {
                            if (orderRequest.OriginDestinationAssociations?.ElementAt(j) != null)
                            {
                                var result = DateTime.Compare(firstDateTime, Convert.ToDateTime(orderRequest.OriginDestinationAssociations?.ElementAt(j).OriginDestination?.DepartureDate));

                                if (result > 0)
                                {
                                    errors.Add(new ErrorDetail
                                    {
                                        IsDynamicMessage = false,
                                        ErrorCode = "102_2"
                                    });

                                    return errors;
                                }
                            }
                        }
                    }
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateReshopServiceID(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            var retainSvcId = orderRequest.ReshopOrder.SelectMany(x => x.Services).Select(y => y.ServiceId).Distinct().ToList();

            foreach (var x in orderRequest.ReshopOrder)
            {
                int serviceId = 0;

                if (x.Services != null && x.Services.Any())
                {
                    var orderItem = orderDocument.OrderItems.FirstOrDefault(o => o.OrderItemId == x.OrderItemId);
                    if (orderItem == null)
                    {
                        // If the corresponding OrderItem is not found, skip processing this ReshopOrder entry
                        continue;
                    }

                    var distinctPax = orderItem.Services.SelectMany(s => s.ServiceBeneficiaries)
                                                          .Distinct()
                                                          .ToArray();

                    var flightServices = orderItem.Services.Where(s => s.Product?.ProductType == Product.ProductTypes.FlightEnum)
                                             .ToArray();

                    var serviceList = distinctPax.SelectMany(pax =>
                    {
                        return orderItem.Services.Where(s => s.Product.ProductType == Product.ProductTypes.FlightEnum).Select(s =>
                        {
                            return new OrderService
                            {
                                ServiceId = $"{Constants.Application.SV}{++serviceId}",
                                ServiceBeneficiaries = [pax],
                                SegmentRefIds = s.SegmentRefIds
                            };
                        });
                    }).ToArray();

                    var requestedRetainSegs = serviceList.Where(x => retainSvcId.Contains(x.ServiceId)).SelectMany(x => x.SegmentRefIds).Distinct();
                    var requestedRetainServices = serviceList.Where(x => requestedRetainSegs.Any(r => x.SegmentRefIds.Contains(r))).Select(x => x.ServiceId);

                    if (requestedRetainServices.Intersect(x.Services.Select(x => x.ServiceId)).Count() != requestedRetainServices.Count())
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "129_4"
                        });
                    }
                }
            }

            return errors;
        }

        public static IEnumerable<NDCWarning> ValidateReshopOrderWarnings(OrderDocument orderDocument)
        {
            return orderDocument.Warnings.Select(warning => new NDCWarning
            {
                Code = warning.Code,
                ShortText = warning.Message,
                Status = warning.Status,
                TagText = warning.Type
            });
        }

        public static List<NDCWarning> ValidateReshopSeatDeleteWarnings(OrderDocument sabreOrderDoc, OrderReshopRequest orderReshopRQ)
        {
            var warnings = new List<NDCWarning>();

            var orderReshopRQ_Ond = orderReshopRQ.OriginDestinationAssociations;
            var orderDoc_Ond = sabreOrderDoc.DataList.OriginDestinationAssociations;

            foreach (var ond in orderReshopRQ_Ond)
            {
                string od_Label = orderDoc_Ond.Where(x => x.OriginDestination.OriginAirport == ond.OriginDestination.OriginAirport && x.OriginDestination.DestinationAirport == ond.OriginDestination.DestinationAirport).Select(x => x.OdLabel).First();
                var seat_details = sabreOrderDoc.OrderItems.SelectMany(x => x.Services).Where(s => s.Product != null).Where(x => x.OriginDestinationRefIds.First() == od_Label && x.CurrentSeatAssignment != null);
                if (seat_details.Any())
                {
                    warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "214_1", Type = "Processed" });
                }
            }

            return warnings;
        }

        public static IEnumerable<NDCWarning> ValidateReshopAPISWarnings(OrderDocument sabreOrderDoc, OrderReshopRequest orderReshopRQ)
        {
            var hasTravelDocuments = sabreOrderDoc?.DataList?.CustomerDetails?.Any(customer => customer?.TravelDocuments?.Any() == true) == true;

            return hasTravelDocuments
            ?
            [
                new NDCWarning
                {
                    IsDynamicMessage = false,
                    Code = "142_1",
                    Type = "Processed"
                }
            ]
         : Enumerable.Empty<NDCWarning>();
        }

        public static IEnumerable<ErrorDetail> ValidateReshopOrderItemID(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();
            var reshopRequestOrderItems = orderRequest?.ReshopOrder;
            var orderDocOrderItems = orderDocument?.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum);
            var orderitemList = reshopRequestOrderItems.Where(x => orderDocOrderItems.All(y => y.OrderItemId != x.OrderItemId)).ToList();

            if (orderitemList != null && orderitemList.Count > 0)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "914_10"
                });

                return errors;
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateReshopOnd(OrderReshopRequest orderRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            if (orderDocument?.DataList?.OriginDestinationAssociations?.Count() > 1 && orderRequest?.OriginDestinationAssociations?.Count() > 1 && orderRequest?.OriginDestinationAssociations?.Count() == orderDocument.DataList?.OriginDestinationAssociations?.Count())
            {
                var servicesCount = orderRequest?.ReshopOrder.SelectMany(x => x.Services).Count();
                if (servicesCount != null && servicesCount > 0)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "129_7"
                    });

                    return errors;
                }
            }

            if (orderRequest?.OriginDestinationAssociations.Count() != orderDocument?.DataList?.OriginDestinationAssociations.Count())
            {
                foreach (var requestond in orderRequest.OriginDestinationAssociations)
                {
                    var requestOndValueAndIndex = orderDocument?.DataList?.OriginDestinationAssociations.Select((v, i) => new { value = v, index = i }).FirstOrDefault(x => x.value.OriginDestination.OriginAirport == requestond.OriginDestination.OriginAirport
                    && x.value.OriginDestination.DestinationAirport == requestond.OriginDestination?.DestinationAirport);

                    if (requestOndValueAndIndex == null)
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "129_5"
                        });
                    }

                    return errors;
                }
            }

            if (orderRequest?.OriginDestinationAssociations != null && orderDocument?.DataList?.OriginDestinationAssociations != null && orderRequest?.OriginDestinationAssociations?.Count() == orderDocument.DataList?.OriginDestinationAssociations?.Count())
            {
                var ondList = orderDocument.DataList?.OriginDestinationAssociations.Where(x => orderRequest.OriginDestinationAssociations.All(y => y.OriginDestination.OriginAirport != x.OriginDestination.OriginAirport || y.OriginDestination.DestinationAirport != x.OriginDestination.DestinationAirport));
                if (ondList != null && ondList.Any())
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "129_5"
                    });

                    return errors;
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateIfRLOCOrdeIDMatch(OrderReshopRequest orderReshopRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            if (!string.IsNullOrEmpty(orderReshopRequest?.OrderId) && !string.IsNullOrEmpty(orderReshopRequest.BookingReferences?.RLOC) && orderDocument?.BookingReference?.RLOC != orderReshopRequest.BookingReferences?.RLOC)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_28",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderStatus(OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            if (orderDocument.OrderStatus == Constants.Application.CancelledOrderItemStatus &&
                orderDocument.OrderItems.FirstOrDefault()?.AccountableDocumentRefs == null)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "303_2",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderIDFormat(OrderReshopRequest orderReshopRequest)
        {
            var errors = new List<ErrorDetail>();

            if (!string.IsNullOrEmpty(orderReshopRequest?.OrderId) && !RegexCompiled.Match(orderReshopRequest.OrderId, "^[A-Z0-9]*$").Success)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "914_1",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateRLOCFormat(OrderReshopRequest orderReshopRequest)
        {
            var errors = new List<ErrorDetail>();

            if (!string.IsNullOrEmpty(orderReshopRequest?.BookingReferences?.RLOC) && (!RegexCompiled.Match(orderReshopRequest.BookingReferences?.RLOC, "^[A-Z0-9]*$").Success || orderReshopRequest.BookingReferences?.RLOC.Length != Constants.Application.PNRLength))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "914_2",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateRFCAgent(OrderReshopRequest orderReshopRQ, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();
            var orderReshopReq = orderReshopRQ.Parties.Participants.Where(x => x.PartyType == Participant.PartyTypeEnum.Seller);
            if ((orderReshopReq.First().AgencyID != orderDocument.Parties.Participants.FirstOrDefault(x => x.PartyType == Participant.PartyTypeEnum.Seller)?.AgencyID))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_2",
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSegmantStatus(OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            var flownExists = orderDocument.OrderItems?.Where(oi => oi.Services.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum)?.Where(x => x.AccountableDocumentRefs != null).SelectMany(orderItem => orderItem?.AccountableDocumentRefs).SelectMany(c => c.Coupons).All(x => x.CouponStatus == Domain.Model.VAA.Constants.AllFlownCouponStatus);
            if (flownExists == true)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "452_1",
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateRequestOrderId(IATA_OrderReshopRQ orderReshopRequest)
        {
            var errors = new List<ErrorDetail>();
            var orderRefId = orderReshopRequest.Request.OrderRefID;
            var cancelOrderRefId = ((orderReshopRequest.Request.UpdateOrder?.Item) as CancelOrderRefType);

            if (string.IsNullOrEmpty(orderRefId) || string.IsNullOrEmpty(cancelOrderRefId.OrderRefID))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "914_4",
                });
            }

            if (!RegexCompiled.IsMatch(orderRefId, "^[A-Za-z0-9-|]*$") || !RegexCompiled.IsMatch(cancelOrderRefId.OrderRefID, "^[A-Za-z0-9-|]*$") || orderRefId.Length != cancelOrderRefId.OrderRefID.Length)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_16",
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateNameCorrectionRequest(IATA_OrderReshopRQ orderReshopRequest, OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();

            var reshopOrder = ((orderReshopRequest.Request.UpdateOrder?.Item) as ReshopOrderType);
            var isNameCorrectionOrder = reshopOrder?.ReshopOrderChoice?.Item as UpdatePaxNameType;
            var orderPaxRefIds = orderDocument.DataList.CustomerDetails.Select(cd => cd.CustId);

            string paxRef = isNameCorrectionOrder?.PaxRefID.First();

            // Validate if PaxRefID present in orderDocument
            if (!orderPaxRefIds.Contains(paxRef))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_4",
                });
            }

            return errors;
        }
        public static bool IsNullOrEmpty(this string[] array)
        {
            return array == null || array.Length == 0;
        }


        public static IEnumerable<ErrorDetail> ValidateRequestOrderRefIds(IATA_OrderReshopRQ orderReshopRQ)
        {
            var errors = new List<ErrorDetail>();
            var cancelOrderRefId = ((orderReshopRQ.Request.UpdateOrder?.Item) as CancelOrderRefType);

            if (orderReshopRQ.Request.OrderRefID != cancelOrderRefId.OrderRefID)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_17",
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSegmentCouponStatus(OrderDocument sabreOrderDocument)
        {
            var orderItems = sabreOrderDocument?.OrderItems;
            var errors = new List<ErrorDetail>();

            if (orderItems is null)
            {
                return [];
            }

            var hasOpenCoupon = orderItems.Any(orderItem => orderItem?.ItemStatus?.Status == Constants.Application.Current &&  orderItem.Services?.FirstOrDefault()?.Product?.ProductType.Value != Product.ProductTypes.SeatEnum && orderItem.AccountableDocumentRefs?.Any(documentReference =>documentReference?.Coupons?.Any(coupon => coupon?.CouponStatus == "OPEN") == true) == true);

            if (hasOpenCoupon)
            {
                return [];
            }

            var coupons = orderItems.Where(orderItem => orderItem?.ItemStatus?.Status == Constants.Application.Current && orderItem?.Services?.FirstOrDefault()?.Product is not null)
                .SelectMany(orderItem => orderItem?.AccountableDocumentRefs ?? [])
                .Where(documentReference => documentReference?.Coupons is not null)
                .SelectMany(documentReference => documentReference!.Coupons!)
                .Where(coupon => coupon?.CouponStatus is not null && coupon.CouponStatus != "OPEN")
                .Select(coupon => coupon!)
                .ToList();
            var allCouponsHaveExpectedStatus = coupons.Count > 0 && coupons.All(coupon => coupon.CouponStatus == Constants.Application.FlownCouponStatus || coupon.CouponStatus == Constants.Application.CouponStatus);

            return allCouponsHaveExpectedStatus ?
            [
                new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "452_1"
                }
            ]:[];         

        }

        public static IEnumerable<ErrorDetail> ValidateOrderReshopRequest(DistributionChainLinkType[] distributionChain)
        {
            var errors = new List<ErrorDetail>();
            string totalDC = distributionChain.Length.ToString();

            if (!distributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Carrier))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "914_12",
                    IsDynamicMessage = false
                });
            }

            if (errors.Count == 0)
            {
                foreach (var dC in distributionChain)
                {
                    if (dC.ParticipatingOrg == null || string.IsNullOrEmpty(dC.ParticipatingOrg.OrgID))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_1",
                            IsDynamicMessage = false
                        });
                    }
                    else if (dC.OrgRole == DistributionChainRoleCodeType.Carrier && dC.Ordinal != totalDC)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_24",
                            IsDynamicMessage = false
                        });
                    }
                    else if (dC.OrgRole == DistributionChainRoleCodeType.Carrier && dC.ParticipatingOrg.OrgID != Constants.Application.OwnerCode)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "914_13",
                            IsDynamicMessage = false
                        });
                    }
                    else if ((dC.SalesAgent != null && string.IsNullOrEmpty(dC.SalesAgent.SalesAgentID)) || (dC.SalesBranch != null && string.IsNullOrEmpty(dC.SalesBranch.SalesBranchID)))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_1",
                            IsDynamicMessage = false
                        });
                    }
                    else if (string.IsNullOrEmpty(dC.Ordinal) || (dC.Ordinal.Equals("0")) || (dC.Ordinal.Any(Char.IsLetter)) || (dC.OrgRole == DistributionChainRoleCodeType.Seller && !dC.Ordinal.StartsWith("1")))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_24",
                            IsDynamicMessage = false
                        });
                    }

                }
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateEligibleSegment(IEnumerable<Segment> segmentList)
        {
            return segmentList.Any(s => s.OperatingCarrier != null && s.OperatingCarrier != "DL")
                        ? [new ErrorDetail { ErrorCode = "137_2", IsDynamicMessage = false }]
                        : [];
        }

        public static IEnumerable<ErrorDetail> ValidateNamingConvention(Domain.Model.VAA.Reshop.PaxNameChange paxNameChange)
        {
            var errors = new List<ErrorDetail>();

            if (string.IsNullOrEmpty(paxNameChange.GivenName))
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "152_2" });

            else if (paxNameChange.GivenName.Length > Convert.ToInt32(Constants.Application.GivenNameMaxLength))
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "150_2" });

            else if (!RegexCompiled.Match(paxNameChange.GivenName, "^[a-zA-Z ]*$").Success)
                errors.Add(new() { IsDynamicMessage = true, ErrorCode = "314_2", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = paxNameChange.PaxRefID } } });

            if (string.IsNullOrEmpty(paxNameChange.Surname) || paxNameChange.Surname.Length < 2)
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "151_1" });

            else if (!RegexCompiled.Match(paxNameChange.Surname, "^[a-zA-Z ]*$").Success)
                errors.Add(new() { IsDynamicMessage = true, ErrorCode = "314_3", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = paxNameChange.PaxRefID } } });

            else if (paxNameChange.Surname.Length > int.Parse(Constants.Application.SurnameMaxLength))
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "149_1" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateNameChanged(OrderDocument orderDocument, string paxId)
        {
            var customerDetail = orderDocument.DataList.CustomerDetails.First(a => a.CustId == paxId);

            return customerDetail.IsNameChanged ? [new ErrorDetail { IsDynamicMessage = false, ErrorCode = "137_4" }] : [];
        }

        public static IEnumerable<ErrorDetail> ValidateFullSurnameChange(OrderDocument orderDocument, Domain.Model.VAA.Reshop.PaxNameChange paxNameChange)
        {
            var customerDetail = orderDocument.DataList.CustomerDetails.First(a => a.CustId == paxNameChange.PaxRefID);

            if(paxNameChange.GivenName.Equals(customerDetail.LastName, StringComparison.CurrentCultureIgnoreCase) && paxNameChange.Surname.Equals(customerDetail.FirstName, StringComparison.CurrentCultureIgnoreCase))
            {
                // interchange
            }
            else if (paxNameChange.Surname.Contains(customerDetail.LastName, StringComparison.CurrentCultureIgnoreCase))
            {
                // addition of surname
            }
            else if (MoreThanThreeCharsChanged(paxNameChange.Surname.ToLower(), customerDetail.LastName.ToLower()))
            {
                return [new ErrorDetail { IsDynamicMessage = false, ErrorCode = "137_3" }];
            }

            return [];
        }

        public static bool MoreThanThreeCharsChanged(string oldName, string newName)
        {
            // If lengths differ, consider the extra length as changes
            int length = Math.Min(oldName.Length, newName.Length);
            int diffCount = Math.Abs(oldName.Length - newName.Length);

            for (int i = 0; i < length; i++)
            {
                if (oldName[i] != newName[i])
                    diffCount++;

                if (diffCount > 3)
                    return true;
            }

            return false;
        }

        public static IEnumerable<ErrorDetail> ValidateFlightSegmentStatus(OrderDocument orderDocument)
        {
            var flownExists = orderDocument.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs?
                                .Any(x => x.Coupons.Any(c => c.CouponStatus == Domain.Model.VAA.Constants.AllFlownCouponStatus)) == true;

            return flownExists ? [new ErrorDetail { IsDynamicMessage = false, ErrorCode = "137_1" }] : [];
        }

        public static IEnumerable<ErrorDetail> ValidateAgencyInRequest(OrderDocument orderDocument, DistributionChainLinkType[] distributionChain)
        {
            var errors = new List<ErrorDetail>();
            var dc = distributionChain.Where(x => x.OrgRole == DistributionChainRoleCodeType.Seller);

            if (orderDocument.Parties.Participants.First(x => x.PartyType == Participant.PartyTypeEnum.Seller).AgencyID != dc.First().ParticipatingOrg.OrgID)
            {
                errors.Add(new ErrorDetail()
                {
                    ErrorCode = "368_1",
                    IsDynamicMessage = false
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSellerFollowUpAction(OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();
            if (!orderDocument.OrderItems.Any( oi => oi.SellerFollowUpAction != null && oi.SellerFollowUpAction.Any(a => a.Is("Cancel"))) && orderDocument.DataList.SegmentDetails.Any(x => x.ScheduleChangeIndicator))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "915_2",
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateReshopHoldBooking(OrderDocument orderDocument)
        {
            var errors = new List<ErrorDetail>();
            if (orderDocument.ReshopHoldBookingCounter.GetValueOrDefault() >= 6)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "790_2"
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateDisruptionScopeAndTicketIssueDate(OrderReshopRequest reshopRQ, OrderDocument orderDocument, int ticketValidityWindowInDays)
        {
            foreach (var requestOnd in reshopRQ.OriginDestinationAssociations)
            {
                var originalSegment = orderDocument.DataList.SegmentDetails.FirstOrDefault(x =>
                    string.Equals(x.OriginAirportCode, requestOnd.OriginDestination.OriginAirport, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.DestinationAirportCode, requestOnd.OriginDestination.DestinationAirport, StringComparison.OrdinalIgnoreCase));

                if (originalSegment == null)
                {
                    return [new ErrorDetail { ErrorCode = "129_5", IsDynamicMessage = false }];
                }

                if (!originalSegment.ScheduleChangeIndicator)
                {
                    continue;
                }

                var ticketDocument = orderDocument.OrderItems
                    .SelectMany(orderItem => orderItem.AccountableDocumentRefs ?? [])
                    .FirstOrDefault(accDoc =>
                        accDoc.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum &&
                        accDoc.Coupons?.Any(coupon => string.Equals(coupon.SegmentRefId, originalSegment.SegmentId, StringComparison.OrdinalIgnoreCase)) == true);

                if (ticketDocument == null)
                {
                    continue;
                }

                var requestedDepartureDate = DateTime.Parse(requestOnd.OriginDestination.DepartureDate);
                var ticketValidityEndDate = DateTime.Parse(ticketDocument.IssueDate).AddDays(ticketValidityWindowInDays);

                if (requestedDepartureDate.Date > ticketValidityEndDate.Date)
                {
                    return [new ErrorDetail { ErrorCode = "16", IsDynamicMessage = false }];
                }
            }

            return [];
        }
    }
}
