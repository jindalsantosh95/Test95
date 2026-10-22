using CSharpFunctionalExtensions;
using Gremlin.Net.Process.Traversal;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Helper;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Location;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Order.Validations.Common;
using OrderDocument = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderDocument;

namespace VAA.EAPI.NDC.Order.Validations.OrderChange
{
    public class OrderChangeValidations
    {
        public static Response<T> ValidateOrderChangeRequest<T>(IATA_OrderChangeRQ orderChangeRequest, bool motoPayment,IHostEnvironment hostEnvironment, [Optional] bool isNameChange)
        {
            var errorIds = new List<ErrorDetail>();

            if (((orderChangeRequest.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod.Item) as PaymentProcessingPaymentCardType) != null)
                errorIds.AddRange(LuhnCheckValidation.ValidateCard(orderChangeRequest.Request?.PaymentFunctions));

            if (((orderChangeRequest.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod.Item) as PaymentProcessingPaymentCardType) != null)
                errorIds.AddRange(LuhnCheckValidation.ValidateSecurePaymentVersion2(orderChangeRequest.Request?.PaymentFunctions));

            if (!motoPayment) errorIds.AddRange(OrderChangeRequestValidations.ValidateSecurePaymentVersion2Data(orderChangeRequest.Request?.PaymentFunctions));

            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqDistributionEntity<T>(orderChangeRequest.DistributionChain));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqOrder<T>(orderChangeRequest));

            if (((orderChangeRequest.Request?.ChangeOrderChoice?.Item) as AcceptCancelOfferType) == null)
            {
                errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqChangeOrderChoice<T>(orderChangeRequest));
                if (!isNameChange) errorIds.AddRange(OrderChangeRequestValidations.ValidatePaxList(orderChangeRequest));
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderChangeAmendApis<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqDistributionEntity<T>(orderChangeRequest.DistributionChain));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqOrder<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidatePostOrderChangeRequestBookingId<T>(orderChangeRequest));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateCancelHoldBookingRequest<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            var errorIds = OrderChangeRequestValidations.ValidateCancelUnpaidOrder(orderChangeRequest);

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderChangeSplitPnr<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqDistributionEntity<T>(orderChangeRequest.DistributionChain));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateIFGAgentsPermission(IATA_OrderChangeRQ request, IFGAgentFopLookUpResponse ifgAgentFopLookUpResponse, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            if (errorIds.Count == 0)
                errorIds.AddRange(OrderChangeRequestValidations.ValidateIFGAgentsPermissionErrorResponse(request, ifgAgentFopLookUpResponse));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateAPISDetails<T>(IATA_OrderChangeRQ request, OrderDocument orderDoc, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            var validDocumentTypes = new List<string>() { Constants.Application.PT, Constants.Application.GC, Constants.Application.CR, Constants.Application.VI, Constants.Application.AEA };

            var changeOrderChoice = (request.Request.ChangeOrderChoice.Item) as OrderChangeUpdatePaxType;
            var pax = request.Request.DataLists.PaxList.Where(x => x.PaxID == changeOrderChoice.New.PaxRefID).First();
            var matchingCust = orderDoc.DataList.CustomerDetails.First(x => x.CustId == changeOrderChoice.Current.PaxRefID);

            if (pax.RedressCase != null)
            {
                errorIds.AddRange(from doc in pax.RedressCase
                                  where !RegexCompiled.Match(doc.RedressCaseID, Constants.Application.RegexToValidateAlphaNumeric).Success || doc.CountryCode?.Length > 3 || (doc.CountryCode != null && !RegexCompiled.Match(doc.CountryCode, Constants.Application.RegexToValidateAlphabets).Success)
                                  select new ErrorDetail { IsDynamicMessage = true, ErrorCode = "914_16", DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent() { Fieldname = "<<PAX n>>", ReplaceWith = changeOrderChoice.Current.PaxRefID } } });
            }

            if (errorIds.Count == 0 && pax.IdentityDoc != null)
            {
                if (pax.IdentityDoc.Where(td => td.IdentityDocTypeCode == Constants.Application.PT).Count() > 1 ||
                    pax.IdentityDoc.Where(td => td.IdentityDocTypeCode == Constants.Application.GC).Count() > 1 ||
                    pax.IdentityDoc.Where(td => td.IdentityDocTypeCode == Constants.Application.CR).Count() > 1)

                    errorIds.Add(new ErrorDetail { IsDynamicMessage = true, ErrorCode = "914_16", DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent() { Fieldname = "<<PAX n>>", ReplaceWith = changeOrderChoice.Current.PaxRefID } } });

                if (errorIds.Count == 0)
                {
                    errorIds.AddRange((from requestdoc in pax.IdentityDoc
                                       where !string.IsNullOrEmpty(requestdoc.GivenName?.First()) && !string.Equals(matchingCust.FirstName, requestdoc.GivenName.First(), StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(requestdoc.Surname) && !string.Equals(matchingCust.LastName, requestdoc.Surname, StringComparison.OrdinalIgnoreCase)
                                             || !string.IsNullOrEmpty(requestdoc.Birthdate.ToString()) && requestdoc.BirthdateSpecified && DateTime.ParseExact(matchingCust.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date != requestdoc.Birthdate
                                             || (!string.IsNullOrEmpty(requestdoc.GenderCode.ToString()) && requestdoc.GenderCodeSpecified && matchingCust.Gender != requestdoc.GenderCode.ToString())
                                       select new ErrorDetail { IsDynamicMessage = true, ErrorCode = "914_16", DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent() { Fieldname = "<<PAX n>>", ReplaceWith = changeOrderChoice.Current.PaxRefID } } }).DistinctBy(x => x.ErrorCode));
                }

                if (errorIds.Count == 0)
                {
                    errorIds.AddRange(from docs in pax.IdentityDoc
                                      where (!string.IsNullOrEmpty(docs.IdentityDocID)) && !RegexCompiled.Match(docs.IdentityDocID, Constants.Application.RegexToValidateAlphaNumeric).Success || !validDocumentTypes.Contains(docs.IdentityDocTypeCode)
                                         || (docs.IssueDateSpecified && Convert.ToDateTime(docs.IssueDate).CompareTo(DateTime.Today) > 0) || (docs.ExpiryDateSpecified && Convert.ToDateTime(docs.ExpiryDate).CompareTo(DateTime.Today) < 0)
                                         || docs.ResidenceCountryCode?.Length > 3 || docs.IssuingCountryCode?.Length > 3 ||
                                         (!string.IsNullOrEmpty(docs.ResidenceCountryCode) && !RegexCompiled.Match(docs.ResidenceCountryCode, Constants.Application.RegexToValidateAlphabets).Success) ||
                                         (!string.IsNullOrEmpty(docs.IssuingCountryCode) && !RegexCompiled.Match(docs.IssuingCountryCode, Constants.Application.RegexToValidateAlphabets).Success)

                                      select new ErrorDetail { IsDynamicMessage = true, ErrorCode = "914_16", DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent() { Fieldname = "<<PAX n>>", ReplaceWith = changeOrderChoice.Current.PaxRefID } } });
                }
            }

            if (errorIds.Count == 0 && pax.IdentityDoc != null && pax.IdentityDoc.Any(x => x.Visa != null))
            {
                foreach (var travelDoc in pax.IdentityDoc.Where(x => x.Visa != null))
                {
                    errorIds.AddRange(from visadoc in travelDoc.Visa
                                      where (visadoc.HostCountryCode != null && !RegexCompiled.Match(visadoc.HostCountryCode, Constants.Application.RegexToValidateAlphabets).Success) ||
                                        (visadoc.IssuingCountryCode != null && !RegexCompiled.Match(visadoc.IssuingCountryCode, Constants.Application.RegexToValidateAlphabets).Success) ||
                                        (visadoc.ExpiryDateSpecified && Convert.ToDateTime(visadoc.ExpiryDate).CompareTo(DateTime.Today) < 0)
                                        || !RegexCompiled.Match(visadoc.VisaID, Constants.Application.RegexToValidateAlphaNumeric).Success

                                      select new ErrorDetail { IsDynamicMessage = true, ErrorCode = "914_16", DynamicMessagesContents = new List<DynamicMessagesContent>() { new DynamicMessagesContent() { Fieldname = "<<PAX n>>", ReplaceWith = changeOrderChoice.Current.PaxRefID } } });

                }
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOTPaymentFunctions<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            IEnumerable<ErrorDetail> errors = null;
            var request = orderChangeRequest.Request.PaymentFunctions.FirstOrDefault().PaymentProcessingDetails;

            if (request.Amount.Value == 0 && request.PaymentMethod.Item as OfflinePaymentType != null)
            {
                errors = OrderChangeRequestValidations.ValidateOfflinePayment((OfflinePaymentType)request.PaymentMethod.Item);
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateDisruptionPaymentFunctions<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            var errors = orderChangeRequest?.Request?.PaymentFunctions?.Any() == true
                ? new List<ErrorDetail>
                {
                    new()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "914_19"
                    }
                }
                : null;

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateDbOffer(OfferResponse airlineOffers, string offerId, IHostEnvironment hostingEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            if (airlineOffers?.Offers == null)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_2",
                });
            }
            else if (!airlineOffers.Offers.Any(x => x.OfferId == offerId))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_2",
                });
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public static Response<T> ValidateDbOrder(OrderRS airlineOrders, IHostEnvironment hostingEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            if (airlineOrders?.OrderDocument == null)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_1",
                });
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public static Response<T> ValidateAcceptSellerFollowUpAction<T>(OrderDocument orderDocument , IATA_OrderChangeRQ request , IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            var selectedOrderItemIds = ((SelectedOrderItemType)request.Request.ChangeOrderChoice?.Item)?.OrderItemRefID;
            var acceptedOrderItemIds = orderDocument.OrderItems.Where(oi => oi.SellerFollowUpAction != null && oi.SellerFollowUpAction.Any(a => a.Is("Accept"))).Select(x => x.OrderItemId);

            if (selectedOrderItemIds?.Length == 0 || !acceptedOrderItemIds.SequenceEqual(selectedOrderItemIds))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_1",
                });
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateCancelSellerFollowUpAction<T>(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();
            if (!orderDocument.OrderItems.Any(oi => oi.SellerFollowUpAction != null && oi.SellerFollowUpAction.Any(a => a.Is("Cancel"))) && orderDocument.DataList.SegmentDetails.Any(x => x.ScheduleChangeIndicator))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "915_2",
                });
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateCancelHoldBooking<T>(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();
            if (orderDocument?.OrderItems?.Any(orderItem => orderItem?.AccountableDocumentRefs is not null) == true)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "395_1",
                });
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateCancelHoldBookingPaxList<T>(IATA_OrderChangeRQ request, OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var requestPaxIds = request?.Request?.DataLists?.PaxList?.Select(pax => pax.PaxID).OrderBy(paxId => paxId, StringComparer.Ordinal);
            var storedPaxIds = orderDocument?.DataList?.CustomerDetails?.Select(customer => customer.CustId).OrderBy(paxId => paxId, StringComparer.Ordinal);

            if (requestPaxIds is null || storedPaxIds is null || !requestPaxIds.SequenceEqual(storedPaxIds, StringComparer.Ordinal))
            {
                return ErrorHelper<T>.GetErrors( [new ErrorDetail { IsDynamicMessage = false, ErrorCode = "143_3" }], hostEnvironment);
            }

            return ErrorHelper<T>.GetErrors(Enumerable.Empty<ErrorDetail>(), hostEnvironment);
        }

        public static Response<T> ValidateCancelHoldBookingOwnership<T>(OrderDocument orderDocument, ValidationRS validationResponse, IHostEnvironment hostEnvironment)
        {
            var storedParties = orderDocument?.Parties?.Participants;
            var validationDetails = validationResponse?.ValidateResponse;
            var storedSeller = storedParties?.FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Seller)?.AgencyID;
            var storedDistributor = storedParties?.FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Distributor)?.AgencyID;
            var validatedSeller = validationDetails?.AgencyDetails?.RequestAgencyId;
            var validatedDistributor = validationDetails?.AggregatorDetails?.AgencyId;

            var sellerMatches = !string.IsNullOrWhiteSpace(storedSeller) && !string.IsNullOrWhiteSpace(validatedSeller) && string.Equals(storedSeller, validatedSeller, StringComparison.Ordinal);
            var isDirect = string.Equals(validationDetails?.KeyType, "DIRECT", StringComparison.Ordinal);
            var isIndirect = string.Equals(validationDetails?.KeyType, "INDIRECT", StringComparison.Ordinal);
            var distributorMatches = (isDirect && string.IsNullOrWhiteSpace(storedDistributor)) || (isIndirect && !string.IsNullOrWhiteSpace(storedDistributor) &&!string.IsNullOrWhiteSpace(validatedDistributor) && string.Equals(storedDistributor, validatedDistributor, StringComparison.Ordinal));

            if (!sellerMatches || !distributorMatches)
            {
                return ErrorHelper<T>.GetErrors( [new ErrorDetail { IsDynamicMessage = false, ErrorCode = "368_2" }], hostEnvironment);
            }

            return ErrorHelper<T>.GetErrors(Enumerable.Empty<ErrorDetail>(), hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateGSTDetails<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();

            warnings.AddRange(OrderChangeRequestValidations.ValidateGSTNumberandOrgName(orderChangeRequest));
            warnings.AddRange(OrderChangeRequestValidations.ValidateGSTAddress(orderChangeRequest));
            warnings.AddRange(OrderChangeRequestValidations.ValidateGSTEmailAddress(orderChangeRequest));
            warnings.AddRange(OrderChangeRequestValidations.ValidateGSTPhone(orderChangeRequest));

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateGSTDetailsAgainstOrderDoc(OrderDocument orderDoc, IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(OrderChangeRequestValidations.ValidateGSTAgainstOrderDoc(orderDoc, orderChangeRequest), hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateAPISDetails<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment, OrderDocument orderDoc)
        {
            var warnings = new List<NDCWarning>();
            List<string> validDocumentTypes = new List<string>() { Constants.Application.PT, Constants.Application.GC, Constants.Application.CR, Constants.Application.VI, Constants.Application.AEA };

            foreach (var y in orderChangeRequest.Request.DataLists.PaxList)
            {
                var orderDocCust = orderDoc.DataList.CustomerDetails.First(x => x.CustId == y.PaxID);

                if (y.Individual == null)
                    y.Individual = new() { GivenName = [orderDocCust.FirstName], Surname = orderDocCust.LastName };
                else
                {
                    y.Individual.GivenName = [orderDocCust.FirstName];
                    y.Individual.Surname = orderDocCust.LastName;
                }

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
                                          where (doc.Visa == null && doc.GivenName != null && (!string.Equals(doc.GivenName.FirstOrDefault(), y.Individual.GivenName.FirstOrDefault(), StringComparison.CurrentCultureIgnoreCase) || !string.Equals(doc.Surname, y.Individual.Surname, StringComparison.CurrentCultureIgnoreCase)))
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

                        if (!warnings.Any(x => x.DynamicMessagesContents?.FirstOrDefault()?.ReplaceWith == y.PaxID))
                        {
                            var gender = orderDocCust.Gender ?? y.Individual.GenderCode.ToString();
                            var dob = orderDocCust.DateOfBirth != null ? Convert.ToDateTime(orderDocCust.DateOfBirth) : y.Individual.Birthdate;

                            if (y.IdentityDoc.Any(td => (td.GenderCodeSpecified && Convert.ToString(td.GenderCode) != gender) || (td.BirthdateSpecified && td.Birthdate != dob)))
                                warnings.Add(new NDCWarning { IsDynamicMessage = true, DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<<PAX n>>", ReplaceWith = y.PaxID } }, Code = "914_16", Type = "Warning" });
                        }

                    }
                }

            }

            if (warnings.Count != 0)
            {
                var custIds = warnings.Select(x => x.DynamicMessagesContents.First().ReplaceWith).Distinct();
                orderChangeRequest.Request.DataLists.PaxList = orderChangeRequest.Request.DataLists.PaxList.Select(x =>
                {
                    if (custIds.Contains(x.PaxID)) { x.IdentityDoc = null; x.RedressCase = null; }

                    return x;

                }).ToArray();
            }
            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static Response<T> ValidateOrderChangeSplitPnrDbRequest<T>(OrderRS orderDataRS, AugmentationPoint augmentationPoint, IHostEnvironment hostEnvironment, IATA_OrderChangeRQ request)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(OrderChangeRequestValidations.ValidateSplitPnrPaxList(orderDataRS, augmentationPoint));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateSplitPnrPartialFlownStatus(orderDataRS));

            if (errorIds.Count() == 0 && orderDataRS.OrderDocument.Parties.Participants.Any(x => x.RequestAgentType == Constants.Application.IndirectAgent))
            {
                errorIds.AddRange(OrderChangeRequestValidations.ValidateSplitPnrContactInfo(orderDataRS.OrderDocument.DataList, augmentationPoint, request));
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderChangeHoldBookingRequest<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            bool isPostSaleSSR = orderChangeRequest.Request?.PaymentFunctions == null;
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderIDHoldBooking<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateDistributionEntityHoldBooking<T>(orderChangeRequest.DistributionChain));
            if (!isPostSaleSSR) errorIds.AddRange(OrderChangeRequestValidations.ValidatePaxList(orderChangeRequest));

            if (orderChangeRequest.Request?.PaymentFunctions?.First().PaymentProcessingDetails?.PaymentMethod.Item != null)
                errorIds.AddRange(LuhnCheckValidation.ValidateCard(orderChangeRequest.Request?.PaymentFunctions));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateVaOrderChangeRequestOfferStore<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment, ValidationRS validationRS, OfferResponse offerDoc, [Optional] bool isCashPayment, [Optional] bool isEvenExchange, [Optional] bool isUnEvenExchangeRefund, [Optional] bool isUnEvenExchangeAddCollect)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidateOfferExpiryTimeLimit(offerDoc));

            if (errorIds.IsNullOrEmpty() && (isEvenExchange || isUnEvenExchangeRefund || isUnEvenExchangeAddCollect)) errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeExchangeOffer(orderChangeRequest, offerDoc, isEvenExchange, isUnEvenExchangeRefund, isUnEvenExchangeAddCollect));

            if (!errorIds.Any())
            {
                if (!isCashPayment) errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeRQFOP(orderChangeRequest, offerDoc.Parties.Participants.First().IsCreditCardAccepted));
                errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeRQBookingIDOfferStore(orderChangeRequest, offerDoc));
                errorIds.AddRange(OrderChangeRequestValidations.ValidationOrderChangeRQChangeOrderChoiceOfferStore(orderChangeRequest.Request?.ChangeOrderChoice?.Item as AcceptSelectedQuotedOfferListType, offerDoc));
                errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeRQOrderOfferStore(orderChangeRequest, offerDoc));
                errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeRQPaxIDOfferDoc(orderChangeRequest.Request?.ChangeOrderChoice?.Item as AcceptSelectedQuotedOfferListType, offerDoc));
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateChangeReshopHoldBooking<T>(IATA_OrderChangeRQ orderChangeRequest, OfferResponse offerDoc, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(OrderChangeRequestValidations.ValidateChangeReshopHoldBookingAmount(orderChangeRequest, offerDoc), hostEnvironment);
        }

        public static Response<T> ValidateOrderChangeFormOfPayment<T>(IATA_OrderChangeRQ orderChangeRequest, OrderRS orderDoc, IHostEnvironment hostEnvironment, bool isPostSaleSeat = false)
        {
            var errorIds = new List<ErrorDetail>();

            if (!isPostSaleSeat) errorIds.AddRange(OrderChangeRequestValidations.ValidateFOP(orderChangeRequest, orderDoc));
            errorIds.AddRange(OrderChangeRequestValidations.ValidatePrivateFareFOP(orderChangeRequest, orderDoc));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderChangeRequestFOP<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment, bool isCreditCardAccepted)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeRQFOP(orderChangeRequest, isCreditCardAccepted));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateFlightServiceList<T>(IATA_OrderChangeRQ orderChangeRequest, FlightOfferServiceRS flightOfferServiceRS, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(ValidateFlightServieListResponse.ValidateFlightServiceListErrorResponse(flightOfferServiceRS));

            if (!errorIds.Any())
                errorIds.AddRange(ValidateFlightServieListResponse.ValidateFlightServiceListResponseOrderChange(flightOfferServiceRS, orderChangeRequest.DistributionChain.OrderBy(x => x.Ordinal).ToList(), orderChangeRequest.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateOrderSearchResponse<T>(string iataNum, Air4OrderSearchMSResponse orderSearchResponse, bool skipGDSAgentValidation, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderSearchResponseValidation.ValidateOrderSearchNullResponse(orderSearchResponse));

            if (!errorIds.Any() && orderSearchResponse.ErrorDetails != null) errorIds.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRS(orderSearchResponse.ErrorDetails));

            if (!errorIds.Any() && skipGDSAgentValidation) errorIds.AddRange(OrderSearchResponseValidation.ValidateOrderSearchRSForGDSAgent(orderSearchResponse.OrderSearchResponse, iataNum));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidatePostSaleOrderChangeSeatRQ<T>(IATA_OrderChangeRQ orderChangeRequest, SeatAvailabilityResponse seatofferDoc, IHostEnvironment hostEnvironment, bool isCashPayment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidatePostSaleOrderChangeSeatOfferDoc(orderChangeRequest, seatofferDoc));

            if (errorIds.Count == 0 && !isCashPayment) errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeRQFOP(orderChangeRequest, seatofferDoc.Parties.Participants.First().IsCreditCardAccepted));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateEvenOrderChangeRQ<T>(IATA_OrderChangeRQ orderChangeRequest, OfferResponse offerDoc, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidateEvenOrderChangeOfferDoc(orderChangeRequest, offerDoc));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static bool GSTValidationAsync(ValidationRS validationMSResult, IEnumerable<AirportDetail> airportDetails, OrderDocument orderDoc = null, OrderSearchResponse orderSearchRS = null)
        {
            bool isGSTValidationFailed = true;

            if (validationMSResult.ValidateResponse?.AgencyDetails?.CountryCode?.ToUpper() == Constants.Application.IndiaCountryCode)
            {
                if (orderDoc != null)
                {
                    if (orderDoc.DataList.SegmentDetails.Any(s => airportDetails.Any(a => a.AirportCode == s.OriginAirportCode && a.CountryCode == Constants.Application.IndiaCountryCode)))
                        isGSTValidationFailed = false;
                }
                else
                {
                    foreach (var segment in orderSearchRS?.SegmentDetails)
                    {
                        if (segment.OriginCountryCode.Equals(Constants.Application.IndiaCountryCode, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isGSTValidationFailed = false;
                            break;
                        }
                    }
                }
            }

            return isGSTValidationFailed;
        }

        public static Response<T> ValidatePostSaleOrderChangeRQAgainstSeatOfferDoc<T>(IATA_OrderChangeRQ orderChangeRequest, SeatAvailabilityResponse seatofferDoc, IEnumerable<SeatDetails> seatDetails, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidatePostSaleOrderChangeRQOfferItemIdandPrice(orderChangeRequest, seatofferDoc));

            if (!errorIds.Any())
                errorIds.AddRange(OrderChangeRequestValidations.ValidatePostSaleOrderChangeRQSeat(orderChangeRequest, seatDetails));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidatePostSaleOrderChangeRequestAgainstSeatMap<T>(IATA_OrderChangeRQ orderChangeRequest, IEnumerable<SeatDetails> seatDetails, SeatAvailabilityResponse seatofferDoc, IHostEnvironment hostEnvironment)
        {
            var errorIds = OrderChangeRequestValidations.ValidatePostSaleOrderChangeSeatAmountAndCurInSeatMap(orderChangeRequest, seatDetails, seatofferDoc);

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidatePostSaleOrderChangeRequestAgainstSeatMapForWarning<T>(IATA_OrderChangeRQ orderChangeRequest, IEnumerable<SeatDetails> seatDetails, SeatAvailabilityResponse seatofferDoc, IHostEnvironment hostEnvironment)
        {
            var warnings = OrderChangeRequestValidations.ValidatePostSaleOrderChangeSeatMapForWarning(orderChangeRequest, seatDetails, seatofferDoc);

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static Response<T> ValidatePostOrderChangeRequest<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            if (((orderChangeRequest.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod.Item) as PaymentProcessingPaymentCardType) != null)
                errorIds.AddRange(LuhnCheckValidation.ValidateCard(orderChangeRequest.Request?.PaymentFunctions));

            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqDistributionEntity<T>(orderChangeRequest.DistributionChain));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqOrder<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqChangeOrderChoice<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidatePostOrderChangeRequestBookingId<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidatePostOrderChangeRequestSeatNumber<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidatePaxList(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateSeatFOP(orderChangeRequest));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateVaPostSaleOrderChangeRequestForGds<T>(IATA_OrderChangeRQ request, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(OrderChangeRequestValidations.ValidateRequestPaxListWithNameDetails(request), hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateSeatFulfilmentResponse<T>(VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS.OrderChangeResponse orderChangeResponse, IEnumerable<SeatDetails> seatDetails, IEnumerable<NDCWarning> warnings, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings(OrderChangeRequestValidations.UpdatePriceMismatchAmount(warnings, orderChangeResponse, seatDetails), hostEnvironment);
        }

        public static Response<T> ValidateEvenOrderChangeRQAgainstOfferDoc<T>(IATA_OrderChangeRQ orderChangeRequest, OfferResponse offerDoc, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqDistributionEntity<T>(orderChangeRequest.DistributionChain));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqOrder<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidateOrderChangeReqChangeOrderChoice<T>(orderChangeRequest));
            errorIds.AddRange(OrderChangeRequestValidations.ValidatePostOrderChangeRequestBookingId<T>(orderChangeRequest));

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static Response<T> ValidateGenderAndDOBWithOrderRetrieve<T>(IATA_OrderChangeRQ orderChangeRequest, OrderDocument orderDoc, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(OrderChangeRequestValidations.ValidateGenderAndDOBinConfirmHB(orderChangeRequest, orderDoc), hostEnvironment);
        }

        public static Response<T> ValidatePaymentInfoOrderChange<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(OrderChangeRequestValidations.ValidatePaymentInformationOrderChange(orderChangeRequest), hostEnvironment);
        }

        public static Response<T> Validate3DS2<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(OrderChangeRequestValidations.Validate3DS2(orderChangeRequest), hostEnvironment);
        }

        public static Response<T> Validate3DSecureAuthenticationData<T>(IATA_OrderChangeRQ orderChangeRequest, IHostEnvironment hostEnvironment)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(OrderChangeRequestValidations.ValidateEnrollStatus((PaymentProcessingPaymentCardType)orderChangeRequest?.Request?.PaymentFunctions?.FirstOrDefault().PaymentProcessingDetails.PaymentMethod.Item));

            if (errorIds.Count == 0)
            {
                var cardEnrolledStatus = (CardEnrollStatus)CardEnrolledStatus.GetCardEnrolledStatus((PaymentProcessingPaymentCardType)orderChangeRequest?.Request?.PaymentFunctions?.FirstOrDefault().PaymentProcessingDetails.PaymentMethod.Item);
                if (cardEnrolledStatus == CardEnrollStatus.CardEnrolled3DS2)
                {
                    errorIds.AddRange(OrderChangeRequestValidations.Validate3DS2Request((PaymentProcessingPaymentCardType)orderChangeRequest?.Request?.PaymentFunctions?.FirstOrDefault().PaymentProcessingDetails.PaymentMethod.Item, ((PaymentProcessingPaymentCardType)(orderChangeRequest?.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item))?.CardTypeText));
                }
                else if (cardEnrolledStatus == CardEnrollStatus.CardEnrolled3DS1 || cardEnrolledStatus == CardEnrollStatus.CardNotEnrolled3DS || cardEnrolledStatus == CardEnrollStatus.Unknown)
                {
                    errorIds.AddRange(OrderChangeRequestValidations.ValidateNon3DS2Request((PaymentProcessingPaymentCardType)orderChangeRequest?.Request?.PaymentFunctions?.FirstOrDefault().PaymentProcessingDetails.PaymentMethod.Item));
                }
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateSSROrderItems(OrderDocument orderDocument, IHostEnvironment hostEnvironment, List<NDCWarning> nDCWarnings)
        {
            var warnings = new List<NDCWarning>();

            if (!orderDocument.Warnings.IsNullOrEmpty())
            {
                if (orderDocument.Warnings.Any(x => x.Code == "185") && !nDCWarnings.Any(x => x.Code == "185_1")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "185_1", Type = "Warning" });

                if (orderDocument.Warnings.Any(x => x.Code == "182") && !nDCWarnings.Any(x => x.Code == "182_1")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "182_1", Type = "Warning" });

                if (orderDocument.Warnings.Any(x => x.Code == "186") && !nDCWarnings.Any(x => x.Code == "185_2")) warnings.Add(new NDCWarning { IsDynamicMessage = false, Code = "185_2", Type = "Warning" });

                if (orderDocument.Warnings.Any(x => x.Code == "173") && !nDCWarnings.Any(x => x.Code == "173_1"))
                {
                    warnings.Add(new NDCWarning
                    {
                        IsDynamicMessage = false,
                        Code = "173_1",
                        Type = "Warning"
                    });
                }
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> SSRTechnicalWarning(IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings([new NDCWarning { IsDynamicMessage = false, Code = "911_47", Type = "Warning" }], hostEnvironment);
        }

        public static Response<T> ValidateOrderChangePrvateFareFOP<T>(IATA_OrderChangeRQ orderChangeRQ, OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            bool isCardTypeCode = orderChangeRQ.Request.PaymentFunctions?.FirstOrDefault()?.PaymentMethodCriteria?.PaymentTypeCode == "CC";
            bool isCardNumber = orderChangeRQ.Request.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is EAPI.Domain.Virgin.Models.Payment.CardDetails { CardNumber: not null };

            if (orderDocument.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode && (isCardTypeCode || isCardNumber))
            {
                errors.Add(new() { IsDynamicMessage = false, ErrorCode = "466_3" });
            }

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);

        }


        public static IEnumerable<NDCWarning> ValidateRefundDetails(OfferResponse offerResponse, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();
            var totalRefundAmount = 0m;
            string currencyCode = null;

            foreach (var offer in offerResponse.Offers.First().OfferItems)
            {
                var paxCount = offer.Services.First().ServiceBeneficiaries.Count();
                var refundPrice = offer.OfferitemPricing.Select(p => p.RefundPrice).FirstOrDefault();

                totalRefundAmount += Math.Abs(Convert.ToDecimal(refundPrice?.FirstOrDefault()?.Amount?.MonetaryAmount) * paxCount);
                currencyCode = refundPrice?.FirstOrDefault()?.Amount.BaseCurrency;

            }

            var DocUrl = new Link()
            {
                Href = "https://help.virginatlantic.com/gb/en/contact-forms/refund-form.html?webform=ticket-refunds",
            };
            warnings.Add(new NDCWarning
            {
                IsDynamicMessage = true,
                Code = "745_1",
                DocURL = DocUrl,
                DynamicMessagesContents = new List<DynamicMessagesContent>
                {
                    new DynamicMessagesContent { Fieldname = "{XX.XX}", ReplaceWith = totalRefundAmount.ToString("F2") },
                    new DynamicMessagesContent { Fieldname = "{CurrencyCode}", ReplaceWith = currencyCode }
                }
            });

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static Response<T> ValidatePaymentStatus(OrderRS orderDocument, IHostEnvironment hostingEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            var paymentStatusPending = orderDocument?.OrderDocument?.OrderItems.Where(o => o.PaymentDetails != null).SelectMany(o => o.PaymentDetails).Any(p => p.PaymentProcessingStatus == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING);

            if ((bool)paymentStatusPending)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "76H_10",
                });
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public static IEnumerable<NDCWarning> PaymentReportingFailureWarning(IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>
            {
                new() { IsDynamicMessage = false, Code = "76H_10", Type = "Warning" }
            };

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }

        public static Response<T> ValidateSellerFollowUpAction<T>(OrderDocument orderDocument, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();
            if (!orderDocument.OrderItems.Any(oi => oi.SellerFollowUpAction != null && oi.SellerFollowUpAction.Any(a => a.Is("Cancel"))) && orderDocument.DataList.SegmentDetails.Any(x => x.ScheduleChangeIndicator))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "915_1",
                });
            }
            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }

        public static Response<T> ValidateCityCode<T>(CityCodeData cityCodeData, IHostEnvironment hostEnvironment)
        {
            var errors = new List<ErrorDetail>();

            if (cityCodeData == null || string.IsNullOrEmpty(cityCodeData.NDCCityCodeLocation))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_51",
                });
            }
            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }
        public static Response<T> ValidatePaymentFunctions<T>(IATA_OrderChangeRQ orderChangeRequest,IHostEnvironment hostEnvironment,OfferResponse offerDoc)
        {
            var errors = new List<ErrorDetail>();

            errors.AddRange(OrderChangeRequestValidations.ValidatePaymentFunctions(orderChangeRequest, offerDoc));

            return ErrorHelper<T>.GetErrors(errors, hostEnvironment);
        }
    }
}
