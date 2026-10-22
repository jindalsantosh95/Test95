using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Validations.OrderCreate
{
    public class OrderCreateRequestValidation
    {
        public static IEnumerable<ErrorDetail> ValidateDistributionChain(IATA_OrderCreateRQ orderCreateRQ)
        {
            string totalDC = orderCreateRQ.DistributionChain.Length.ToString();
            var errors = new List<ErrorDetail>();

            if (!orderCreateRQ.DistributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Carrier))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "914_17",
                    IsDynamicMessage = false
                });
            }

            if (errors.Count == 0)
            {
                foreach (var dc in orderCreateRQ.DistributionChain)
                {
                    if (dc.ParticipatingOrg is null || string.IsNullOrEmpty(dc.ParticipatingOrg.OrgID))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "912_21"
                        });
                    }
                    else if (dc.OrgRole == DistributionChainRoleCodeType.Carrier && dc.Ordinal != totalDC)
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

                }

            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOfferID(IATA_OrderCreateRQ request)
        {
            if ((request.Request.CreateOrder?.Item as AcceptSelectedQuotedOfferListType)?.SelectedPricedOffer?.FirstOrDefault()?.OfferRefID.IsNullOrEmpty() ?? false)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_26" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateNonFlightServices(IATA_OrderCreateRQ request)
        {
            var errors = new List<ErrorDetail>();
            var offers = (AcceptSelectedQuotedOfferListType)request.Request.CreateOrder?.Item;

            if (offers.SelectedPricedOffer.Any(x => x.SelectedOfferItem == null || x.SelectedOfferItem.Any(y => y.OfferItemRefID == null)))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_27"
                });
            }
            else if (offers.SelectedPricedOffer.Any(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null || y.SelectedALaCarteOfferItem != null)))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "495_1"
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidatePax(IATA_OrderCreateRQ request, bool holdBooking, bool isEasyPay)
        {
            var errors = new List<ErrorDetail>();
            var offerItems = (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedSeat == null)).SelectedOfferItem;
            bool iscashFOP = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod.Item is SettlementPlanType;
            
            foreach (var offerItem in offerItems)
            {
                foreach (var pax in offerItem.PaxRefID)
                {
                    var paxDetails = request.Request.DataLists.PaxList.FirstOrDefault(x => x.PaxID == pax);

                    if (paxDetails == null || string.IsNullOrEmpty(paxDetails.Individual.GivenName.FirstOrDefault()))
                        errors.Add(new() { IsDynamicMessage = false, ErrorCode = "152_2" });

                    else if (!RegexCompiled.Match(paxDetails.Individual.GivenName.FirstOrDefault(), "^[a-zA-Z ]*$").Success)
                        errors.Add(new() { IsDynamicMessage = true, ErrorCode = "314_2", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = paxDetails.PaxID } } });

                    if (paxDetails.Individual.GivenName.Length > Convert.ToInt32(Constants.Application.GivenNameMaxLength))
                        errors.Add(new() { IsDynamicMessage = false, ErrorCode = "150_2" });
                    
                    if (paxDetails == null || string.IsNullOrEmpty(paxDetails.Individual.Surname) || paxDetails.Individual.Surname.Length < 2)
                        errors.Add(new() { IsDynamicMessage = false, ErrorCode = "151_1" });

                    else if (!RegexCompiled.Match(paxDetails.Individual.Surname, "^[a-zA-Z ]*$").Success)
                        errors.Add(new() { IsDynamicMessage = true, ErrorCode = "314_3", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = paxDetails.PaxID } } });

                    else if (paxDetails.Individual.Surname.Length > int.Parse(Constants.Application.SurnameMaxLength))
                        errors.Add(new() { IsDynamicMessage = false, ErrorCode = "149_1" });

                    if (!holdBooking)
                    {
                        if (paxDetails == null || !paxDetails.Individual.GenderCodeSpecified || Convert.ToString(paxDetails.Individual.GenderCode).IsNullOrEmpty() ||
                            !Constants.Application.ValidGender.Split(',').Any(x => string.Equals(x, Convert.ToString(paxDetails.Individual.GenderCode), StringComparison.CurrentCultureIgnoreCase)))
                            errors.Add(new() { IsDynamicMessage = true, ErrorCode = "911_7", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = paxDetails.PaxID } } });

                        if (!iscashFOP && !isEasyPay)
                        {
                            foreach (var payment in request.Request?.PaymentFunctions)
                            {
                                if (payment.PaymentProcessingDetails == null || payment.PaymentProcessingDetails?.PaymentMethod == null || payment.PaymentProcessingDetails.Payer == null || payment.PaymentProcessingDetails.Payer.PayerName.Item == null ||
                                    (payment.PaymentProcessingDetails.Payer.PayerName.Item as PayerIndividualType).GivenName.FirstOrDefault().IsNullOrEmpty())
                                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "152_2" });

                                else if (!RegexCompiled.Match((payment.PaymentProcessingDetails.Payer.PayerName.Item as PayerIndividualType).GivenName.FirstOrDefault(), "^[a-zA-Z ]*$").Success)
                                    errors.Add(new() { IsDynamicMessage = true, ErrorCode = "314_2", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = "Payer FirstName" } } });

                                if (payment.PaymentProcessingDetails == null || payment.PaymentProcessingDetails?.PaymentMethod == null || payment.PaymentProcessingDetails.Payer == null || payment.PaymentProcessingDetails.Payer.PayerName.Item == null ||
                                    (payment.PaymentProcessingDetails.Payer.PayerName.Item as PayerIndividualType).Surname.IsNullOrEmpty())
                                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "151_1" });

                                else if (!RegexCompiled.Match((payment.PaymentProcessingDetails.Payer.PayerName.Item as PayerIndividualType).Surname, "^[a-zA-Z ]*$").Success)
                                    errors.Add(new() { IsDynamicMessage = true, ErrorCode = "314_3", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = "Payer LastName" } } });
                            }
                        }

                    }
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateAmountForHoldBooking(IATA_OrderCreateRQ request, OfferResponse offerDoc)
        {
            var totalPrice = (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedSeat == null)).TotalOfferPriceAmount;
            var offerDocPrice = offerDoc.Offers.FirstOrDefault().SalePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).FirstOrDefault().Amount;

            if (totalPrice == null || totalPrice.CurCode != offerDocPrice.BaseCurrency || totalPrice.Value != Convert.ToDecimal(offerDocPrice.MonetaryAmount))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_3" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateAmount(IATA_OrderCreateRQ request, OfferResponse offerDoc)
        {
            if (request.Request.PaymentFunctions.FirstOrDefault()?.PaymentProcessingDetails != null)
            {
                if (request.Request.PaymentFunctions.Length == 1)
                {
                    var orderRequestPrice = ((AcceptSelectedQuotedOfferListType)request?.Request?.CreateOrder?.Item).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedSeat == null)).TotalOfferPriceAmount;
                    var offerDBPrice = offerDoc.Offers.FirstOrDefault()?.SalePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).FirstOrDefault()?.Amount;
                    var orderRequesPaymentPrice = request.Request.PaymentFunctions.FirstOrDefault()?.PaymentProcessingDetails;

                    var totalAmount = ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.Length > 1 && ((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedSeat != null))?.TotalOfferPriceAmount != null ?
                    Convert.ToDecimal(orderRequestPrice.Value) + Convert.ToDecimal(((AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedSeat != null)).TotalOfferPriceAmount.Value) : Convert.ToDecimal(orderRequestPrice?.Value);

                    if (orderRequestPrice == null || orderRequestPrice.CurCode != offerDBPrice.BaseCurrency || orderRequestPrice.Value != Convert.ToDecimal(offerDBPrice.MonetaryAmount) || orderRequesPaymentPrice?.Amount.CurCode != offerDBPrice.BaseCurrency || totalAmount != Convert.ToDecimal(orderRequesPaymentPrice?.Amount.Value))
                        return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "911_3" } };
                }
                else
                {
                    var orderRequest = ((AcceptSelectedQuotedOfferListType)request?.Request?.CreateOrder?.Item).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedSeat == null));
                    var orderRequestPrice = orderRequest.TotalOfferPriceAmount;
                    var seatRequest = ((AcceptSelectedQuotedOfferListType)request?.Request?.CreateOrder?.Item).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null));
                    var seatRequestPrice = ((AcceptSelectedQuotedOfferListType)request?.Request?.CreateOrder?.Item).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null)).TotalOfferPriceAmount;
                    var offerDBPrice = offerDoc.Offers.FirstOrDefault().SalePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).FirstOrDefault()?.Amount;
                    var orderRequesPaymentPrice = request.Request.PaymentFunctions?.FirstOrDefault(x => x.OfferAssociation?.FirstOrDefault()?.OfferRefID == orderRequest.OfferRefID)?.PaymentProcessingDetails;
                    var totalPrice = request.Request.PaymentFunctions.Sum(p => p.PaymentProcessingDetails.Amount.Value);

                    var totalAmount = (seatRequestPrice?.Value ?? 0M) + (orderRequestPrice?.Value ?? 0M);

                    if (orderRequestPrice == null || orderRequestPrice.CurCode != offerDBPrice.BaseCurrency || orderRequestPrice.Value != Convert.ToDecimal(offerDBPrice.MonetaryAmount) || orderRequesPaymentPrice?.Amount.CurCode != offerDBPrice.BaseCurrency || totalAmount != totalPrice)
                        return [new() { IsDynamicMessage = false, ErrorCode = "911_3" }];
                }
            }

            return Enumerable.Empty<ErrorDetail>();

        }

        public static IEnumerable<ErrorDetail> ValidateContactDetails(IATA_OrderCreateRQ orderCreateRQ)
        {
            var contactInfoList = orderCreateRQ?.Request?.DataLists?.ContactInfoList;
            if (contactInfoList.IsNullOrEmpty())
            {
                return [new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_5"
                }];
            }

            var firstContactInfo = contactInfoList.FirstOrDefault();
            var primaryContactInfo = IsContactRefusedWithoutContactDetails(firstContactInfo)? GetPrimaryContactInfo(orderCreateRQ) : firstContactInfo;
            var phoneNumber = primaryContactInfo?.Phone?.FirstOrDefault()?.PhoneNumber;        

            if (phoneNumber.IsNullOrEmpty())
            {
                return [new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "316_2"
                }];
            }

            if (phoneNumber.Length < 6)
            {
                return [new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "317_2"
                }];
            }         


            return [];
        }
        
        public static IEnumerable<ErrorDetail> ValidatePayerAddress(IATA_OrderCreateRQ request)
        {
            var cardPayment = request?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as PaymentProcessingPaymentCardType;

            var address = cardPayment?.CardholderAddress;
            if (address == null)
            {
                return Enumerable.Empty<ErrorDetail>();
            }

            if (Constants.Environment.EligibleCountryBillingState.Contains(address.CountryCode))
            {
                var missingMandatoryParams = new List<string>();

                if (address.CountrySubDivisionName.IsNullOrEmpty()) missingMandatoryParams.Add("State");
                if (address.PostalCode.IsNullOrEmpty()) missingMandatoryParams.Add("PostalCode");
                if (address.CityName.IsNullOrEmpty()) missingMandatoryParams.Add("CityName");
                if (address.StreetText.IsNullOrEmpty()) missingMandatoryParams.Add("StreetText");

                if (missingMandatoryParams.Any())
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = true, ErrorCode = "316_3", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{missingManadatoryParameters}", ReplaceWith = string.Join(",", missingMandatoryParams) } } } };
            }
            else
            {
                var missingMandatoryParams = new List<string>();

                if (address.CountryCode.IsNullOrEmpty()) missingMandatoryParams.Add("Country");
                if (address.CityName.IsNullOrEmpty()) missingMandatoryParams.Add("CityName");
                if (address.StreetText.IsNullOrEmpty()) missingMandatoryParams.Add("StreetText");

                if (missingMandatoryParams.Any())
                    return new List<ErrorDetail>() { new() { IsDynamicMessage = true, ErrorCode = "316_3", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{missingManadatoryParameters}", ReplaceWith = string.Join(",", missingMandatoryParams) } } } };
            }

            return Enumerable.Empty<ErrorDetail>();

        }

        public static IEnumerable<ErrorDetail> ValidateDuplicatePaxID(IATA_OrderCreateRQ request)
        {
            var errors = new List<ErrorDetail>();
            var offers = (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer;
            foreach (var duplicatePax in from offer in offers
                                         from offerItem in offer.SelectedOfferItem
                                         let duplicatePax = offerItem.PaxRefID.GroupBy(x => x).Where(y => y.Count() > 1).Select(y => y.Key)
                                         where duplicatePax.Any()
                                         select duplicatePax)
            {
                errors.AddRange(duplicatePax.Select(item => new ErrorDetail { IsDynamicMessage = true, ErrorCode = "314_2", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "{PaxId}", ReplaceWith = item } } }));
            }
            if (errors.Any())
                return errors;

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateAgentForHoldBooking(IATA_OrderCreateRQ request, OfferResponse offerDoc, ValidationRS validationRS)
        {
            var errors = new List<ErrorDetail>();
            var email = request.DistributionChain.Length > 1 ? request.DistributionChain.OrderBy(x => x.Ordinal).FirstOrDefault()?.ContactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText : request.DistributionChain.FirstOrDefault()?.ContactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText;
            var sender = request.DistributionChain.Any(x => string.IsNullOrEmpty(x.Ordinal) || !int.TryParse(x.Ordinal, out var ordinal)) ? null : request.DistributionChain.OrderByDescending(x => x.Ordinal).LastOrDefault();
            var contactInfo = request.DistributionChain.Length > 1 ? request.DistributionChain.OrderBy(x => x.Ordinal).FirstOrDefault()?.ContactInfo?.ContactInfoID : request.DistributionChain.FirstOrDefault()?.ContactInfo?.ContactInfoID;

            if ((validationRS.ValidateResponse.AgencyDetails.RequestAgencyIdType == Constants.Application.AgentTypeIATA || validationRS.ValidateResponse.AgencyDetails.RequestAgencyIdType == Constants.Application.IndirectAgent) && string.IsNullOrEmpty(email))
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "368_7" });

            else if (!string.IsNullOrEmpty(email) && (email.Length > Convert.ToInt32(Constants.Application.EmailMaxLength) || !RegexCompiled.Match(email, Constants.Application.RegexToValidateEmail).Success))
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "317_1" });

            if (contactInfo.IsNullOrEmpty())
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_34" });

            else if (request.DistributionChain.Length > 1 && offerDoc.Parties.Participants != null)
            {
                var participant = request.DistributionChain.OrderBy(x => x.Ordinal).FirstOrDefault();

                if (!offerDoc.Parties.Participants.Any(x => Convert.ToString(x.PartyType) == Convert.ToString(participant.OrgRole) && x.AgencyID == participant.ParticipatingOrg?.OrgID))
                {
                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "368_1" });
                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateAgent(IATA_OrderCreateRQ request, ValidationRS validationRS)
        {
            var distributionChain = (request.DistributionChain.Where(x => x.OrgRole != DistributionChainRoleCodeType.Carrier).OrderBy(x => int.Parse(x.Ordinal)));

            if (validationRS.ValidateResponse.KeyType == "DIRECT" && (distributionChain.Count() != 2 && distributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Distributor) || distributionChain.FirstOrDefault()?.ParticipatingOrg?.OrgID != validationRS.ValidateResponse.AgencyDetails.RequestAgencyId))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "368_2" } };

            if (validationRS.ValidateResponse.KeyType != "DIRECT" && (distributionChain.Count() != 2 || !distributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Seller && x.ParticipatingOrg?.OrgID == validationRS.ValidateResponse.AgencyDetails.RequestAgencyId
            || !distributionChain.Any(x => x.OrgRole != DistributionChainRoleCodeType.Seller && x.ParticipatingOrg?.OrgID == validationRS.ValidateResponse.AggregatorDetails.AgencyId))))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "368_2" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOffer(IATA_OrderCreateRQ request, OfferResponse offerDoc)
        {
            var errors = new List<ErrorDetail>();
            var acceptSelectedQuotedOfferListType = (AcceptSelectedQuotedOfferListType)request.Request.CreateOrder.Item;
            bool isCardTypeCode = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentMethodCriteria?.PaymentTypeCode == "CC";
            bool isCardNumber = request.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is CardDetails { CardNumber: not null };

            if (offerDoc == null || offerDoc.DataList == null)
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_2" });
                return errors;
            }

            if (offerDoc?.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode && (isCardTypeCode || isCardNumber))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "466_3" });
                return errors;
            }

            if (acceptSelectedQuotedOfferListType.SelectedPricedOffer.Any(x => x.OwnerCode.IsNullOrEmpty()))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_26" });

            else if (acceptSelectedQuotedOfferListType.SelectedPricedOffer.Any(x => x.OwnerCode.Length != 2))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_40" });

            var offerItems = (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedSeat == null)).SelectedOfferItem.Select(x => x.OfferItemRefID);
            var offerItemsOfferDoc = offerDoc.Offers.FirstOrDefault().OfferItems.Select(oi => oi.OfferItemId);

            if (offerItems.Intersect(offerItemsOfferDoc).Count() != offerItems.Count())
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_10" });

            var paxListRequest = new List<string>();
            var ptcListRequest = new List<string>();
            foreach (var pax in (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedSeat == null)).SelectedOfferItem.SelectMany(offerItem => offerItem.PaxRefID))
            {
                paxListRequest.Add(pax);
                if (request.Request.DataLists.PaxList?.Any(x => x.PaxID == pax && x.PTC is not null) == true)
                    ptcListRequest.Add(request.Request.DataLists.PaxList.First(x => x.PaxID == pax).PTC);
            }

            var paxListOfferDoc = offerDoc.DataList.CustomerDetails.Select(x => x.CustId);
            var PTCOfferDoc = offerDoc.DataList.CustomerDetails.DistinctBy(x => x.Ptc).Select(x => x.Ptc.Replace(Constants.Application.CNN, Constants.Application.CHD));

            if (paxListRequest.Any(x => x.IsNullOrEmpty()))
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_28" });

            else if (paxListRequest.Count != paxListOfferDoc.Count() || paxListOfferDoc.Intersect(paxListRequest).Count() != paxListOfferDoc.Count() || request.Request.DataLists.PaxList?.Select(x => x.PaxID).Count() != paxListOfferDoc.Count() || PTCOfferDoc.Intersect(ptcListRequest).Count() != PTCOfferDoc.Count() || ptcListRequest.Except(PTCOfferDoc).Any())
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_6" });

            if (request.Request.DataLists.PaxList?.Any(x => x.PaxID.IsNullOrEmpty()) ?? false || request.Request.DataLists.PaxList?.Select(x => x.PaxID).Intersect(paxListOfferDoc).Count() < paxListOfferDoc.Count())
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_23" });

            if (offerDoc.Offers.FirstOrDefault().OfferId != (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedSeat == null)).OfferRefID)
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_2" });

            return errors;

        }

        public static IEnumerable<NDCWarning> ValidateGSTNumberandOragName(IATA_OrderCreateRQ request)
        {
            var warnings = new List<NDCWarning>();
            foreach (var y in request.Request.DataLists.PaxList)
            {
                if (y.Employer != null)
                {
                    if (y.Employer.Name.IsNullOrEmpty() || !RegexCompiled.Match(y.Employer.Name, Constants.Application.RegexToValidateAlphaNumeric).Success
                            || !(y.Employer.Name.Length <= 35))

                        warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "914_11", Type = "Warning" });

                    var salesTaxRegistration = y.Employer.SalesTaxRegistration.FirstOrDefault();

                    if (salesTaxRegistration == null || salesTaxRegistration.SalesTaxRegistrationID.IsNullOrEmpty() || !RegexCompiled.Match(salesTaxRegistration.SalesTaxRegistrationID, Constants.Application.RegexToValidateAlphaNumeric).Success
                            || !(salesTaxRegistration.SalesTaxRegistrationID?.Length <= 20))

                        warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "914_10", Type = "Warning" });
                }
            }

            return warnings;
        }

        public static IEnumerable<NDCWarning> ValidateGSTPhoneEmailAddress(IATA_OrderCreateRQ request)
        {
            var warnings = new List<NDCWarning>();

            if (request.Request.DataLists.PaxList.Any(x => x.Employer != null && ((x.Employer.SalesTaxRegistration.FirstOrDefault() == null || x.Employer.SalesTaxRegistration.FirstOrDefault().CityName.IsNullOrEmpty() || !RegexCompiled.Match(x.Employer.SalesTaxRegistration.FirstOrDefault().CityName, Constants.Application.RegexToValidateAlphaNumeric).Success || x.Employer.SalesTaxRegistration.FirstOrDefault().CityName.Length > 25 ||
                    x.Employer.SalesTaxRegistration.FirstOrDefault().CountryCode.IsNullOrEmpty() || !RegexCompiled.Match(x.Employer.SalesTaxRegistration.FirstOrDefault().CountryCode, Constants.Application.RegexToValidateAlphaNumeric).Success || x.Employer.SalesTaxRegistration.FirstOrDefault().CountryCode.Length > 3 ||
                    x.Employer.SalesTaxRegistration.FirstOrDefault().PostalCode.IsNullOrEmpty() || !RegexCompiled.Match(x.Employer.SalesTaxRegistration.FirstOrDefault().PostalCode, Constants.Application.RegexToValidateAlphaNumeric).Success || x.Employer.SalesTaxRegistration.FirstOrDefault().PostalCode.Length > 17 ||
                    x.Employer.SalesTaxRegistration.FirstOrDefault().StreetText.Any(x => x.IsNullOrEmpty()) || x.Employer.SalesTaxRegistration.FirstOrDefault().StreetText.Any(x => !RegexCompiled.Match(x, Constants.Application.RegexToValidateAlphaNumeric).Success) ||
                    x.Employer.SalesTaxRegistration.FirstOrDefault().StreetText.First().Length > 35 || x.Employer.SalesTaxRegistration.FirstOrDefault().StreetText.Length == 2) && x.Employer.SalesTaxRegistration.FirstOrDefault().StreetText.Last().Length > 25)))
                warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "914_13", Type = "Warning" });

            if (request.Request.DataLists.PaxList.Any(x => x.Employer != null && (x.Employer.SalesTaxRegistration.FirstOrDefault() == null || x.Employer.SalesTaxRegistration.FirstOrDefault().EmailAddressText.IsNullOrEmpty() || x.Employer.SalesTaxRegistration.FirstOrDefault().EmailAddressText.Length > 35)))
                warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "914_14", Type = "Warning" });

            if (request.Request.DataLists.PaxList.Any(x => x.Employer != null && (x.Employer.SalesTaxRegistration.FirstOrDefault() == null || x.Employer.SalesTaxRegistration.FirstOrDefault().CountryDialingCode.IsNullOrEmpty() ||
            x.Employer.SalesTaxRegistration.FirstOrDefault().PhoneNumber.Any(y => y.IsNullOrEmpty() || $"{x.Employer.SalesTaxRegistration.FirstOrDefault().CountryDialingCode}{y}".Length > 15 || !RegexCompiled.Match($"{x.Employer.SalesTaxRegistration.FirstOrDefault().CountryDialingCode}{y}", Constants.Application.RegexToValidateNumeric).Success))))
                warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "914_15", Type = "Warning" });

            return warnings;
        }

        public static bool ValidateGST(ValidationRS validationRS, OfferResponse offer, IEnumerable<AirportDetail> airportDetails)
        {
            if (!validationRS.ValidateResponse.AgencyDetails.CountryCode.Is(Constants.Application.IndiaCountryCode)) return true;

            return !offer.DataList.OriginDestinationAssociations.Any(x => airportDetails.Any(a => a.AirportCode.Is(x.OriginDestination.OriginAirport) && a.CountryCode.Is(Constants.Application.IndiaCountryCode)));
        }

        public static IEnumerable<NDCWarning> ValidateAPISDetails(IATA_OrderCreateRQ request)
        {
            var warnings = new List<NDCWarning>();
            List<string> validDocumentTypes = new List<string>() { Constants.Application.PT, Constants.Application.GC, Constants.Application.CR, Constants.Application.VI, Constants.Application.AEA };

            foreach (var y in request.Request.DataLists.PaxList)
            {
                if (y.RedressCase != null)
                {
                    warnings.AddRange(from doc in y.RedressCase
                                      where !RegexCompiled.Match(doc.RedressCaseID, Constants.Application.RegexToValidateAlphaNumeric).Success
                                        || doc.CountryCode?.Length > 3 || (doc.CountryCode != null && !RegexCompiled.Match(doc.CountryCode, Constants.Application.RegexToValidateAlphabets).Success)
                                      select new NDCWarning { IsDynamicMessage = true, DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<<PAX n>>", ReplaceWith = y.PaxID } }, Code = "914_16", Type = "Warning" });

                }

                if (y.IdentityDoc != null && !warnings.Any(x => x.DynamicMessagesContents?.FirstOrDefault()?.ReplaceWith == y.PaxID))
                {
                    if (y.IdentityDoc.Any(td => td.IdentityDocTypeCode == Constants.Application.GC) && y.IdentityDoc.Any(td => td.IdentityDocTypeCode == Constants.Application.PT && td.Visa != null))
                    {
                        y.IdentityDoc = y.IdentityDoc.Select(x =>
                        {
                            x.Visa = null;
                            return x;
                        }).ToArray();
                    }

                    if (y.IdentityDoc.Where(td => td.IdentityDocTypeCode == Constants.Application.PT).Count() > 1 ||
                        y.IdentityDoc.Where(td => td.IdentityDocTypeCode == Constants.Application.GC).Count() > 1 ||
                        y.IdentityDoc.Where(td => td.IdentityDocTypeCode == Constants.Application.CR).Count() > 1)
                        warnings.Add(new NDCWarning { IsDynamicMessage = true, DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<<PAX n>>", ReplaceWith = y.PaxID } }, Code = "914_16", Type = "Warning" });


                    else
                    {
                        warnings.AddRange(from doc in y.IdentityDoc
                                          where (doc.Visa == null && doc.GivenName != null && (!string.Equals(doc.GivenName.FirstOrDefault(), y.Individual.GivenName.FirstOrDefault(), StringComparison.CurrentCultureIgnoreCase) || !string.Equals(doc.Surname, y.Individual.Surname, StringComparison.CurrentCultureIgnoreCase)) || (doc.GenderCode != null && doc.GenderCodeSpecified && doc.GenderCode != y.Individual.GenderCode) || (doc.Birthdate != null && doc.BirthdateSpecified && doc.Birthdate != y.Individual.Birthdate))
                                            || !RegexCompiled.Match(doc.IdentityDocID, Constants.Application.RegexToValidateAlphaNumeric).Success || !validDocumentTypes.Contains(doc.IdentityDocTypeCode)
                                            || (doc.IssueDateSpecified && Convert.ToDateTime(doc.IssueDate).CompareTo(DateTime.Today) > 0) || (doc.ExpiryDateSpecified && Convert.ToDateTime(doc.ExpiryDate).CompareTo(DateTime.Today) < 0)
                                            || doc.ResidenceCountryCode?.Length > 3 || doc.IssuingCountryCode?.Length > 3 ||
                                            (doc.ResidenceCountryCode != null && !RegexCompiled.Match(doc.ResidenceCountryCode, Constants.Application.RegexToValidateAlphabets).Success) ||
                                            (doc.IssuingCountryCode != null && !RegexCompiled.Match(doc.IssuingCountryCode, Constants.Application.RegexToValidateAlphabets).Success)
                                          select new NDCWarning { IsDynamicMessage = true, DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<<PAX n>>", ReplaceWith = y.PaxID } }, Code = "914_16", Type = "Warning" });

                        if (!warnings.Any(x => x.DynamicMessagesContents?.FirstOrDefault()?.ReplaceWith == y.PaxID))
                        {
                            foreach (var doc in y.IdentityDoc.Where(doc => doc.Visa != null))
                            {

                                warnings.AddRange(from v in doc.Visa
                                                  where (doc.GivenName != null && (!string.Equals(doc.GivenName.FirstOrDefault(), y.Individual.GivenName.FirstOrDefault(), StringComparison.CurrentCultureIgnoreCase) || !string.Equals(doc.Surname, y.Individual.Surname, StringComparison.CurrentCultureIgnoreCase)))
                                                    || !RegexCompiled.Match(v.VisaID, Constants.Application.RegexToValidateAlphaNumeric).Success
                                                    || (v.IssueDateSpecified && Convert.ToDateTime(v.IssueDate).CompareTo(DateTime.Today) > 0) || (v.ExpiryDateSpecified && Convert.ToDateTime(v.ExpiryDate).CompareTo(DateTime.Today) < 0)
                                                    || doc.ResidenceCountryCode?.Length > 3 || v.IssuingCountryCode?.Length > 3 || v.HostCountryCode?.Length > 3 ||
                                                    (doc.ResidenceCountryCode != null && !RegexCompiled.Match(doc.ResidenceCountryCode, Constants.Application.RegexToValidateAlphabets).Success) ||
                                                    (v.HostCountryCode != null && !RegexCompiled.Match(v.HostCountryCode, Constants.Application.RegexToValidateAlphabets).Success) ||
                                                    (v.IssuingCountryCode != null && !RegexCompiled.Match(v.IssuingCountryCode, Constants.Application.RegexToValidateAlphabets).Success)
                                                  select new NDCWarning { IsDynamicMessage = true, DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<<PAX n>>", ReplaceWith = y.PaxID } }, Code = "914_16", Type = "Warning" });

                            }
                        }

                    }
                }

            }

            if (warnings.Count != 0)
            {
                var custIds = warnings.Select(x => x.DynamicMessagesContents.First().ReplaceWith).Distinct();
                request.Request.DataLists.PaxList = request.Request.DataLists.PaxList.Select(x =>
                {
                    if (custIds.Contains(x.PaxID)) { x.IdentityDoc = null; x.RedressCase = null; }

                    return x;

                }).ToArray();
            }


            return warnings;
        }

        public static IEnumerable<ErrorDetail> ValidateContactInfoName(IATA_OrderCreateRQ request)
        {
            var errors = new List<ErrorDetail>();
            var paxList = new List<string>();
            var primaryContactInfo = GetPrimaryContactInfo(request);
            var primaryContact = request.Request.DataLists.PaxList.Where(x => x.PTC == Constants.Application.AdultPTCCode && x.ContactInfoRefID == primaryContactInfo?.ContactInfoID);

            if (request.Request.DataLists.ContactInfoList.IsNullOrEmpty() || primaryContactInfo?.ContactInfoID.IsNullOrEmpty() != false
                || primaryContact.IsNullOrEmpty() || primaryContact.Any(x => x.Individual.GivenName.IsNullOrEmpty() && x.Individual.Surname.IsNullOrEmpty())
                || primaryContactInfo.Phone.IsNullOrEmpty() || primaryContactInfo.EmailAddress.IsNullOrEmpty())
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_5" });

            foreach (var pax in request.Request.DataLists.PaxList)
                paxList.Add(pax.Individual?.GivenName.FirstOrDefault() + pax.Individual?.Surname);

            var paxUniquelist = paxList.Distinct();

            if (paxUniquelist.Count() != paxList.Count)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "399_1" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateEmailAndPhone<T>(IATA_OrderCreateRQ request)
        {
            var errors = new List<ErrorDetail>();
            var primaryContactInfo = GetPrimaryContactInfo(request);

            if (primaryContactInfo != null && !primaryContactInfo.EmailAddress.IsNullOrEmpty())
            {
                if (primaryContactInfo.EmailAddress.FirstOrDefault().EmailAddressText.IsNullOrEmpty())
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "316_1" });
                else if (primaryContactInfo.EmailAddress.FirstOrDefault().EmailAddressText.Length > 64 || !RegexCompiled.Match(primaryContactInfo.EmailAddress.FirstOrDefault().EmailAddressText, @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$").Success)
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "317_1" });
            }

            if (primaryContactInfo != null && !primaryContactInfo.Phone.IsNullOrEmpty())
            {
                if (primaryContactInfo.Phone.FirstOrDefault().PhoneNumber.IsNullOrEmpty())
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "316_2" });
                else
                {
                    if (primaryContactInfo.Phone.FirstOrDefault().PhoneNumber.Length > 25 || !RegexCompiled.Match(primaryContactInfo.Phone.FirstOrDefault().PhoneNumber, Constants.Application.RegexToValidateNumeric).Success)
                        errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "317_2" });

                    if (primaryContactInfo.Phone.FirstOrDefault().CountryDialingCode.IsNullOrEmpty() || !RegexCompiled.Match(primaryContactInfo.Phone.FirstOrDefault().CountryDialingCode, Constants.Application.RegexToValidateCountryCode).Success)
                        errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "317_3" });
                }
            }

            return errors;
        }

        private static ContactInfoType GetPrimaryContactInfo(IATA_OrderCreateRQ request)
        {
            var dataLists = request?.Request?.DataLists;
            var primaryContactInfo = dataLists?.ContactInfoList?.FirstOrDefault();
            if (!IsContactRefusedWithoutContactDetails(primaryContactInfo))
            {
                return primaryContactInfo;
            }

            var adultContactInfoRefIds = dataLists.PaxList
                .Where(pax => pax.PTC == Constants.Application.AdultPTCCode)
                .Select(pax => pax.ContactInfoRefID);

            return dataLists.ContactInfoList
                .Where(contactInfo => adultContactInfoRefIds.Contains(contactInfo.ContactInfoID))
                .FirstOrDefault(contactInfo => !IsContactRefusedWithoutContactDetails(contactInfo));
        }

        private static bool IsContactRefusedWithoutContactDetails(ContactInfoType contactInfo)
        {
            return contactInfo != null
                && contactInfo.ContactRefusedInd
                && contactInfo.EmailAddress.IsNullOrEmpty()
                && contactInfo.Phone.IsNullOrEmpty();
        }

        public static IEnumerable<ErrorDetail> ValidateBirthDate<T>(IATA_OrderCreateRQ request, OfferResponse offerDoc)
        {
            var errors = new List<ErrorDetail>();

            if (offerDoc != null && offerDoc.DataList != null)
            {
                foreach (var pax in request.Request.DataLists.PaxList)
                {
                    DateTime? deptDate = Convert.ToDateTime(offerDoc.DataList.FlightSegments?.First().ScheduledDepartureTime.LocalDateTime);
                    DateTime birthDate = Convert.ToDateTime(pax.Individual?.Birthdate);
                    double? deptcount = (deptDate?.Year * 100 + deptDate?.Month) * 100 + deptDate?.Day;
                    double birthcount = (birthDate.Year * 100 + birthDate.Month) * 100 + birthDate.Day;
                    double? age = (deptcount - birthcount) / 10000;

                    if (birthDate > DateTime.Today)
                        errors.Add(new ErrorDetail { IsDynamicMessage = true, ErrorCode = "73_6", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<<PAX n>>", ReplaceWith = pax.PaxID } } });

                    else if (pax.PTC == Constants.Application.AdultPTCCode)
                    {
                        if (!(age < 100 && age >= 16))
                            errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_1" });
                    }

                    else if (pax.PTC == Constants.Application.InfantPTCCode)
                    {
                        if (!(age < 2 && age > 0))
                            errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_5" });
                    }

                    else if (pax.PTC == Constants.Application.ChildPTCCode)
                    {
                        if (!(age < 12 && age >= 2))
                            errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_3" });
                    }

                    else if (pax.PTC == Constants.Application.YoungPTCCode)
                    {
                        if (!(age < 16 && age >= 12))
                            errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "73_4" });
                    }

                }
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateSeatOffer(IATA_OrderCreateRQ request, IEnumerable<FlightSegmentSeatMap> seat)
        {
            var errors = new List<ErrorDetail>();
            double catalogueAmount = 0;
            var offers = (request.Request.CreateOrder.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First(x => x.SelectedOfferItem.Any(y => y.SelectedSeat != null));

            foreach (var offItems in offers.SelectedOfferItem)
            {
                bool found = false;
                var catalogue = seat?.SelectMany(c => c.Catalogue.Where(x => x.CatalogueId == offItems.OfferItemRefID)).Distinct();

                if (catalogue != null && catalogue.Any())
                {
                    found = true;
                    catalogueAmount = catalogueAmount + catalogue.Sum(x => Convert.ToDouble(x.SeatPrice.MonetaryAmount));

                    if (catalogue.FirstOrDefault().SeatPrice.BaseCurrency != offers.TotalOfferPriceAmount.CurCode)
                        errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_3" });

                }

                if (!found && !errors.Exists(e => e.ErrorCode == "911_10"))
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "911_10" });
            }

            if (errors.Count == 0 && catalogueAmount != Convert.ToDouble(offers.TotalOfferPriceAmount.Value))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = true,
                    ErrorCode = "290_3",
                    DynamicMessagesContents = new List<DynamicMessagesContent>()
                    {
                        new DynamicMessagesContent()
                        {
                            Fieldname ="<<TotalAmount>>",
                            ReplaceWith = catalogueAmount.ToString()
                        }
                    }
                });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateCreditCardFOP(IATA_OrderCreateRQ request, bool isCreditCardAccepted)
        {
            if ((request.Request.PaymentFunctions != null && request.Request.PaymentFunctions.First()?.PaymentProcessingDetails?.PaymentMethod?.Item is not PaymentProcessingPaymentCardType) || isCreditCardAccepted == false)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "466_2" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateIFGAgentsPermissionErrorResponse(IATA_OrderCreateRQ request, IFGAgentFopLookUpResponse ifgAgentFopLookUpResponse)
        {
            var cashPaymentMethodType = (request.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as SettlementPlanType)?.PaymentTypeCode;
            var easyPayPaymentMethodType = request.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item as PaymentProcessingIATA_EasyPayType;

            if (ifgAgentFopLookUpResponse is null)

                return [new()
                {
                    IsDynamicMessage = true,
                    ErrorCode = "911_50",
                    DynamicMessagesContents = new List<DynamicMessagesContent>()
                    {
                        new DynamicMessagesContent()
                        {
                            Fieldname = "<<Reason>>",
                            ReplaceWith = "IFG Agent LookUp returned null/unavailable response"
                        }
                    }
                }];

            if (ifgAgentFopLookUpResponse.HasTicketingAuthority is false)

                return [new() { IsDynamicMessage = false, ErrorCode = "368_11" }];

            if (cashPaymentMethodType != null && ifgAgentFopLookUpResponse.BspCashAllowed is false)

                return [new() { IsDynamicMessage = false, ErrorCode = "368_9" }];

            if (easyPayPaymentMethodType != null && ifgAgentFopLookUpResponse.EasyPayAllowed is false)

                return [new() { IsDynamicMessage = false, ErrorCode = "368_10" }];          

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateSecurePaymentVersion2(PaymentFunctionsCreateType[]? paymentFunctions)
        {
            if (paymentFunctions?.Any(x => x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderCreateRQ.PaymentProcessingPaymentCardType card && card.SecurePaymentVersion2 == null) == true)
            {
                return
                [
                    new()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_43"
                    }
                ];
            }

            return [];
        }

        public static IEnumerable<ErrorDetail> ValidateSecurePaymentVersion2Data(PaymentFunctionsCreateType[]? paymentFunctions)
        {
            var errors = new List<ErrorDetail>();

            var cards = paymentFunctions?.Select(x => x?.PaymentProcessingDetails?.PaymentMethod?.Item).OfType<Domain.OrderCreateRQ.PaymentProcessingPaymentCardType>();

            if (cards == null || !cards.Any())
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_9" });
            }

            if (cards.Any(card => string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.DirectoryServerTrxID)))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_7" });
            }

            if (cards.Any(card => string.IsNullOrWhiteSpace(card.SecureProgram?.EnrollmentStatusText)))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_8" });
            }

            if (cards.Any(card =>
                !string.IsNullOrWhiteSpace(card.SecureProgram?.EnrollmentStatusText) &&
                !string.Equals(card.SecureProgram.EnrollmentStatusText, "CardEnrolled3DS2.1.0", StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_9" });
            }

            if (cards.Any(card => string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.ElectronicCommerceInd)))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_10" });
            }

            if (cards.Any(card =>
                !string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.ElectronicCommerceInd) &&
                card.SecurePaymentVersion2.ElectronicCommerceInd is not ("05" or "02")))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_11" });
            }

            if (cards.Any(card =>
                card.SecurePaymentVersion2?.TrxStatusText == null ||
                !card.SecurePaymentVersion2.TrxStatusText.Any() ||
                card.SecurePaymentVersion2.TrxStatusText.All(string.IsNullOrWhiteSpace)))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_12" });
            }

            if (cards.Any(card =>
                card.SecurePaymentVersion2?.TrxStatusText?.Any(s =>
                    !string.Equals(s, "Y", StringComparison.OrdinalIgnoreCase)) == true))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_13" });
            }

            if (cards.Any(card => string.IsNullOrWhiteSpace(card.SecurePaymentVersion2?.AuthenticationValue)))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_44" });
            }

            return errors;
        }
    }

}

