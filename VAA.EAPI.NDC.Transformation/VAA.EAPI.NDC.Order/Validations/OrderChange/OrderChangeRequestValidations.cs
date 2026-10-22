using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using OrderDocument = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderDocument;

namespace VAA.EAPI.NDC.Order.Validations.OrderChange
{
    public class OrderChangeRequestValidations
    {
        public static IEnumerable<ErrorDetail> ValidateOrderChangeReqDistributionEntity<T>(DistributionChainLinkType[] distributionChain)
        {
            {
                var errors = new List<ErrorDetail>();

                if (!distributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Carrier))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "914_17",
                        IsDynamicMessage = false
                    });
                }

                if (errors.Count == 0)
                {
                    foreach (var dc in distributionChain)
                    {
                        var emailId = dc.ContactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText;
                        var contactInfo = dc.ContactInfo?.ContactInfoID;

                        if (dc.ParticipatingOrg is null || string.IsNullOrEmpty(dc.ParticipatingOrg.OrgID))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "912_21"
                            });
                        }
                        else if (dc.OrgRole == DistributionChainRoleCodeType.Carrier && dc.Ordinal != distributionChain.Length.ToString())
                        {
                            errors.Add(new ErrorDetail
                            {
                                ErrorCode = "912_24",
                                IsDynamicMessage = false
                            });
                        }
                        else if (dc.OrgRole == DistributionChainRoleCodeType.Carrier && dc.ParticipatingOrg.OrgID != Constants.Application.OwnerAirline)
                        {
                            errors.Add(new ErrorDetail
                            {
                                ErrorCode = "914_17",
                                IsDynamicMessage = false
                            });
                        }
                        else if ((dc.SalesAgent != null && string.IsNullOrEmpty(dc.SalesAgent.SalesAgentID)) || (dc.SalesBranch != null && string.IsNullOrEmpty(dc.SalesBranch.SalesBranchID)))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "913_4"
                            });
                        }
                        else if (string.IsNullOrEmpty(dc.Ordinal) || (dc.Ordinal.Equals("0")) || (dc.Ordinal.Any(Char.IsLetter)) || (dc.OrgRole == DistributionChainRoleCodeType.Seller && !dc.Ordinal.StartsWith("1")))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "913_4"
                            });
                        }
                        else if (contactInfo.IsNullOrEmpty() && dc.OrgRole.Equals("Seller"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "912_34"
                            });
                        }
                        else if (!string.IsNullOrEmpty(emailId) && (emailId.Length > Convert.ToInt32(Constants.Application.EmailMaxLength) || !RegexCompiled.Match(emailId, Constants.Application.RegexToValidateEmail).Success))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "317_1"
                            });
                        }

                    }
                }

                return errors;
            }

        }

        public static IEnumerable<ErrorDetail> ValidateIFGAgentsPermissionErrorResponse(IATA_OrderChangeRQ request, IFGAgentFopLookUpResponse ifgAgentFopLookUpResponse)
        {
            var cashPaymentMethodType = (request.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as SettlementPlanType)?.PaymentTypeCode;
            var easyPayPaymentMethodType = request.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as PaymentProcessingIATA_EasyPayType;

            if (ifgAgentFopLookUpResponse is null)

                return [new() { IsDynamicMessage = false, ErrorCode = "911_50" }];

            if (ifgAgentFopLookUpResponse.HasTicketingAuthority is false)

                return [new() { IsDynamicMessage = false, ErrorCode = "368_11" }];

            if (cashPaymentMethodType != null && ifgAgentFopLookUpResponse.BspCashAllowed is false)

                return [new() { IsDynamicMessage = false, ErrorCode = "368_9" }];

            if (easyPayPaymentMethodType != null && ifgAgentFopLookUpResponse.EasyPayAllowed is false)

                return [new() { IsDynamicMessage = false, ErrorCode = "368_10" }];

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOrderChangeReqOrder<T>(IATA_OrderChangeRQ request)
        {
            var errors = new List<ErrorDetail>();

            if (request.Request == null || request.Request.Order == null || request.Request.Order.OrderID.IsNullOrEmpty())
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_29" });

            if (!string.IsNullOrEmpty(request.Request.Order?.OrderID) && !RegexCompiled.Match(request.Request.Order?.OrderID, Constants.Application.RegexToValidateAlphaNumeric).Success)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "914_3" });

            if (request.Request == null || request.Request.Order == null || request.Request.Order.OwnerCode.IsNullOrEmpty())
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_26" });

            if (!string.IsNullOrEmpty(request.Request.Order?.OwnerCode) && request.Request.Order?.OwnerCode?.Length != 2)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_40" });

            return errors;

        }

        public static IEnumerable<ErrorDetail> ValidateCancelUnpaidOrder(IATA_OrderChangeRQ request)
        {
            var errors = new List<ErrorDetail>();
            var cancelUnpaidOrder = request?.Request?.ChangeOrderChoice?.Item as CancelUnpaidOrderType;
            var order = request?.Request?.Order;
            var paxList = request?.Request?.DataLists?.PaxList;

            if (cancelUnpaidOrder.OrderRefID.IsNullOrEmpty() || !string.Equals(cancelUnpaidOrder.OrderRefID, order?.OrderID, StringComparison.Ordinal) ||cancelUnpaidOrder.SelectedOrderItem?.Any() == true)
            {
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_16" });
            }

            if (cancelUnpaidOrder.OwnerCode.IsNullOrEmpty()
                || !string.Equals(cancelUnpaidOrder.OwnerCode, order?.OwnerCode, StringComparison.Ordinal)
                || !string.Equals(cancelUnpaidOrder.OwnerCode, Constants.Application.OwnerAirline, StringComparison.Ordinal))
            {
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_40" });
            }

            if (paxList.IsNullOrEmpty())
            {
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_30" });
            }
            else
            {
                errors.AddRange(ValidatePaxList(request));
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderChangeReqChangeOrderChoice<T>(IATA_OrderChangeRQ request)
        {
            var errors = new List<ErrorDetail>();

            var acceptSelectedQuotedOfferListType = (AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item;

            if (acceptSelectedQuotedOfferListType.SelectedPricedOffer == null || acceptSelectedQuotedOfferListType.SelectedPricedOffer.Any(x => x.OfferRefID.IsNullOrEmpty()))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_25" });

            if (acceptSelectedQuotedOfferListType.SelectedPricedOffer == null || acceptSelectedQuotedOfferListType.SelectedPricedOffer.Any(x => x.OwnerCode.IsNullOrEmpty()))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_26" });

            if (acceptSelectedQuotedOfferListType.SelectedPricedOffer == null || acceptSelectedQuotedOfferListType.SelectedPricedOffer.Any(x => x.SelectedOfferItem == null || x.SelectedOfferItem.Any(y => y.OfferItemRefID.IsNullOrEmpty())))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_27" });

            if (acceptSelectedQuotedOfferListType.SelectedPricedOffer == null || acceptSelectedQuotedOfferListType.SelectedPricedOffer.Any(x => x.SelectedOfferItem.Any(y => y.PaxRefID.Length == 0)) || (!request.Request.DataLists.PaxList.IsNullOrEmpty() && acceptSelectedQuotedOfferListType.SelectedPricedOffer.Any(x => x.SelectedOfferItem.Any(y => y.PaxRefID.Any(z => !request.Request.DataLists.PaxList.Any(q => q.PaxID == z))))))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_30" });

            if (!request.Request.DataLists.PaxList.IsNullOrEmpty() && request.Request.DataLists.PaxList.Any(q => q.PaxID.IsNullOrEmpty()))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_28" });

            return errors;

        }

        public static IEnumerable<NDCWarning> ValidateGSTNumberandOrgName(IATA_OrderChangeRQ request)
        {
            var warnings = new List<NDCWarning>();

            foreach (var y in request.Request.DataLists.PaxList)
            {
                if (y.Employer != null)
                {
                    if (y.Employer.Name.IsNullOrEmpty() || !RegexCompiled.Match(y.Employer.Name, Constants.Application.RegexToValidateAlphaNumeric).Success
                            || !(y.Employer.Name?.Length <= 35))
                        warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "914_11", Type = "Warning" });

                    var salesTaxRegistration = y.Employer.SalesTaxRegistration?.FirstOrDefault();

                    if (salesTaxRegistration == null || string.IsNullOrEmpty(salesTaxRegistration?.SalesTaxRegistrationID) || !RegexCompiled.Match(salesTaxRegistration?.SalesTaxRegistrationID, Constants.Application.RegexToValidateAlphaNumeric).Success
                            || !(salesTaxRegistration?.SalesTaxRegistrationID?.Length <= 20))
                        warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "914_10", Type = "Warning" });
                }
            }

            return warnings;
        }

        public static IEnumerable<NDCWarning> ValidateGSTAddress(IATA_OrderChangeRQ request)
        {
            foreach (var y in request.Request.DataLists.PaxList)
            {
                if (y.Employer != null)
                {
                    var salesTaxRegistration = y.Employer.SalesTaxRegistration?.FirstOrDefault();

                    if (salesTaxRegistration == null || (salesTaxRegistration.CityName.IsNullOrEmpty() || !RegexCompiled.Match(salesTaxRegistration.CityName, Constants.Application.RegexToValidateAlphaNumeric).Success || salesTaxRegistration.CityName?.Length > 25 ||
                    salesTaxRegistration.CountryCode.IsNullOrEmpty() || !RegexCompiled.Match(salesTaxRegistration.CountryCode, Constants.Application.RegexToValidateAlphaNumeric).Success || salesTaxRegistration.CountryCode?.Length > 3 ||
                    salesTaxRegistration.PostalCode.IsNullOrEmpty() || !RegexCompiled.Match(salesTaxRegistration.PostalCode, Constants.Application.RegexToValidateAlphaNumeric).Success || salesTaxRegistration.PostalCode?.Length > 17 ||
                    salesTaxRegistration.StreetText != null ? salesTaxRegistration.StreetText.Any(x => x.IsNullOrEmpty()) : true || salesTaxRegistration.StreetText != null ? salesTaxRegistration.StreetText.Any(x => !RegexCompiled.Match(x, Constants.Application.RegexToValidateAlphaNumeric).Success) : true ||
                    salesTaxRegistration.StreetText?.Length > 0 ? salesTaxRegistration.StreetText.First().Length > 35 : true || salesTaxRegistration.StreetText?.Length == 2 ? salesTaxRegistration.StreetText.Last().Length > 25 : false))
                        return new List<NDCWarning>() { new() { IsDynamicMessage = false, Code = "914_13", Type = "Warning" } };
                }
            }

            return Enumerable.Empty<NDCWarning>();
        }

        public static IEnumerable<NDCWarning> ValidateGSTEmailAddress(IATA_OrderChangeRQ request)
        {
            foreach (var y in request.Request.DataLists.PaxList)
            {
                if (y.Employer != null)
                {
                    var salesTaxRegistration = y.Employer.SalesTaxRegistration?.FirstOrDefault();

                    if (salesTaxRegistration == null || (salesTaxRegistration.EmailAddressText.IsNullOrEmpty() || salesTaxRegistration.EmailAddressText?.Length > 35))
                        return new List<NDCWarning>() { new() { IsDynamicMessage = false, Code = "914_14", Type = "Warning" } };
                }
            }

            return Enumerable.Empty<NDCWarning>();
        }

        public static IEnumerable<NDCWarning> ValidateGSTPhone(IATA_OrderChangeRQ request)
        {
            foreach (var y in request.Request.DataLists.PaxList)
            {
                if (y.Employer != null)
                {
                    var salesTaxRegistration = y.Employer.SalesTaxRegistration?.FirstOrDefault();

                    if (salesTaxRegistration == null || (salesTaxRegistration.CountryDialingCode.IsNullOrEmpty() ||
                    salesTaxRegistration.PhoneNumber.Any(x => x.IsNullOrEmpty() || $"{salesTaxRegistration.CountryDialingCode}{x}".Length > 15 || !RegexCompiled.Match($"{salesTaxRegistration.CountryDialingCode}{x}", Constants.Application.RegexToValidateNumeric).Success)))
                        return new List<NDCWarning>() { new() { IsDynamicMessage = false, Code = "914_15", Type = "Warning" } };
                }
            }

            return Enumerable.Empty<NDCWarning>();
        }

        public static IEnumerable<NDCWarning> ValidateGSTAgainstOrderDoc(OrderDocument orderDocument, IATA_OrderChangeRQ request)
        {
            if (request.Request.DataLists.PaxList.Any(x => x.Employer != null && orderDocument.DataList.CustomerDetails.First(c => c.CustId == x.PaxID).Employer != null))
                return [new NDCWarning()
                {
                    Code = "911_43",
                    Type = "Warning",
                    IsDynamicMessage = false
                }];

            return [];
        }

        public static IEnumerable<ErrorDetail> ValidateOrderIDHoldBooking<T>(IATA_OrderChangeRQ request)
        {
            var errors = new List<ErrorDetail>();
            if (request.Request == null || request.Request.Order == null || request.Request.Order.OrderID.IsNullOrEmpty())
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_15" });

            if (!string.IsNullOrEmpty(request.Request.Order?.OrderID) && !RegexCompiled.Match(request.Request.Order?.OrderID, Constants.Application.RegexToValidateAlphaNumeric).Success)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "914_3" });

            if (string.IsNullOrEmpty(request.Request.Order?.OwnerCode))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_26" });

            if (!string.IsNullOrEmpty(request.Request.Order?.OwnerCode) && request.Request.Order?.OwnerCode?.Length != 2)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_40" });

            if (request.Request.BookingRef == null || string.IsNullOrEmpty(request.Request.BookingRef?.FirstOrDefault()?.BookingID))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_4" });

            if (!string.IsNullOrEmpty(request.Request.BookingRef?.FirstOrDefault()?.BookingID) && !string.IsNullOrEmpty(request.Request.Order?.OrderID) && (request.Request.BookingRef?.FirstOrDefault()?.BookingID.Length != 6 || request.Request.BookingRef?.FirstOrDefault()?.BookingID != request.Request.Order?.OrderID?.Substring(5, 6)))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "914_5" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateDistributionEntityHoldBooking<T>(DistributionChainLinkType[] distributionChain)
        {
            var errors = new List<ErrorDetail>();
            var sortedDistributedChain = distributionChain.Any(x => x.Ordinal.IsNullOrEmpty()) ? null : distributionChain.OrderByDescending(x => x.Ordinal).FirstOrDefault();

            if (sortedDistributedChain == null || sortedDistributedChain.Ordinal.IsNullOrEmpty() || Convert.ToString(sortedDistributedChain?.OrgRole).IsNullOrEmpty() || sortedDistributedChain.ParticipatingOrg.Name.IsNullOrEmpty())
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_24" });

            if (!errors.Any() && distributionChain.Any(x => x.Ordinal.Any(Char.IsLetter) ))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "913_4" });

            if ((sortedDistributedChain.OrgRole == DistributionChainRoleCodeType.Carrier) && (sortedDistributedChain.Ordinal != distributionChain.Length.ToString() || sortedDistributedChain.ParticipatingOrg.OrgID != Constants.Application.OwnerAirline))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "913_4" });

            var emailId = distributionChain.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller)?.ContactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText;

            if (emailId.IsNullOrWhiteSpace())
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "316_1" });

            var contactInfo = distributionChain.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller)?.ContactInfo?.ContactInfoID;

            if (contactInfo.IsNullOrEmpty())
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_34" });

            if (!string.IsNullOrEmpty(emailId) && (emailId.Length > Convert.ToInt32(Constants.Application.EmailMaxLength) || !RegexCompiled.Match(emailId, Constants.Application.RegexToValidateEmail).Success))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "317_1" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOfferExpiryTimeLimit(OfferResponse offerDoc)
        {
            if (offerDoc == null || offerDoc.Offers == null) return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_42" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOrderChangeRQBookingIDOfferStore(IATA_OrderChangeRQ request, OfferResponse offerDoc)
        {
            var errors = new List<ErrorDetail>();

            if (request.Request.BookingRef == null || string.IsNullOrEmpty(request.Request.BookingRef?.FirstOrDefault()?.BookingID))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_4" });

            if (!string.IsNullOrEmpty(request.Request.BookingRef?.FirstOrDefault()?.BookingID) && request.Request.BookingRef?.FirstOrDefault()?.BookingID != request.Request.Order?.OrderID?.Substring(5, 6))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "914_5" });

            if (!RegexCompiled.Match(request.Request.BookingRef?.FirstOrDefault()?.BookingID, "^[A-Z0-9]*$").Success || request.Request.BookingRef?.FirstOrDefault()?.BookingID.Length != Constants.Application.PNRLength)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "914_5" });

            if (!string.IsNullOrEmpty(request.Request.BookingRef?.FirstOrDefault()?.BookingID) && !string.IsNullOrEmpty(offerDoc?.OrderId) && request.Request.BookingRef?.FirstOrDefault()?.BookingID != offerDoc.OrderId?.Substring(5, 6))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "129_2" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderChangeRQOrderOfferStore(IATA_OrderChangeRQ request, OfferResponse offerDoc)
        {
            if (!string.IsNullOrEmpty(request.Request.Order?.OrderID) && request.Request.Order.OrderID != offerDoc.OrderId)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "129_3" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOrderChangeRQPaxIDOfferDoc(AcceptSelectedQuotedOfferListType acceptSelectedQuotedOfferList, OfferResponse offerDoc)
        {
            var selectedofferDocOfferItems = offerDoc.Offers.FirstOrDefault(x => x.OfferId == acceptSelectedQuotedOfferList.SelectedPricedOffer.FirstOrDefault().OfferRefID)?.OfferItems;
            var requestOfferItems = acceptSelectedQuotedOfferList.SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null))?.SelectedOfferItem;

            foreach (var offerItem in selectedofferDocOfferItems)
            {
                var matchingOfferItem = requestOfferItems.FirstOrDefault(x => x.OfferItemRefID == offerItem.OfferItemId);

                if (matchingOfferItem == null || offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries?.Count() != matchingOfferItem.PaxRefID.Length || matchingOfferItem.PaxRefID.Any(x => !offerItem.Services.FirstOrDefault().ServiceBeneficiaries.Contains(x)))
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_37" } };
            }

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOrderChangeExchangeOffer(IATA_OrderChangeRQ request, OfferResponse offerDoc, bool isEvenExchange = false, bool isUnEvenExchangeRefund = false, bool isUnEvenExchangeAddCollect = false)
        {
            var errors = new List<ErrorDetail>();
            var totalAddCollectAmount = 0m;
            var totalRefundAmount = 0m;
            var requestAmount = request.Request.PaymentFunctions?.FirstOrDefault().PaymentProcessingDetails.Amount;

            foreach (var offerItem in offerDoc.Offers.First().OfferItems)
            {
                var addCollectPrice = offerItem.OfferitemPricing.Select(oiPricing => oiPricing?.AddCollectPrice)?.FirstOrDefault();
                var refundPrice = offerItem.OfferitemPricing.Select(oiPricing => oiPricing?.RefundPrice)?.FirstOrDefault();

                totalAddCollectAmount += Convert.ToDecimal(addCollectPrice?.Amount?.MonetaryAmount) * offerItem.Services.First().ServiceBeneficiaries.Count();
                totalRefundAmount += Convert.ToDecimal(refundPrice?.FirstOrDefault()?.Amount?.MonetaryAmount) * offerItem.Services.First().ServiceBeneficiaries.Count();
            }

            if (isEvenExchange && (totalAddCollectAmount != requestAmount.Value || totalRefundAmount != requestAmount.Value))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_36" });

            else if (isUnEvenExchangeRefund && (totalAddCollectAmount != 0m || totalRefundAmount >= 0m))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_36" });

            else if (isUnEvenExchangeAddCollect)
            {
                var baseCurrency = offerDoc.Offers.SelectMany(o => o.OfferItems).SelectMany(oi => oi.OfferitemPricing).FirstOrDefault(p => p?.AddCollectPrice != null)?.AddCollectPrice?.Amount?.BaseCurrency;
                if (totalAddCollectAmount - totalRefundAmount != requestAmount.Value || baseCurrency != requestAmount.CurCode)
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_46" });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidationOrderChangeRQChangeOrderChoiceOfferStore(AcceptSelectedQuotedOfferListType acceptSelectedQuotedOfferList, OfferResponse offerDoc)
        {
            var errors = new List<ErrorDetail>();

            foreach (var offer in acceptSelectedQuotedOfferList.SelectedPricedOffer.Where(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem == null)))
            {
                if (offer.OfferRefID != offerDoc.Offers.FirstOrDefault().OfferId)
                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_39" });

                if (offer.OwnerCode.ToUpper() != offerDoc?.Offers?.FirstOrDefault().OfferItems?.FirstOrDefault().Services?.FirstOrDefault().Product.ProductOwner.ToUpper())
                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_40" });

                if (offer.SelectedOfferItem.Any(x => !offerDoc.Offers.FirstOrDefault().OfferItems.Any(y => y.OfferItemId == x.OfferItemRefID)))
                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_41" });

                if (offer.SelectedOfferItem.Length != offerDoc.Offers.FirstOrDefault().OfferItems.Count())
                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_9" });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidatePostSaleOrderChangeSeatOfferDoc(IATA_OrderChangeRQ request, SeatAvailabilityResponse seatofferDoc)
        {
            if (seatofferDoc == null)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_34" } };
            else
            {
                var docAgencyId = seatofferDoc.Parties?.Participants?.Count() > 1 ? seatofferDoc.Parties?.Participants?.First(x => x.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller).AgencyID : string.Empty;
                var docAgencyIATANumber = seatofferDoc.Parties?.Participants?.Count() > 1 ? seatofferDoc.Parties?.Participants?.First(x => x.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller).AgencyID : string.Empty;
                var docParticipantId = seatofferDoc.Parties?.Participants?.Count() > 1 ? seatofferDoc.Parties?.Participants?.First(x => x.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller).Name : string.Empty;
                var docParticipantIATANumber = seatofferDoc.Parties?.Participants?.Count() > 1 ? seatofferDoc.Parties?.Participants?.First(x => x.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller).AgencyID : string.Empty;
                var sortedDistributionChain = !string.IsNullOrEmpty(request.DistributionChain?.FirstOrDefault()?.Ordinal) ? request.DistributionChain?.OrderBy(x => int.Parse(x?.Ordinal)) : null;
                var reqAgencyId = sortedDistributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller)?.ParticipatingOrg?.OrgID;
                var reqAgencyIATANumber = sortedDistributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller)?.ParticipatingOrg?.OrgID;
                var reqParticipantId = sortedDistributionChain?.Count() > 1 ? sortedDistributionChain.First(x => x.OrgRole == DistributionChainRoleCodeType.Seller).ParticipatingOrg?.Name : string.Empty;
                var reqParticipantIATANumber = sortedDistributionChain?.Count() > 1 ? sortedDistributionChain.First(x => x.OrgRole == DistributionChainRoleCodeType.Seller).ParticipatingOrg?.OrgID : string.Empty;

                if (docAgencyId != reqAgencyId || docAgencyIATANumber != reqAgencyIATANumber || docParticipantId != reqParticipantId || docParticipantIATANumber != reqParticipantIATANumber)
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "368_1" } };

                else
                    return Enumerable.Empty<ErrorDetail>();
            }
        }

        public static IEnumerable<ErrorDetail> ValidateEvenOrderChangeOfferDoc(IATA_OrderChangeRQ request, OfferResponse offerDoc)
        {
            if (offerDoc == null)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_34" } };
            else
            {
                var docAgencyId = offerDoc.Parties.Participants.Any() ? offerDoc.Parties.Participants.OrderBy(x => x.Ordinal).FirstOrDefault().Name : offerDoc.Parties.Participants.OrderBy(x => x.Ordinal).FirstOrDefault().Name;
                var docAgencyIATANumber = offerDoc.Parties.Participants.Any() ? offerDoc.Parties.Participants.OrderBy(x => x.Ordinal).FirstOrDefault().AgencyID : offerDoc.Parties.Participants.OrderBy(x => x.Ordinal).FirstOrDefault().AgencyID;
                var docParticipantId = offerDoc.Parties.Participants.Any() ? offerDoc.Parties.Participants.OrderBy(x => x.Ordinal).FirstOrDefault().Name : string.Empty;
                var docParticipantIATANumber = offerDoc.Parties.Participants.Any() ? offerDoc.Parties.Participants.OrderBy(x => x.Ordinal).FirstOrDefault().AgencyID : string.Empty;

                var sortedDistributionChain = (request.DistributionChain.Where(x => x.OrgRole != Domain.OrderChangeRQ.DistributionChainRoleCodeType.Carrier).OrderByDescending(x => int.Parse(x?.Ordinal)));
                var reqAgencyId = sortedDistributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller)?.ParticipatingOrg?.Name;
                var reqAgencyIATANumber = sortedDistributionChain?.FirstOrDefault(x => x.OrgRole == DistributionChainRoleCodeType.Seller)?.ParticipatingOrg?.OrgID;
                var reqParticipantId = sortedDistributionChain.Count() == 2 ? sortedDistributionChain.Last()?.ParticipatingOrg.Name : reqAgencyId;
                var reqParticipantIATANumber = sortedDistributionChain.Count() == 2 ? sortedDistributionChain.Last()?.ParticipatingOrg.OrgID : reqAgencyIATANumber;

                if (docAgencyId != reqAgencyId || docAgencyIATANumber != reqAgencyIATANumber || docParticipantId != reqParticipantId || docParticipantIATANumber != reqParticipantIATANumber)
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "368_1" } };

                else
                    return Enumerable.Empty<ErrorDetail>();
            }


        }

        public static IEnumerable<ErrorDetail> ValidatePostSaleOrderChangeRQOfferItemIdandPrice(IATA_OrderChangeRQ request, SeatAvailabilityResponse seatofferDoc)
        {
            var offerItemList = ((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType)?.SelectedPricedOffer ?? Enumerable.Empty<SelectedPricedOfferType>())
                .SelectMany(o => o?.SelectedOfferItem ?? Enumerable.Empty<SelectedOfferItemType>())
                .Where(i => i?.SelectedSeat != null)
                .Where(i => ((i.SelectedALaCarteOfferItem?.OfferFlightAssociations?.Item as PaxSegmentRefListType)?.PaxSegmentRefID) == null)
                .ToList();
            var isofferItemIdPresent = offerItemList.All(item => seatofferDoc.FlightSegmentSeatMap.Any(x => x.Catalogue.Any(x => x.CatalogueId == item.OfferItemRefID)));

            if (!isofferItemIdPresent)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_36" } };

            if ((request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.Amount != null && (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount == null) || (request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.Amount == null && (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount != null && Convert.ToDecimal((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount.Value) != 0))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "290_4" } };

            if (request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.Amount != null && (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount != null)
            {
                double seatDocAmount = default;
                var totalAmountInPayment = Convert.ToDouble(request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.Amount.Value);
                var totalSeatAmount = Convert.ToDouble((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount?.Value);
                var seatOfferDocCurCode = seatofferDoc.FlightSegmentSeatMap?.FirstOrDefault()?.Catalogue?.FirstOrDefault()?.SeatPrice?.BaseCurrency;
                var paymentCurCode = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.Amount?.CurCode;
                var totalSeatAmountCurCode = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount?.CurCode;

                seatDocAmount += offerItemList.SelectMany(t => seatofferDoc.FlightSegmentSeatMap.SelectMany(x => x.Catalogue).Where(x => t.OfferItemRefID == x.CatalogueId)).Sum(x => Convert.ToDouble(x.SeatPrice.MonetaryAmount));

                if ((totalSeatAmount != totalAmountInPayment) || (totalSeatAmount != seatDocAmount) || (totalAmountInPayment != seatDocAmount))
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "290_4" } };

                if (paymentCurCode != null && totalSeatAmountCurCode != paymentCurCode)
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_3" } };

                if ((paymentCurCode != null && paymentCurCode != seatOfferDocCurCode) || (totalSeatAmountCurCode != null && totalSeatAmountCurCode != seatOfferDocCurCode))
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_3" } };
            }

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidatePostSaleOrderChangeRQSeat(IATA_OrderChangeRQ request, IEnumerable<SeatDetails> seatDetails)
        {

            if ((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault().SelectedOfferItem.Any(oi => !RegexCompiled.Match(oi.SelectedSeat.ColumnID, @"^[a-zA-Z]{1}$").Success))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "290_5" } };

            var duplicateSeatInSameSegment = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType)?.SelectedPricedOffer?.Where(o => o?.SelectedOfferItem?.Any(i => i?.SelectedSeat != null) == true).SelectMany(o => o?.SelectedOfferItem ?? Enumerable.Empty<SelectedOfferItemType>()).Where(i => i?.SelectedSeat != null).GroupBy(i => new { i.OfferItemRefID, Seat = $"{i.SelectedSeat.SeatRowNumber}-{i.SelectedSeat.ColumnID}" }).Any(g => g.Count() > 1) ?? false;

            if (duplicateSeatInSameSegment)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "290_7" } };

            else
            {
                var twoSeatForSameInSegment = seatDetails.GroupBy(x => new { x?.AssociatedSegment?.SegmentId, x?.SeatNumber }).Any(g => g.Count() > 1);
                var twoSeatForSamePaxInSegment = seatDetails.GroupBy(x => new { x?.AssociatedSegment?.SegmentId, x?.Beneficiary?.CustId }).Any(g => g.Count() > 1);

                if (twoSeatForSameInSegment)
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "290_7" } };

                if (twoSeatForSamePaxInSegment)
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "290_6" } };
            }

            return Enumerable.Empty<ErrorDetail>();

        }

        public static IEnumerable<ErrorDetail> ValidatePostSaleOrderChangeSeatAmountAndCurInSeatMap(IATA_OrderChangeRQ request, IEnumerable<SeatDetails> seatDetails, SeatAvailabilityResponse seatofferDoc)
        {
            var errors = new List<ErrorDetail>();
            var totalSeatAmount = Convert.ToDouble((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount?.Value);
            var totalSeatCurCode = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount?.CurCode;
            double totalSeatPriceInSeatMap = seatDetails.Where(x => x.SeatPrice != null).Sum(x => Convert.ToDouble(x.SeatPrice));

            foreach (var (seatDetail, seatMap) in seatDetails.SelectMany(seatDetail => seatofferDoc.FlightSegmentSeatMap.Where(seatMap => seatMap.FlightSegment.SegmentId == seatDetail.AssociatedSegment.SegmentId).Select(seatMap => (seatDetail, seatMap))))
            {
                var SeatColumnsList = seatMap.Seatmap.SelectMany(y => y.SeatRows.SelectMany(z => z.SeatColumns));
                var seatColumn = SeatColumnsList?.FirstOrDefault(sc => sc.SeatIdentifer == seatDetail.SeatNumber);

                if (seatColumn == null)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = true,
                        ErrorCode = "256_1",
                        DynamicMessagesContents = new List<DynamicMessagesContent>()
                        {
                            new DynamicMessagesContent()
                            {
                                Fieldname = "<<SeatNumber>>",
                                ReplaceWith = seatDetail.AssociatedSegment?.MarketingCarrier + seatDetail.AssociatedSegment?.MarketingFlightNumber +"-" + seatDetail.SeatNumber
                            }
                        }
                    });
                    seatDetail.IsInvalid = true;
                }
            }

            if (!errors.Any() && (totalSeatPriceInSeatMap > totalSeatAmount))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "290_4",
                });
            }

            if (!errors.Any() && !totalSeatCurCode.IsNullOrEmpty() && seatDetails?.FirstOrDefault(x => x.IsInvalid == false)?.Currency != null && seatDetails?.FirstOrDefault(x => x.IsInvalid == false)?.Currency != totalSeatCurCode)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "729_1",
                });
            }

            return errors;
        }

        public static IEnumerable<NDCWarning> ValidatePostSaleOrderChangeSeatMapForWarning(IATA_OrderChangeRQ request, IEnumerable<SeatDetails> seatDetails, SeatAvailabilityResponse seatofferDoc)
        {
            var warnings = new List<NDCWarning>();
            var invalidSegmentIDs = seatDetails.Where(s => s.IsInvalid).Select(s => s.AssociatedSegment?.SegmentId);
            var totalSeatAmount = Convert.ToDouble((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount?.Value);
            var totalSeatCurCode = (request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null)).FirstOrDefault()?.TotalOfferPriceAmount?.CurCode;
            double totalSeatPriceInSeatMap = default;

            foreach (var (seatDetail, seatMap) in seatDetails.SelectMany(seatDetail => seatofferDoc.FlightSegmentSeatMap.Where(seatMap => seatMap.FlightSegment?.SegmentId == seatDetail.AssociatedSegment?.SegmentId).Select(seatMap => (seatDetail, seatMap))))
            {
                var SeatColumnsList = seatMap.Seatmap?.SelectMany(y => y.SeatRows.SelectMany(z => z.SeatColumns));
                var seatColumn = SeatColumnsList?.FirstOrDefault(sc => sc.SeatIdentifer == seatDetail.SeatNumber);

                if (seatColumn?.Available == false)
                {
                    seatDetail.IsInvalid = true;
                    warnings.Add(new()
                    {
                        Code = "281_1",
                        Type = "Warning",
                        IsDynamicMessage = true,
                        DynamicMessagesContents = new List<DynamicMessagesContent>()
                        {
                            new DynamicMessagesContent()
                            {
                                Fieldname = "<<SeatNumber>>",
                                ReplaceWith = seatDetail.AssociatedSegment?.MarketingCarrier + seatDetail.AssociatedSegment?.MarketingFlightNumber +"-" + seatDetail.SeatNumber
                            }
                        }
                    });
                }

                if (seatColumn?.Available == true)
                {
                    var custID = seatDetail.Beneficiary?.LastNameNumber + "-" + seatDetail.Beneficiary?.FirstNameNumber;
                    var eligibilityCatalogueID = seatColumn.Eligibility?.FirstOrDefault(e => e.CustomerId.First() == custID)?.CatalogueRefId;
                    var catalogue = seatMap.Catalogue?.FirstOrDefault(c => c.CatalogueId == eligibilityCatalogueID);

                    seatDetail.SeatPrice = catalogue?.SeatPrice?.MonetaryAmount;
                    totalSeatPriceInSeatMap += Convert.ToDouble(seatDetail.SeatPrice);
                }
            }

            if (totalSeatPriceInSeatMap < totalSeatAmount)
            {
                warnings.Add(new()
                {
                    Code = "992_1",
                    Type = "Warning",
                    IsDynamicMessage = true,
                    DynamicMessagesContents = new List<DynamicMessagesContent>()
                    {
                        new DynamicMessagesContent()
                        {
                            Fieldname = "<<TotalAmount>>",
                            ReplaceWith = totalSeatPriceInSeatMap + totalSeatCurCode
                        }
                    }
                });
            }

            foreach (var s in seatDetails)
            {
                if (!s.IsInvalid && invalidSegmentIDs.Contains(s.AssociatedSegment?.SegmentId))
                {
                    warnings.Add(new()
                    {
                        Code = "290_1",
                        Type = "Warning",
                        IsDynamicMessage = true,
                        DynamicMessagesContents = new List<DynamicMessagesContent>()
                        {
                            new DynamicMessagesContent()
                            {
                                Fieldname = "<<SeatNumber>>",
                                ReplaceWith = s.AssociatedSegment?.MarketingCarrier + s.AssociatedSegment?.MarketingFlightNumber + "-" + s.SeatNumber
                            }
                        }
                    });

                    s.IsInvalid = true;
                }
            }

            return warnings;
        }

        public static IEnumerable<ErrorDetail> ValidatePostOrderChangeRequestBookingId<T>(IATA_OrderChangeRQ request)
        {
            var errors = new List<ErrorDetail>();

            if (request?.Request?.BookingRef?.Length != 0 && string.IsNullOrEmpty(request.Request.BookingRef.FirstOrDefault().BookingID))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_4",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidatePostOrderChangeRequestSeatNumber<T>(IATA_OrderChangeRQ request)
        {
            var acceptSelectedQuotedOfferListType = ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item)?.SelectedPricedOffer?.Where(x => x?.SelectedOfferItem != null && x.SelectedOfferItem.Any(i => i?.SelectedSeat != null));
            
            if (acceptSelectedQuotedOfferListType.FirstOrDefault().SelectedOfferItem.Any(x => x.SelectedSeat == null || x.SelectedSeat?.SeatRowNumber == decimal.Zero))
            {
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "912_33" } };
            }

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidatePaxList(IATA_OrderChangeRQ request)
        {
            PaxType[] paxList = request.Request.DataLists.PaxList;
            var errors = new List<ErrorDetail>();

            if (paxList.IsNullOrEmpty())
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "143_1",
                });
            }
            else
            {
                var countAdt = paxList.Where(x => x.PTC == Constants.Application.AdultPTCCode).Count();
                var countChd = paxList.Where(x => x.PTC == Constants.Application.ChildPTCCode).Count();
                var countYoung = paxList.Where(x => x.PTC == Constants.Application.YoungPTCCode).Count();
                var countInf = paxList.Where(x => x.PTC == Constants.Application.InfantPTCCode).Count();
                var countDistinctPaxId = paxList.Select(x => x.PaxID).Distinct().Count();

                if (countAdt < countInf)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "324_1",
                    });
                }

                if ((countAdt + countChd + countYoung) > 9)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "68_1",
                    });
                }

                if (((countAdt + countChd + countInf + countYoung) != paxList.Length) || countAdt == 0 || countDistinctPaxId < paxList.Length)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "143_2",
                    });
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSplitPnrPaxList(OrderRS orderDataRS, AugmentationPoint augmentationPoint)
        {
            var customerDetails = orderDataRS?.OrderDocument?.DataList?.CustomerDetails;
            var adults = customerDetails?.Where(x => x.Ptc == Constants.Application.AdultPTCCode);
            var adultsWithInfants = adults?.Where(a => !string.IsNullOrEmpty(a.LinkedInfantId));
            var infants = customerDetails?.Where(x => x.Ptc == Constants.Application.InfantPTCCode);
            var childPnrPax = customerDetails.Where(c => augmentationPoint.PaxRefID.Contains(c.CustId));
            var parentPnrPax = customerDetails.Where(c => !augmentationPoint.PaxRefID.Contains(c.CustId));

            bool hasChildOrYouth = childPnrPax.Any(c =>c.Ptc == Constants.Application.ChildPTCCode || c.Ptc == Constants.Application.YoungPTCCode);
            bool hasAdult = childPnrPax.Any(c => c.Ptc == Constants.Application.AdultPTCCode);
            bool hasInfant = customerDetails.Any(c => c.Ptc == Constants.Application.InfantPTCCode);

            var errors = new List<ErrorDetail>();

            if (orderDataRS?.OrderDocument == null)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_1",
                });
            }

            else
            {
                int adultCount = adults?.Count() ?? 0;
                int paxRefIdCount = augmentationPoint.PaxRefID?.Count() ?? 0;
                int customerDetailsCount = customerDetails?.Count() ?? 0;

                if (customerDetailsCount == 1)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_1"
                    });
                }

                else if (customerDetails?.Count() == paxRefIdCount)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_6"
                    });
                }

                else if (!parentPnrPax.Any(c => c.Ptc == Constants.Application.AdultPTCCode))
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_7"
                    });
                }

                else if (hasChildOrYouth && !hasAdult)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_4",
                    });
                }

                else if (customerDetailsCount == 2 && hasInfant)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_2"
                    });
                }

                else if (!childPnrPax.Any(c => c.Ptc == Constants.Application.AdultPTCCode))
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_8"
                    });
                }
                else
                {
                    errors.AddRange(infants.Where(infant => augmentationPoint.PaxRefID.Contains(infant.CustId) && !augmentationPoint.PaxRefID.Contains(infant.AssociatedWithAdult)).Select(infant => new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_9"
                    }));

                    errors.AddRange(adultsWithInfants.Where(adult => augmentationPoint.PaxRefID.Contains(adult.CustId) && !augmentationPoint.PaxRefID.Contains(adult.LinkedInfantId)).Select(adult => new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "76H_3"
                    }));
                }
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSplitPnrPartialFlownStatus(OrderRS orderDataRS)
        {
            var flownExists = orderDataRS.OrderDocument?.OrderItems?.FirstOrDefault()?.AccountableDocumentRefs?.Any(x => x.Coupons.Any(c => c.CouponStatus == Constants.Application.AllFlownCouponStatus)) == true;
            var errors = new List<ErrorDetail>();

            if (flownExists == true)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "76H_5"
                });
            }
            return flownExists ? [new ErrorDetail { IsDynamicMessage = false, ErrorCode = "76H_5" }] : [];
        }

        public static IEnumerable<ErrorDetail> ValidateSplitPnrContactInfo(Infrastructure.Model.OrderMS.DataList dataList, AugmentationPoint augmentationPoint, IATA_OrderChangeRQ request)
        {
            var errors = new List<ErrorDetail>();
            var adtCustomers = dataList.CustomerDetails.Where(c => c.Ptc == Constants.Application.AdultPTCCode);
            var adultcustomerCount = adtCustomers?.Count();
            var matchingCustomerIds = adtCustomers.Where(cus => augmentationPoint.PaxRefID.Contains(cus.CustId));
            bool anyCustomerHasContact = matchingCustomerIds.Any(c => c.Contacts != null);
            var paxList = request.Request?.DataLists?.PaxList?.Where(x => x.PTC == Constants.Application.AdultPTCCode);

            if (paxList != null)
            {
                foreach (var pax in paxList)
                {
                    var paxRefId = pax.PaxID;
                    var paxNumber = int.Parse(paxRefId?.Split('_').Last());

                    if (paxNumber > adultcustomerCount)
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "911_6"
                        });
                    }
                }
            }

            if (errors.Count == 0 && !anyCustomerHasContact)
            {
                var paxDetails = request.Request?.DataLists?.PaxList?.Select(x => new { x.ContactInfoRefID, x.PaxID });
                var contactInfoList = request?.Request?.DataLists?.ContactInfoList?.Select(x => new { x.ContactInfoID });
                var paxMatchNotFound = paxDetails != null ? paxDetails.Where(c => !augmentationPoint.PaxRefID.Contains(c.PaxID)) : null;

                if (paxDetails == null || contactInfoList == null)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "318_1"
                    });
                }

                if (errors.Count == 0 && paxMatchNotFound.Any())
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_6"
                    });

                }
                else
                {
                    var contactMatchNotFound = paxDetails?.Where(c => !contactInfoList.Any(info => info.ContactInfoID == c.ContactInfoRefID));

                    if (errors.Count == 0 && contactMatchNotFound == null)
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "318_1"
                        });
                    }
                }
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateRequestPaxListWithNameDetails(IATA_OrderChangeRQ request)
        {
            if (request.Request.DataLists.PaxList.Any(x => x.Individual.Surname.IsNullOrEmpty() || (bool)x.Individual.GivenName?.FirstOrDefault().IsNullOrEmpty()))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "912_1" } };

            if (request.Request.DataLists.PaxList.Any(x => x.Individual.GivenName.Length > Convert.ToInt32(Constants.Application.GivenNameMaxLength)))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "150_2" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<NDCWarning> UpdatePriceMismatchAmount(IEnumerable<NDCWarning> warnings, VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS.OrderChangeResponse orderChangeResponse, IEnumerable<SeatDetails> seatDetails)
        {
            if (warnings.Any(x => x.Code == "992_1"))
            {
                var seat = seatDetails.Where(x => orderChangeResponse.OrderDetails.OrderItems.Any(a => a.CurrentSeatAssignment == x.SeatNumber && a.SegmentRefId == x.AssociatedSegment.SegmentId));

                double totalPrice = seat.Sum(x => Convert.ToDouble(x.SeatPrice));
                string currency = seat.First().Currency;

                foreach (var warning in warnings.Where(x => x.Code == "992_1"))
                    warning.DynamicMessagesContents.First().ReplaceWith = totalPrice.ToString() + currency;
            }

            return warnings;
        }

        public static IEnumerable<ErrorDetail> ValidateGenderAndDOBinConfirmHB(IATA_OrderChangeRQ orderChangeRequest, OrderDocument orderDoc)
        {
            var errors = new List<ErrorDetail>();

            foreach (var pax in orderChangeRequest.Request.DataLists.PaxList)
            {
                var paxDetails = orderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == pax.PaxID);

                if (paxDetails == null)
                {
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "143_3" } };
                }
            }

            foreach (var pax in orderDoc.DataList.CustomerDetails)
            {
                var paxDetails = orderChangeRequest.Request.DataLists.PaxList.FirstOrDefault(x => x.PaxID == pax.CustId);

                if (pax.DateOfBirth.IsNullOrEmpty() && paxDetails != null)
                {
                    if ((paxDetails.Individual?.Birthdate == null || paxDetails.Individual?.Birthdate == DateTime.MinValue) && !errors.Any(x => x.ErrorCode == "912_31"))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "912_31"
                        });
                    }
                    else
                    {
                        DateTime? deptDate = Convert.ToDateTime(orderDoc.DataList.OriginDestinationAssociations.First().OriginDestination.DepartureDate);
                        DateTime birthDate = Convert.ToDateTime(paxDetails.Individual.Birthdate);
                        double? deptcount = (deptDate?.Year * 100 + deptDate?.Month) * 100 + deptDate?.Day;
                        double birthcount = (birthDate.Year * 100 + birthDate.Month) * 100 + birthDate.Day;
                        double? age = (deptcount - birthcount) / 10000;

                        if (birthDate > System.DateTime.Today)
                        {
                            errors.Add(new ErrorDetail { IsDynamicMessage = true, ErrorCode = "73_6", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxID}", ReplaceWith = paxDetails.PaxID } } });

                        }
                        else if (paxDetails.PTC == Constants.Application.AdultPTCCode)
                        {
                            if (!(age < 100 && age >= 16))
                                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_1" });
                        }
                        else if (paxDetails.PTC == Constants.Application.InfantPTCCode)
                        {
                            if (!(age < 2 && age > 0))
                                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_5" });
                        }

                        else if (paxDetails.PTC == Constants.Application.ChildPTCCode)
                        {
                            if (!(age < 12 && age >= 2))
                                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_3" });
                        }

                        else if (paxDetails.PTC == Constants.Application.YoungPTCCode)
                        {
                            if (!(age < 16 && age >= 12))
                                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_4" });
                        }

                    }
                }

                if (!errors.Any(x => x.ErrorCode == "912_32") && pax.Gender.IsNullOrEmpty() && paxDetails != null)
                {
                    if (paxDetails.Individual == null || !paxDetails.Individual.GenderCodeSpecified)
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "912_32"
                        });
                    }
                    else
                    {
                        if (!new[] { "M", "F", "U", "X" }.Contains(paxDetails.Individual.GenderCode.ToString()))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = true,
                                ErrorCode = "911_7",
                                DynamicMessagesContents = new List<DynamicMessagesContent>()
                            { new DynamicMessagesContent() {
                            Fieldname ="{PaxId}",
                            ReplaceWith = paxDetails.PaxID} }
                            });
                        }
                    }
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidatePaymentInformationOrderChange(IATA_OrderChangeRQ orderChangeRequest)
        {
            var errors = new List<ErrorDetail>();

            if (orderChangeRequest?.Request?.PaymentFunctions == null || orderChangeRequest?.Request?.PaymentFunctions.Length == 0)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "466_1",
                });
            }
            if (errors.Count > 0 && errors.First().ErrorCode == "466_1")
                return errors;

            bool validpaymentinfoamount = true;
            foreach (var tp in orderChangeRequest.Request.PaymentFunctions)
            {
                if (tp?.PaymentProcessingDetails?.Amount == null)
                    validpaymentinfoamount = false;
            }

            if (validpaymentinfoamount == false)
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_9",
                });

            ErrorDetail error = ValidatePaymentTypeEnumOrderChange(orderChangeRequest);
            if (!string.IsNullOrEmpty(error.ErrorCode))
                errors.Add(error);

            foreach (var tp in orderChangeRequest.Request.PaymentFunctions)
            {
                if (tp?.PaymentProcessingDetails?.Payer == null)
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "911_11",
                    });
            }

            foreach (var tp in orderChangeRequest.Request.PaymentFunctions)
            {
                if (tp?.PaymentProcessingDetails?.PaymentMethod == null || tp?.PaymentProcessingDetails?.Payer == null)
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "466_1",
                    });
            }

            if (errors.Any(er => er.ErrorCode == "466_1"))
                return errors;

            //Validate Payment Card details

            bool breakloop = false;

            foreach (var tp in orderChangeRequest.Request.PaymentFunctions)
            {
                //Skipping validation for UATP cards
                var paymentCardType = (PaymentProcessingPaymentCardType)tp?.PaymentProcessingDetails?.PaymentMethod?.Item;

                if (paymentCardType?.CardTypeText == "TP" && paymentCardType?.CardSecurityCode == null)
                {
                    breakloop = true;
                }

                if (paymentCardType != null)
                {
                    var errormsg = new List<string>();
                    string errorMssgCombined = null;

                    if (paymentCardType?.CardNumber == null)
                        errormsg.Add("CardNumber");
                    if (paymentCardType?.CardTypeText == null)
                        errormsg.Add("CreditCardVendorCode");
                    if (!breakloop)
                    {
                        if (paymentCardType?.CardSecurityCode == null)
                            errormsg.Add("SeriesCode");
                    }
                    if (paymentCardType?.ExpirationDate == null)
                        errormsg.Add("ExpirationDate");
                    if (paymentCardType?.CardholderAddress?.StreetText == null)
                        errormsg.Add("StreetText");
                    if (paymentCardType?.CardholderAddress?.CityName == null)
                        errormsg.Add("CityName");
                    if (paymentCardType?.CardholderAddress?.CountryCode == null)
                        errormsg.Add("CountryName");

                    foreach (var mssg in errormsg)
                    {
                        if (errorMssgCombined == null)
                            errorMssgCombined = mssg;
                        else
                            errorMssgCombined = errorMssgCombined + "," + mssg;
                    }

                    if (errorMssgCombined != null)
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = true,
                            ErrorCode = "911_12",
                            DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent { Fieldname = "{Field}", ReplaceWith = errorMssgCombined } }
                        });
                }

            }

            ErrorDetail error2 = ValidateCardvendorCodeOrderChange(orderChangeRequest);

            if (!string.IsNullOrEmpty(error2.ErrorCode))
                errors.Add(error2);

            return errors;
        }

        private static ErrorDetail ValidatePaymentTypeEnumOrderChange(IATA_OrderChangeRQ orderChangeRequest)
        {
            var error = new ErrorDetail();
            List<string> paymenttypeenum = Constants.Application.PaymentTypeEnum.ToList();

            bool validpaymentinfoMethod = true;
            foreach (var tp in orderChangeRequest.Request.PaymentFunctions)
            {
                if (!paymenttypeenum.Contains(tp?.PaymentMethodCriteria.PaymentTypeCode))
                    validpaymentinfoMethod = false;
            }

            if (validpaymentinfoMethod == false)
            {
                string paymentypeenumstring = null;
                paymenttypeenum.ForEach(pte =>
                {
                    if (paymentypeenumstring == null)
                        paymentypeenumstring = pte;
                    else
                        paymentypeenumstring = paymentypeenumstring + "," + pte;
                });
                error = new ErrorDetail
                {
                    IsDynamicMessage = true,
                    ErrorCode = "911_14",
                    DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent { Fieldname = "{Payment_Type_Enum}", ReplaceWith = paymentypeenumstring } }
                };
            }

            return error;
        }

        public static ErrorDetail ValidateCardvendorCodeOrderChange(IATA_OrderChangeRQ orderChangeRequest)
        {
            var error = new ErrorDetail();
            List<string> vendorTypeEnum = Constants.Application.CCVendor.ToList();
            bool valid = true;

            foreach (var tp in orderChangeRequest.Request.PaymentFunctions)
            {
                if (((PaymentProcessingPaymentCardType)tp?.PaymentProcessingDetails?.PaymentMethod?.Item)?.CardTypeText != null && !vendorTypeEnum.Contains(((PaymentProcessingPaymentCardType)tp?.PaymentProcessingDetails?.PaymentMethod?.Item)?.CardTypeText))
                    valid = false;
            }

            if (valid == false)
            {
                string errorMssgCombined = null;
                vendorTypeEnum.ForEach(vte =>
                {
                    if (errorMssgCombined == null)
                        errorMssgCombined = vte;
                    else
                        errorMssgCombined = errorMssgCombined + "," + vte;
                });
                error = new ErrorDetail
                {
                    IsDynamicMessage = true,
                    ErrorCode = "911_13",
                    DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent { Fieldname = "{CC_VENDOR_ENUM}", ReplaceWith = errorMssgCombined } }
                };
            }

            return error;
        }

        public static IEnumerable<ErrorDetail> Validate3DS2(IATA_OrderChangeRQ orderChangeRequest)
        {
            var errors = new List<ErrorDetail>();

            if (((PaymentProcessingPaymentCardType)orderChangeRequest?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item)?.SecurePaymentVersion2 == null)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_22",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateEnrollStatus(PaymentProcessingPaymentCardType processingPaymentCardType)
        {
            var errors = new List<ErrorDetail>();

            if (string.IsNullOrEmpty(processingPaymentCardType?.SecureProgram?.EnrollmentStatusText))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_8",
                });
            }
            else
            {
                string enrollStatusStr = processingPaymentCardType?.SecureProgram?.EnrollmentStatusText?.Trim();
                enrollStatusStr = string.IsNullOrEmpty(enrollStatusStr) ? null : enrollStatusStr.IndexOf('.') > 0 ? enrollStatusStr.Substring(0, enrollStatusStr.IndexOf('.')) : enrollStatusStr;
                var isValidEnrolledStatus = Enum.GetNames(typeof(CardEnrollStatus)).Contains(enrollStatusStr);

                if (!string.IsNullOrEmpty(enrollStatusStr) && isValidEnrolledStatus == false)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_9",
                    });
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> Validate3DS2Request(PaymentProcessingPaymentCardType processingPaymentCardType, string cardType)
        {
            return Validate_ECI_CustAuth_CAVV_XID(processingPaymentCardType, cardType);
        }

        public static IEnumerable<ErrorDetail> Validate_ECI_CustAuth_CAVV_XID(PaymentProcessingPaymentCardType processingPaymentCardType, string cardType)
        {
            var errors = new List<ErrorDetail>();
            var ECIStr = processingPaymentCardType?.SecurePaymentVersion2?.ElectronicCommerceInd?.Trim();

            if (string.IsNullOrEmpty(ECIStr))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_10",
                });
            }
            else
            {
                var customerAutherror = ValidateCustomerAuthStatus(processingPaymentCardType);

                if (customerAutherror?.Any() == true)
                {
                    errors.AddRange(customerAutherror);
                }
                else
                {
                    var cardError = new List<ErrorDetail>();
                    if ((processingPaymentCardType?.SecurePaymentVersion2?.TrxStatusText?.FirstOrDefault()?.ToUpper() == CustomerAuthStatus.U.ToString()))
                    {
                        cardError.AddRange(ValidateECIforBusinessCards(processingPaymentCardType?.SecurePaymentVersion2?.ElectronicCommerceInd, cardType));
                    }
                    else
                    {
                        cardError.AddRange(ValidateECI(processingPaymentCardType?.SecurePaymentVersion2?.ElectronicCommerceInd, cardType));
                        var cardCAVVerror = ValidateCAVV(processingPaymentCardType?.SecurePaymentVersion2?.AuthenticationValue);
                        if (cardCAVVerror?.Any() == true)
                        {
                            errors.AddRange(cardCAVVerror);
                        }
                        var cardXIDError = ValidateTransactionID_TYPE(processingPaymentCardType?.SecurePaymentVersion2?.DirectoryServerTrxID);
                        if (cardXIDError?.Any() == true)
                        {
                            errors.AddRange(cardXIDError);
                        }
                    }
                    if (cardError?.Any() == true)
                    {
                        errors.AddRange(cardError);
                    }

                }
            }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateCustomerAuthStatus(PaymentProcessingPaymentCardType processingPaymentCardType)
        {
            var errors = new List<ErrorDetail>();

            string customerAuthStatusStr = processingPaymentCardType?.SecurePaymentVersion2?.TrxStatusText?.FirstOrDefault().Trim();

            if (string.IsNullOrEmpty(customerAuthStatusStr))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_12",
                });
            }
            else
            {
                var isValidcustomerAuthStatus = Enum.GetNames(typeof(CustomerAuthStatus)).Contains(customerAuthStatusStr);

                if (isValidcustomerAuthStatus == false)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_13",
                    });
                }
                else
                {
                    var isEligibleCustomerAuthStatus = (customerAuthStatusStr?.ToUpper() == CustomerAuthStatus.Y.ToString() || customerAuthStatusStr?.ToUpper() == CustomerAuthStatus.A.ToString() || customerAuthStatusStr?.ToUpper() == CustomerAuthStatus.U.ToString()) ? true : false;

                    if (isEligibleCustomerAuthStatus == false)
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "912_18",
                        });
                    }

                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateECIforBusinessCards(string cardECI, string cardType)
        {
            var errors = new List<ErrorDetail>();
            var iECI = string.IsNullOrEmpty(cardECI) ? 0 : Convert.ToInt16(cardECI);
            var isvalidECI = cardType.ToUpper().Equals(Constants.Application.MASTERCARDTYPE) ? iECI == Constants.Application.CA_ECISUCCESS_AUTH_U ? true : false : iECI == Constants.Application.OTHERCARDS_ECISUCCESS_AUTH_U ? true : false;

            if (isvalidECI == false || cardECI?.Length != 2)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_11",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateECI(string cardECI, string cardType)
        {
            var errors = new List<ErrorDetail>();

            var iECI = string.IsNullOrEmpty(cardECI) ? 0 : Convert.ToInt16(cardECI);
            var isvalidECI = cardType.ToUpper().Equals(Constants.Application.MASTERCARDTYPE) ? (iECI == Constants.Application.CA_ECISUCCESS ||
                iECI == Constants.Application.CA_ECIATTEMPTED) ? true : false : (iECI == Constants.Application.OTHERCARDS_ECISUCCESS ||
                iECI == Constants.Application.OTHERCARDS_ECIATTEMPTED) ? true : false;

            if (isvalidECI == false || cardECI?.Length != 2)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_11",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateCAVV(string cardCAVV)
        {
            var errors = new List<ErrorDetail>();

            if (string.IsNullOrEmpty(cardCAVV))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_6",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateTransactionID_TYPE(String cardXID)
        {
            var errors = new List<ErrorDetail>();

            if (string.IsNullOrEmpty(cardXID?.Trim()))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_7",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateNon3DS2Request(PaymentProcessingPaymentCardType processingPaymentCardType)
        {
            var errors = new List<ErrorDetail>();

            if ((string.IsNullOrEmpty(processingPaymentCardType?.SecurePaymentVersion2?.ElectronicCommerceInd) == false) || (string.IsNullOrEmpty(processingPaymentCardType?.SecurePaymentVersion2?.AuthenticationValue) == false) || (string.IsNullOrEmpty(processingPaymentCardType?.SecurePaymentVersion2?.TrxStatusText?.FirstOrDefault()) == false))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = true,
                    ErrorCode = "912_15",
                    DynamicMessagesContents = new List<DynamicMessagesContent>()
                            { new DynamicMessagesContent() {
                            Fieldname ="{Enrollmentstatus}",
                            ReplaceWith = processingPaymentCardType?.SecureProgram?.EnrollmentStatusText} }
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderChangeRQFOP(IATA_OrderChangeRQ request, bool isCreditCardAccepted)
        {
            var errors = new List<ErrorDetail>();
            var paymentMethodCriteria = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentMethodCriteria;

            if (paymentMethodCriteria != null && (paymentMethodCriteria?.PaymentTypeCode != "CC" || isCreditCardAccepted == false))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "466_2",
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOfflinePayment(OfflinePaymentType offlinePaymentType)
        {
            var errors = new List<ErrorDetail>();

            if (offlinePaymentType != null)
            {
                string errorMssg = null;

                if (offlinePaymentType.PaymentBrandCode == null)
                    errorMssg = "PaymentBrandCode in the request";

                if (errorMssg != null)
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = true,
                        ErrorCode = "911_12",
                        DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent { Fieldname = "{Field}", ReplaceWith = errorMssg } }
                    });
            }

            if (offlinePaymentType.PaymentTypeCode != null && offlinePaymentType.PaymentTypeCode != Constants.Application.OfflinePaymentTypeCode)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_36" });

            if (offlinePaymentType.PaymentBrandCode != null && offlinePaymentType.PaymentBrandCode != Constants.Application.OfflineBrandTypeCode)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_35" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateChangeReshopHoldBookingAmount(IATA_OrderChangeRQ request, OfferResponse offerDoc)
        {
            var errors = new List<ErrorDetail>();
            decimal totalPrice = 0;

            var requestAmount = request.Request.PaymentFunctions? .FirstOrDefault()?.PaymentProcessingDetails?.Amount;
            var baseCurrency = offerDoc.Offers.SelectMany(o => o.OfferItems).SelectMany(oi => oi.OfferitemPricing).SelectMany(p => p.SalePrice).FirstOrDefault(sp => sp.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency;

            foreach (var offerItem in offerDoc.Offers.First().OfferItems)
            {
                var basePrice = offerItem.OfferitemPricing.SelectMany(p => p.SalePrice).FirstOrDefault(sp => sp.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum);

                if (basePrice != null && decimal.TryParse(basePrice.Amount?.MonetaryAmount, out var price))
                {
                    int paxCount = offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries?.Count() ?? 0;
                    totalPrice += price * paxCount;
                }
            }

            if (requestAmount == null ||  totalPrice != requestAmount.Value || baseCurrency != requestAmount.CurCode)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_46"
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateFOP(IATA_OrderChangeRQ orderChangeRq, OrderRS orderDoc)
        {
            var errors = new List<ErrorDetail>();

            var paymentRq = orderChangeRq.Request.PaymentFunctions?.First().PaymentProcessingDetails;
            if(paymentRq.Amount.Value > 0)
            {
                bool isCashPayment = orderDoc?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
                bool isEasyPayment = orderDoc?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

                if (isCashPayment)
                {
                    var cashPaymentMethod = paymentRq?.PaymentMethod?.Item as VAA.EAPI.NDC.Order.Domain.OrderChangeRQ.SettlementPlanType;
                    if (cashPaymentMethod == null)
                    {
                        errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "759_1" });
                    }
                }

                if (isEasyPayment)
                {
                    var EasyPaymentMethod = paymentRq?.PaymentMethod?.Item as PaymentProcessingIATA_EasyPayType;
                    if (EasyPaymentMethod == null)
                    {
                        errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "759_1" });
                    }
                }

            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSeatFOP(IATA_OrderChangeRQ orderChangeRq)
        {
            var errors = new List<ErrorDetail>();

            var paymentRq = orderChangeRq?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails;
            if (paymentRq?.Amount != null && paymentRq.Amount.Value > 0)
            {
                if(orderChangeRq.Request.PaymentFunctions?.FirstOrDefault()?.PaymentMethodCriteria?.PaymentTypeCode != "CC")
                {
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "466_4" });
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidatePrivateFareFOP(IATA_OrderChangeRQ orderChangeRequest, OrderRS orderDoc)
        {
            var errors = new List<ErrorDetail>();

            bool isCardTypeCode = orderChangeRequest.Request.PaymentFunctions?.FirstOrDefault()?.PaymentMethodCriteria?.PaymentTypeCode == "CC";
            bool isCardNumber = orderChangeRequest.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is EAPI.Domain.Virgin.Models.Payment.CardDetails { CardNumber: not null };

            if (orderDoc?.OrderDocument?.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode && (isCardTypeCode || isCardNumber))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "466_3" });
            }

            return errors;
        }
        public static IEnumerable<ErrorDetail> ValidateSecurePaymentVersion2Data(PaymentFunctionsChangeType[]? paymentFunctions)
        {
            var errors = new List<ErrorDetail>();

            if (paymentFunctions?.Any(x =>
                x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.DirectoryServerTrxID)) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_7" });
            }

            if (paymentFunctions?.Any(x =>
                x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                string.IsNullOrWhiteSpace(card.SecureProgram?.EnrollmentStatusText)) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_8" });
            }

            if (paymentFunctions?.Any(x =>
                 x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                 !string.IsNullOrWhiteSpace(card.SecureProgram?.EnrollmentStatusText) &&
                 !string.Equals(card.SecureProgram.EnrollmentStatusText, "CardEnrolled3DS2.1.0", StringComparison.OrdinalIgnoreCase)) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_9" });
            }

            if (paymentFunctions?.Any(x =>
                x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.ElectronicCommerceInd)) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_10" });
            }

            if (paymentFunctions?.Any(x =>
                x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                !string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.ElectronicCommerceInd) &&
                card.SecurePaymentVersion2.ElectronicCommerceInd is not ("05" or "02")) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_11" });
            }

            if (paymentFunctions?.Any(x =>
                x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                (card.SecurePaymentVersion2?.TrxStatusText == null || !card.SecurePaymentVersion2.TrxStatusText.Any() || card.SecurePaymentVersion2.TrxStatusText.All(string.IsNullOrWhiteSpace))) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_12" });
            }

            if (paymentFunctions?.Any(x =>
                x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                card.SecurePaymentVersion2?.TrxStatusText?.Any(s => !string.Equals(s, "Y", StringComparison.OrdinalIgnoreCase)) == true) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_13" });
            }

            if (paymentFunctions?.Any(x =>
                x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card &&
                string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.AuthenticationValue)) == true)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_44" });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidatePaymentFunctions(IATA_OrderChangeRQ orderChangeRequest, OfferResponse offerDoc)
        {
            var errors = new List<ErrorDetail>();

            var paymentFunctionsMissing = orderChangeRequest?.Request?.PaymentFunctions == null || !orderChangeRequest.Request.PaymentFunctions.Any();

            var hasAddCollectPrice = offerDoc?.Offers?.SelectMany(x => x.OfferItems).Any(x => x.OfferitemPricing.Any(p => p?.AddCollectPrice != null)) == true;

            var hasRefundPrice = offerDoc?.Offers?.SelectMany(x => x.OfferItems).Any(x => x.OfferitemPricing.Any(p => p?.RefundPrice != null)) == true;
            if (paymentFunctionsMissing)
            {
                if (hasAddCollectPrice)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_45"
                    });
                }

                else if (!hasRefundPrice) //Even Exchange Scenario
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_45"
                    });
                }
            }
            return errors;
        }
    }
}
