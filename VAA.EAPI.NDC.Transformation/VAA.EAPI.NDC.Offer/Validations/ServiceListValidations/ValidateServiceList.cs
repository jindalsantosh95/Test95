using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Validations.ServiceListValidations
{
    public class ValidateServiceList
    {

        public static IEnumerable<ErrorDetail> ValidateServiceOrderDetails(IATA_ServiceListRQ serviceListReq)
        {
            var errors = new List<ErrorDetail>();
            var serviceOrder = (serviceListReq.Request.ServiceListCoreRequest.Item as ServiceListOrderRequestType).Order;
            if (serviceOrder == null || string.IsNullOrEmpty(serviceOrder?.OrderID) || string.IsNullOrEmpty(serviceOrder?.OwnerCode))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "912_1",
                    IsDynamicMessage = false
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderDetails(OrderDocument orderDocument)
        {
            var errorIds = new List<ErrorDetail>();

            // Check Order is not found in the Order store of NDC
            if (orderDocument.Warnings != null && orderDocument.Warnings.ToList().Count > 0)
            {
                if (orderDocument.Warnings.Any(x => x.Code == "129" || x.Code == "914"))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_1",
                    });
                }
               
            }

            // Check Service List Requested with Flown Pnr
            else if (orderDocument.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs != null)
            {
                var flwnExists = orderDocument.OrderItems.FirstOrDefault().AccountableDocumentRefs.Any(x => x.Coupons.Any(x => x.CouponStatus == Constants.Application.AllFlownCouponStatus));
                if (flwnExists)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_1",
                    });
                }
            }

            if (errorIds.Count == 0)
            {

                // Check all the segments in the Order is operated by non - VS carrier
                var totalFlightsCount = orderDocument?.DataList.SegmentDetails.Count();
                var nonVS = orderDocument?.DataList.SegmentDetails.Count(x => x.OperatingCarrier != null);

                if (totalFlightsCount == nonVS)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_1",
                    });
                }
            }
            return errorIds;
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
                distributionChain.ToList().ForEach(dC =>
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
                });
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateServiceOfferDetails(IATA_ServiceListRQ serviceListReq)
        {
            var errors = new List<ErrorDetail>();
            var serviceOffers = (serviceListReq.Request.ServiceListCoreRequest.Item as ServiceListOfferRequestType).Offer;
            foreach (var offer in serviceOffers)
            {
                if (offer == null || string.IsNullOrEmpty(offer?.OfferID) || string.IsNullOrEmpty(offer?.OwnerCode) || offer?.OwnerCode != Constants.Application.OwnerCode)
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "912_1",
                        IsDynamicMessage = false
                    });
                }
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOfferDetails(OfferResponse offerResponse)
        {
            var errorIds = new List<ErrorDetail>();

            // Check offer is not found in the offer store.
            if (offerResponse == null || (offerResponse.Error?.Any() ?? false))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "913_1",
                });
            }
            else
            {
                int nonVSsegment = 0;
                offerResponse?.DataList?.FlightSegments?.ToList().ForEach(fs =>
                {
                    if (fs.OperatingCarrier != Constants.Application.OwnerCode && fs.OperatingCarrier != null)
                    {
                        nonVSsegment++;

                    }
                });
                if (offerResponse.DataList?.FlightSegments?.ToList().Count == nonVSsegment)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        ErrorCode = "76H_3",
                        IsDynamicMessage = false
                    });
                }
            }
            return errorIds;
        }

    }
}
