using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;


namespace VAA.EAPI.NDC.Offer.Validations.SeatAvailability
{
    public class ValidateSeatAvailability
    {
        public static IEnumerable<ErrorDetail> ValidateSeatAvailabilityOfferID(IATA_SeatAvailabilityRQ seatavailabilityRequest)
        {
            var seatOffer = (seatavailabilityRequest.Request.SeatAvailCoreRequest.Item as SeatAvailOfferRequestType).Offer;
            var errorIds = new List<ErrorDetail>();

            if (seatOffer != null)
            {
                if ((seatOffer.FirstOrDefault().OfferID == null) || string.IsNullOrEmpty(seatOffer.FirstOrDefault().OfferID))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_5",
                    });
                }
                if (seatOffer.FirstOrDefault().OwnerCode == null || string.IsNullOrEmpty(seatOffer.FirstOrDefault().OwnerCode))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_10",
                    });
                }
            }
            return errorIds;
        }

        public static IEnumerable<ErrorDetail> ValidateSeatAvailabilityRequest(IATA_SeatAvailabilityRQ seatavailabilityRequest)
        {
            var errors = new List<ErrorDetail>();
            if (seatavailabilityRequest == null)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_6",
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateAlacarteOfferInResponse(IATA_SeatAvailabilityRS seatResponse)
        {
            var errorIds = new List<ErrorDetail>();

            if ((seatResponse?.Items as SeatAvailResponseType[]).FirstOrDefault()?.ALaCarteOffer == null)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_30",
                });
            }

            return errorIds;
        }

        public static IEnumerable<ErrorDetail> ValidateSeatOrderDetails(IATA_SeatAvailabilityRQ seatAvailabilityRQ)
        {
            var errors = new List<ErrorDetail>();
            var seatOrder = (seatAvailabilityRQ.Request.SeatAvailCoreRequest.Item as SeatAvailOrderRequestType).Order;
            if (seatOrder == null || string.IsNullOrEmpty(seatOrder?.OrderID) || string.IsNullOrEmpty(seatOrder?.OwnerCode))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "912_1",
                    IsDynamicMessage = false
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateAgentDetails(DistributionChainLinkType[] distributionChain)
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
                    else if (dC.SalesAgent != null && string.IsNullOrEmpty(dC.SalesAgent.SalesAgentID) || dC.SalesBranch != null && string.IsNullOrEmpty(dC.SalesBranch.SalesBranchID))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_1",
                            IsDynamicMessage = false
                        });
                    }
                    else if (string.IsNullOrEmpty(dC.Ordinal))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "912_7",

                        });
                    }
                    else if (dC.Ordinal.Equals("0") || dC.Ordinal.Any(char.IsLetter) || dC.OrgRole == DistributionChainRoleCodeType.Seller && !dC.Ordinal.StartsWith("1"))
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

        public static IEnumerable<ErrorDetail> ValidateSeatOfferDetailsInOfferDoc(IATA_SeatAvailabilityRQ seatAvailabilityRQ, OfferResponse offerdoc, string offerId)
        {
            var errors = new List<ErrorDetail>();
            if (offerdoc.Error != null || offerdoc.Offers.Count() == 0 || offerdoc.Offers.First().OfferId != offerId)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_9",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateEligibleSegment(IEnumerable<FlightSegment> segmentList)
        {
            var errorIds = new List<ErrorDetail>();

            foreach (var segment in segmentList)
            {
                if (segment.OperatingCarrier != null && segment.OperatingCarrier != "DL")
                {
                    errorIds.Add(new ErrorDetail
                    {
                        ErrorCode = "200_1",
                        IsDynamicMessage = true,
                        DynamicMessagesContents = new VAA.EAPI.Domain.Entities.Common.DynamicMessagesContent[]
                        {
                            new()
                            {
                                Fieldname = "<<FlightNumber>>",
                                ReplaceWith = segment.OperatingCarrier+ segment.OperatingFlightNumber
                            }
                }
                    });
                }
            };

            return errorIds;
        }

        public static IEnumerable<ErrorDetail> ValidateEligibleVssSegment(IEnumerable<Segment> segmentList)
        {
            var errorIds = new List<ErrorDetail>();

            foreach (var segment in segmentList)
            {
                if (segment.OperatingCarrier != "DL" && segment.OperatingCarrier is not null)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        ErrorCode = "200_1",
                        IsDynamicMessage = true,
                        DynamicMessagesContents = new VAA.EAPI.Domain.Entities.Common.DynamicMessagesContent[]
                        {
                            new()
                            {
                                Fieldname = "<<FlightNumber>>",
                                ReplaceWith = segment.OperatingCarrier+ segment.OperatingFlightNumber
                            }
                }
                    });
                }
            };

            return errorIds;
        }

        public static IEnumerable<ErrorDetail> ValidateEligibleSegmentSeatsWithOrderId(IEnumerable<Segment> segmentList)
        {
            var errorIds = new List<ErrorDetail>();

            if (!segmentList.Any())
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "197_1",
                });
            }

            return errorIds;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderDetails(OrderDocument orderDocument)
        {
            var errorIds = new List<ErrorDetail>();

            // Check Order is not found in the Order store of NDC
            if (orderDocument == null)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_1",
                });

            }

            else if (orderDocument.Warnings != null && orderDocument.Warnings.Any())
            {
                if (orderDocument.Warnings.Any(x => x.Code == "129"))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "129_1",
                    });
                }
                else
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_22",
                    });
                }
            }

            else if (orderDocument.OrderItems.Any(x => x.Services?.FirstOrDefault().Product?.ProductType.Value == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum))
            {
                var seatOrderItems = orderDocument.OrderItems.Where(x => x.Services?.Any(s => s?.Product?.ProductType.Value == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum) == true);

                bool areSeatBookedForAllPax = orderDocument.DataList.CustomerDetails.Where(x => !x.CustId.Contains(Constants.Application.INF)).All(cust =>
                orderDocument.DataList.SegmentDetails.All(seg => seatOrderItems.Any(z => z.Services.First().ServiceBeneficiaries.First() == cust.CustId && z.Services.First().SegmentRefIds.First() == seg.SegmentId)));

                if (areSeatBookedForAllPax)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "197_1",
                    });
                }
            }

            // Check PNR is Hold booking or Instant
            else if (orderDocument.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs == null)
            {
                var tktNumber = orderDocument.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs?.FirstOrDefault().DocumentNumber;
                if (tktNumber is null)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_28",
                    });
                }
            }
            // Check Seat map requested with only OrderID
            else if (orderDocument.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs != null)
            {
                var flownExists = orderDocument.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs.Any(x => x.Coupons.FirstOrDefault().CouponStatus == Domain.Model.VAA.Constants.AllFlownCouponStatus);
                if (flownExists == true)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "208_1",
                    });
                }
            }

            if (errorIds.Count == 0)
            {
                // Check all the segments in the Order is operated by non - VS carrier
                var totalFlightsCount = orderDocument?.OrderItems?.FirstOrDefault()?.Services.Count();
                var nonVS = orderDocument?.DataList.SegmentDetails.Count(seg => (seg.OperatingCarrier != null) && (seg.OperatingCarrier != "DL"));

                if (totalFlightsCount == nonVS)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "219_1",
                    });
                }
            }

            return errorIds;
        }

        public static IEnumerable<NDCWarning> ValidateSeatAvailabilityOrderWarnings(OrderDocument orderDocument)
        {
            return orderDocument.Warnings.Select(warning => new NDCWarning
            {
                Code = warning.Code,
                ShortText = warning.Message,
                Status = warning.Status,
                TagText = warning.Type
            });
        }
        public static IEnumerable<NDCWarning> ValidateFormOfPaymentType(Infrastructure.Model.ValidationMS.ValidationRS validationMSResponse, bool isCreditCardAccepted, IHostEnvironment hostEnvironment)
        {
            var warningIds = ResponseValidations.ValidateFormOfPaymentType(validationMSResponse,isCreditCardAccepted).ToList();

            return ErrorHelper<NDCWarning>.GetWarnings(warningIds, hostEnvironment);
        }

        public static IEnumerable<ErrorDetail> ValidateGDSEligibleSegment(IEnumerable<Segment> segmentList)
        {
            return (segmentList?.Count() == 0) ? new ErrorDetail[]
           {
                new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "197_1",
                }
           } : new ErrorDetail[] { };
        }

        public static IEnumerable<ErrorDetail> ValidateOrderSearchNullResponse(Air4OrderSearchMSResponse air4OrderSearchRS)
        {
            return (air4OrderSearchRS == null || air4OrderSearchRS.OrderSearchResponse == null || (air4OrderSearchRS.ErrorDetails != null)) ? new ErrorDetail[]
            {
               new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "914_11",
                }
            } : new ErrorDetail[] { };
        }

        public static IEnumerable<ErrorDetail> ValidateGdsAgentBooking(Air4OrderSearchMSResponse air4OrderSearchRS, string iataNumber)
        {
            return (iataNumber != air4OrderSearchRS.OrderSearchResponse.PointOfSale?.IATANumber) ? new ErrorDetail[]
            {
                new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_2",
                }
            } : new ErrorDetail[] { };
        }

        public static IEnumerable<ErrorDetail> ValidateGDSFlownSegments(Air4OrderSearchMSResponse air4OrderSearchRS)
        {
            return (air4OrderSearchRS.OrderSearchResponse.OrderItems.First().AccountableDocumentRefs?.FirstOrDefault()?.AllFlown == true) ? new ErrorDetail[]
            {
                    new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "208_1",
                    }
            } : new ErrorDetail[] { };
        }
    }
}

