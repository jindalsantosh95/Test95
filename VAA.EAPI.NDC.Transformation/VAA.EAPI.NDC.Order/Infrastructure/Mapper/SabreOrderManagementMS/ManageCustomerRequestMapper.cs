using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using OrderDocument = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderDocument;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS
{
    public static class ManageCustomerRequestMapper
    {
        public static ManageCustomerRepoCommand MapOrderChangeHoldBookingRequest(IATA_OrderChangeRQ orderChangeRq, OrderDocument orderDoc, FlightOfferServiceRS flightServiceResponse, bool isGstValidationFailed)
        {
            return new ManageCustomerRepoCommand
            {
                ManageCustomerRequest = new ManageCustomerRequest()
                {
                    RLOC = orderChangeRq.Request.BookingRef.First().BookingID,
                    CustomerDetails = GetCustomerDetails(orderChangeRq, orderDoc, isGstValidationFailed),
                    Pos = orderDoc.PointOfSale,
                    Parties = orderDoc.Parties,
                    OrderInstructions = GetOrderInstructions(orderChangeRq, orderDoc, isGstValidationFailed, flightServiceResponse),
                    SegmentDetails = orderDoc.DataList.SegmentDetails
                }

            };
        }

        public static ManageCustomerRepoCommand MapOrderChangeAmendApisRequest(IATA_OrderChangeRQ orderChangeRQ, OrderDocument orderDoc, bool isAmendApis)
        {
            return new ManageCustomerRepoCommand
            {
                ManageCustomerRequest = new ManageCustomerRequest()
                {
                    RLOC = orderChangeRQ.Request.BookingRef.First().BookingID,
                    CustomerDetails = GetCustomerDetailsForAmendApis(orderChangeRQ, orderDoc, isAmendApis),
                    Pos = orderDoc.PointOfSale,
                    Parties = orderDoc.Parties,
                    AmendApis = true
                }

            };
        }

        public static ManageCustomerRepoCommand MapOrderChangeNameCorrectionRequest(IATA_OrderChangeRQ orderChangeRQ, OrderDocument orderDoc, OfferResponse offerDoc)
        {
            var isTicketedPnr = orderDoc.OrderItems.First().AccountableDocumentRefs != null;
            return new ManageCustomerRepoCommand
            {
                ManageCustomerRequest = new ManageCustomerRequest()
                {
                    RLOC = orderChangeRQ.Request.BookingRef.First().BookingID,
                    CustomerDetails = GetCustomerDetailsForNameCorrection(orderChangeRQ, orderDoc, offerDoc),
                    Pos = orderDoc.PointOfSale,
                    Parties = orderDoc.Parties,
                    TicketedNameCorrection = isTicketedPnr,
                    UnTicketedNameCorrection = !isTicketedPnr
                }

            };
        }

        private static IEnumerable<Customer> GetCustomerDetailsForNameCorrection(IATA_OrderChangeRQ orderChangeRQ, OrderDocument orderDoc, OfferResponse offerDoc)
        {
            var changeOrderChoice = orderChangeRQ.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType;
            var paxRefId = changeOrderChoice.SelectedPricedOffer.First().SelectedOfferItem.First().PaxRefID.First();

            var matchingCustomer = orderDoc.DataList.CustomerDetails.First(x => x.CustId == paxRefId);

            var updatedName = offerDoc.Offers.First().OfferItems.First().PaxNameChangeDetails;

            matchingCustomer.FirstName = updatedName.GivenName;
            matchingCustomer.LastName = updatedName.Surname;
            matchingCustomer.FirstNameNumber = matchingCustomer.FirstNameNumber;
            matchingCustomer.LastNameNumber = matchingCustomer.LastNameNumber;

            return new[] { matchingCustomer }.AsEnumerable();
        }

        private static IEnumerable<Customer> GetCustomerDetailsForAmendApis(IATA_OrderChangeRQ orderChangeRQ, OrderDocument orderDoc, bool isAmendApis)
        {
            var changeOrderChoice = (orderChangeRQ.Request.ChangeOrderChoice.Item) as OrderChangeUpdatePaxType;
            var matchingCustomerDetails = orderDoc.DataList.CustomerDetails.Where(x => x.CustId == changeOrderChoice.Current.PaxRefID);

            return orderChangeRQ.Request.DataLists.PaxList.Where(x => x.PaxID == changeOrderChoice.New.PaxRefID).Select(pax =>
            {
                return new Customer()
                {
                    FirstName = pax.Individual?.GivenName.FirstOrDefault() ?? matchingCustomerDetails.First().FirstName,
                    LastName = pax.Individual?.Surname ?? matchingCustomerDetails.First().LastName,
                    DateOfBirth = pax.BirthdateSpecified ? pax.Birthdate.ToString("yyyy-MM-dd") : matchingCustomerDetails.First().DateOfBirth,
                    Gender = pax.Individual != null && pax.Individual.GenderCodeSpecified ? Enum.GetName(typeof(GenderCodeType), pax.Individual?.GenderCode) : matchingCustomerDetails.First().Gender,
                    FirstNameNumber = matchingCustomerDetails.First().FirstNameNumber,
                    LastNameNumber = matchingCustomerDetails.First().LastNameNumber,
                    CustId = changeOrderChoice.Current.PaxRefID,
                    Ptc = pax.PTC,
                    TravelDocuments = GetTravelDocumentsAmendApis(pax, matchingCustomerDetails)
                };
            });
        }

        private static List<TravelDocument> GetTravelDocumentsAmendApis(PaxType pax, IEnumerable<Customer> customerDetails)
        {
            IdentityDocType[] identityDocuments = [];
            List<TravelDocument> travelDocuments = [];

            if (pax.RedressCase != null)
            {
                foreach (RedressCaseType redressCase in pax.RedressCase)
                {
                    travelDocuments.Add(new TravelDocument
                    {
                        DocumentType = Constants.Application.AEA,
                        DocumentNumber = redressCase.RedressCaseID,
                        CountryOfIssue = redressCase.CountryCode,
                        PersonName = new PersonName
                        {
                            FirstName = pax.Individual?.GivenName?.FirstOrDefault() ?? customerDetails.First().FirstName,
                            MiddleName = pax.Individual?.MiddleName?.FirstOrDefault(),
                            LastName = pax.Individual?.Surname ?? customerDetails.First().LastName,
                            Title = pax.Individual?.TitleName
                        }
                    });
                }
            }

            if (pax.IdentityDoc != null)
            {
                identityDocuments = pax.IdentityDoc;
                
                foreach (var identityDoc in identityDocuments)
                {
                    var countryOfResidence = identityDoc.ResidenceCountryCode;

                    if (identityDoc.IdentityDocTypeCode != null)
                    {
                        travelDocuments.Add(new TravelDocument
                        {
                            DateOfBirth = identityDoc.BirthdateSpecified ? identityDoc.Birthdate.ToShortDateString() : null,
                            Gender = identityDoc.GenderCodeSpecified ? identityDoc.GenderCode.ToString() : null,
                            CountryOfResidence = countryOfResidence,
                            ExpirationDate = identityDoc.ExpiryDateSpecified ? identityDoc.ExpiryDate.ToShortDateString() : null,
                            PersonName = new PersonName()
                            {
                                FirstName = identityDoc.GivenName?.First().ToUpper() ?? customerDetails.First().FirstName,
                                LastName = identityDoc.Surname?.ToUpper() ?? customerDetails.First().LastName,
                            },
                            DocumentNumber = identityDoc.IdentityDocID,
                            DocumentType = identityDoc.IdentityDocTypeCode,
                            IssueDate = identityDoc.IssueDateSpecified ? identityDoc.IssueDate.ToShortDateString() : null,
                            CountryOfIssue = identityDoc.IssuingCountryCode,
                            Nationality = countryOfResidence
                        });
                    }

                    if (identityDoc.Visa != null)
                    {
                        foreach (VisaType visa in identityDoc.Visa)
                        {
                            travelDocuments.Add(new TravelDocument
                            {
                                ExpirationDate = visa.ExpiryDateSpecified ? visa.ExpiryDate.ToShortDateString() : null,
                                CountryOfResidence = (visa?.HostCountryCode) ?? countryOfResidence,
                                Nationality = countryOfResidence,
                                CountryOfIssue = visa?.IssuingCountryCode,
                                IssueDate = visa.IssueDateSpecified ? visa.IssueDate.ToShortDateString() : null,
                                DocumentNumber = visa.VisaID,
                                DocumentType = Constants.Application.VI,
                                PersonName = new PersonName
                                {
                                    FirstName = identityDoc.GivenName?.First().ToUpper() ?? customerDetails.First().FirstName,
                                    LastName = identityDoc.Surname?.ToUpper() ?? customerDetails.First().FirstName,
                                },
                            });
                        }
                    }
                }
            }
            return travelDocuments;
        }

        public static ManageCustomerRepoCommand MapOrderChangeHoldBookingRequestForGenderDOB(IATA_OrderChangeRQ orderChangeRq, OrderDocument orderDoc)
        {
            return new ManageCustomerRepoCommand
            {
                ManageCustomerRequest = new ManageCustomerRequest()
                {
                    RLOC = orderChangeRq.Request.BookingRef.First().BookingID,
                    CustomerDetails = GetCustomerDetailsForGenderDOB(orderChangeRq, orderDoc),
                    Pos = orderDoc.PointOfSale,
                    Parties = orderDoc.Parties,
                    SegmentDetails = orderDoc.DataList.SegmentDetails
                }
            };
        }

        private static IEnumerable<Customer> GetCustomerDetails(IATA_OrderChangeRQ orderChangeRQ, OrderDocument orderDoc, bool isGstValidationFailed)
        {
            return orderChangeRQ.Request.DataLists.PaxList.Select(pax =>
            {
                var orderDocCustomer = orderDoc.DataList.CustomerDetails.First(x => x.CustId == pax.PaxID);

                return new Customer()
                {
                    CustId = pax.PaxID,
                    Ptc = pax.PTC,
                    FirstName = pax.Individual.GivenName.FirstOrDefault() ?? orderDocCustomer.FirstName,
                    LastName = pax.Individual.Surname ?? orderDocCustomer.LastName,
                    DateOfBirth = pax.BirthdateSpecified ? pax.Birthdate.ToString("yyyy-MM-dd") : orderDocCustomer.DateOfBirth,
                    Gender = pax.Individual.GenderCodeSpecified ? Enum.GetName(typeof(GenderCodeType), pax.Individual.GenderCode) : orderDocCustomer.Gender,
                    FirstNameNumber = orderDocCustomer.FirstNameNumber.TrimStart('0'),
                    LastNameNumber = orderDocCustomer.LastNameNumber.TrimStart('0'),
                    Contacts = GetContactInfo(pax, orderChangeRQ),
                    Addresses = GetAddressList(pax, orderChangeRQ),
                    Employer = pax.Employer != null && !isGstValidationFailed ? GetEmployerDetails(pax, orderChangeRQ) : null,
                    LoyaltyMemberDetails = pax.LoyaltyProgramAccount != null ? GetLoyaltyMemberDetails(pax, orderChangeRQ) : null,
                    TravelDocuments = GetTravelDocuments(pax, orderDoc)
                };
            });
        }

        private static IEnumerable<Customer> GetCustomerDetailsForGenderDOB(IATA_OrderChangeRQ orderChangeRQ, OrderDocument orderDoc)
        {
            return orderChangeRQ.Request.DataLists.PaxList.Select(pax =>
            {
                var orderDocCustomer = orderDoc.DataList.CustomerDetails.First(x => x.CustId == pax.PaxID);
                if (orderDocCustomer.DateOfBirth.IsNullOrEmpty() || orderDocCustomer.Gender.IsNullOrEmpty() || Convert.ToDateTime(orderDocCustomer.DateOfBirth) != pax.Individual?.Birthdate || orderDocCustomer.Gender != Convert.ToString(pax.Individual?.GenderCode))
                {

                    return new Customer()
                    {
                        CustId = pax.PaxID,
                        Ptc = pax.PTC,
                        FirstName = pax.Individual.GivenName.FirstOrDefault() ?? orderDocCustomer.FirstName,
                        LastName = pax.Individual.Surname ?? orderDocCustomer.LastName,
                        DateOfBirth = pax.Individual.Birthdate.ToString("yyyy-MM-dd"),
                        Gender = Enum.GetName(typeof(GenderCodeType), pax.Individual.GenderCode),
                        FirstNameNumber = orderDocCustomer.FirstNameNumber.TrimStart('0'),
                        LastNameNumber = orderDocCustomer.LastNameNumber.TrimStart('0')
                    };
                }
                else return null;
            }).Where(x => x != null);
        }

        private static Employer GetEmployerDetails(PaxType pax, IATA_OrderChangeRQ orderChangeRQ)
        {

            return new Employer()
            {
                Name = pax.Employer.Name,
                TaxDeductedAtSourceCapabilityInd = pax.Employer.TaxDeductedatSourceCapabilityInd,
                SalesTaxRegistration = new SalesTaxRegistration
                {
                    SalesTaxRegistrationId = pax.Employer.SalesTaxRegistration.First().SalesTaxRegistrationID,
                    Contact = new()
                    {
                        Email = pax.Employer.SalesTaxRegistration.First().EmailAddressText,
                        Phone = pax.Employer.SalesTaxRegistration.First().PhoneNumber.First(),
                        Countrycode = pax.Employer.SalesTaxRegistration.First().CountryDialingCode,
                    },
                    Address = new()
                    {
                        City = pax.Employer.SalesTaxRegistration.First().CityName,
                        PostalCode = pax.Employer.SalesTaxRegistration.First().PostalCode,
                        Country = pax.Employer.SalesTaxRegistration.First().CountryCode,
                        AddressLine1 = pax.Employer.SalesTaxRegistration.First().StreetText.First(),
                    },

                }
            };
        }

        private static List<TravelDocument> GetTravelDocuments(PaxType pax, OrderDocument orderDoc)
        {
            IdentityDocType[] identityDocuments = [];
            List<TravelDocument> travelDocuments = [];

            if (pax.RedressCase != null)
            {
                foreach (RedressCaseType redressCase in pax.RedressCase)
                {
                    travelDocuments.Add(new TravelDocument
                    {
                        DocumentType = Constants.Application.AEA,
                        DocumentNumber = redressCase.RedressCaseID,
                        CountryOfIssue = redressCase.CountryCode,
                        PersonName = new PersonName
                        {
                            FirstName = pax.Individual?.GivenName?.FirstOrDefault(),
                            MiddleName = pax.Individual?.MiddleName?.FirstOrDefault(),
                            LastName = pax.Individual?.Surname,
                            Title = pax.Individual?.TitleName
                        }
                    });
                }
            }

            if (pax.IdentityDoc != null)
            {
                var orderDocCust = orderDoc.DataList.CustomerDetails.First(c => c.CustId == pax.PaxID);

                if (orderDocCust.TravelDocuments != null && orderDocCust.TravelDocuments.Any())
                    identityDocuments = pax.IdentityDoc.Where(x => !orderDocCust.TravelDocuments.Any(td => td.DocumentType == x.IdentityDocTypeCode)).ToArray();
                else identityDocuments = pax.IdentityDoc;


                foreach (var identityDoc in identityDocuments)
                {
                    var countryOfResidence = identityDoc.ResidenceCountryCode;

                    if (identityDoc.IdentityDocTypeCode != null)
                    {
                        travelDocuments.Add(new TravelDocument
                        {
                            DateOfBirth = identityDoc.BirthdateSpecified ? identityDoc.Birthdate.ToShortDateString() : null,
                            Gender = identityDoc.GenderCodeSpecified ? identityDoc.GenderCode.ToString() : null,
                            CountryOfResidence = countryOfResidence,
                            ExpirationDate = identityDoc.ExpiryDateSpecified ? identityDoc.ExpiryDate.ToShortDateString() : null,
                            PersonName = new PersonName()
                            {
                                FirstName = identityDoc.GivenName?.First().ToUpper(),
                                LastName = identityDoc.Surname?.ToUpper(),
                            },
                            DocumentNumber = identityDoc.IdentityDocID,
                            DocumentType = identityDoc.IdentityDocTypeCode,
                            IssueDate = identityDoc.IssueDateSpecified ? identityDoc.IssueDate.ToShortDateString() : null,
                            CountryOfIssue = identityDoc.IssuingCountryCode,
                            Nationality = countryOfResidence
                        });
                    }

                    if (identityDoc.Visa != null)
                    {
                        foreach (VisaType visa in identityDoc.Visa)
                        {
                            travelDocuments.Add(new TravelDocument
                            {
                                ExpirationDate = visa.ExpiryDateSpecified ? visa.ExpiryDate.ToShortDateString() : null,
                                CountryOfResidence = (visa?.HostCountryCode) ?? countryOfResidence,
                                Nationality = countryOfResidence,
                                CountryOfIssue = visa?.IssuingCountryCode,
                                IssueDate = visa.IssueDateSpecified ? visa.IssueDate.ToShortDateString() : null,
                                DocumentNumber = visa.VisaID,
                                DocumentType = Constants.Application.VI,
                                PersonName = new PersonName
                                {
                                    FirstName = identityDoc.GivenName?.First().ToUpper(),
                                    LastName = identityDoc.Surname?.ToUpper(),
                                },
                            });
                        }
                    }
                }
            }
            return travelDocuments;
        }

        private static LoyaltyMemberDetails GetLoyaltyMemberDetails(PaxType pax, IATA_OrderChangeRQ orderChangeRQ)
        {
            return new LoyaltyMemberDetails()
            {
                MemberNumber = pax.LoyaltyProgramAccount?.First().AccountNumber,
                MemberTierCode = pax.LoyaltyProgramAccount?.First().TierCode,
                ProgramCode = pax.LoyaltyProgramAccount?.First().LoyaltyProgram.ProgramCode,
                ProgramName = pax.LoyaltyProgramAccount?.First().LoyaltyProgram.ProviderName,
            };
        }

        private static Address GetAddressList(PaxType pax, IATA_OrderChangeRQ orderChangeRQ)
        {
            var addressDetails = orderChangeRQ.Request.DataLists.ContactInfoList?.FirstOrDefault(c => c.ContactInfoID == pax.ContactInfoRefID)?.PostalAddress?.FirstOrDefault();

            return addressDetails == null ? null : new Address()
            {
                City = addressDetails.CityName,
                DepartureCity = addressDetails.CountryName,
                PostalCode = addressDetails.PostalCode,
                Country = addressDetails.CountryCode,
                Region = addressDetails.CountrySubDivisionName,
                AddressType = addressDetails.ContactTypeText,
                AddressLine1 = addressDetails.StreetText?.FirstOrDefault(),
                AddressLine2 = addressDetails.StreetText?.Length == 2 ? addressDetails.StreetText?.Last() : null,
                AddressLine3 = addressDetails.PO_BoxCode
            };
        }

        private static Contacts GetContactInfo(PaxType pax, IATA_OrderChangeRQ orderChangeRQ)
        {
            var contactInfo = orderChangeRQ.Request.DataLists.ContactInfoList?.FirstOrDefault(x => x.ContactInfoID == pax.ContactInfoRefID);

            return contactInfo == null ? null : new Contacts()
            {
                Countrycode = contactInfo?.Phone?.First().CountryDialingCode,
                PhoneAreaNumber = contactInfo?.Phone?.First().AreaCodeNumber,
                Phone = contactInfo?.Phone?.First().PhoneNumber,
                Email = contactInfo?.EmailAddress?.First().EmailAddressText
            };
        }

        private static IEnumerable<OrderInstructions> GetOrderInstructions(IATA_OrderChangeRQ orderChangeRq, OrderDocument orderDoc, bool isGSTValidationFailed, FlightOfferServiceRS flightServiceResponse)
        {
            var orderInstructions = new List<OrderInstructions>();
            int orderInstructionIdCounter = 0;

            //Map GST details
            if (!isGSTValidationFailed && orderChangeRq.Request.DataLists.PaxList.Any(x => x.Employer != null))
            {
                var employerDetails = orderChangeRq.Request.DataLists.PaxList?.FirstOrDefault(x => x.Employer != null)?.Employer;
                var salesTaxRegistrationDetails = employerDetails.SalesTaxRegistration.FirstOrDefault();

                orderInstructions.AddRange(new[]
                {
                    new OrderInstructions
                    {
                        InstructionId = $"{Constants.Application.OrderInstructionId}{++orderInstructionIdCounter}",
                        ItemType = OrderInstructions.ItemTypes.SSREnum,
                        ItemCode = "GSTN",
                        ItemText = $"{salesTaxRegistrationDetails?.CountryCode}/{salesTaxRegistrationDetails?.SalesTaxRegistrationID}/{employerDetails.Name}"
                    },
                    new OrderInstructions
                    {
                        InstructionId = $"{Constants.Application.OrderInstructionId}{++orderInstructionIdCounter}",
                        ItemType = OrderInstructions.ItemTypes.SSREnum,
                        ItemCode = "GSTA",
                        ItemText = $"{salesTaxRegistrationDetails?.CountryCode}/{salesTaxRegistrationDetails?.StreetText.FirstOrDefault()}/{salesTaxRegistrationDetails?.StreetText.LastOrDefault()}/{salesTaxRegistrationDetails?.CityName}//{salesTaxRegistrationDetails.PostalCode}"
                    },
                    new OrderInstructions
                    {
                        InstructionId = $"{Constants.Application.OrderInstructionId}{++orderInstructionIdCounter}",
                        ItemType = OrderInstructions.ItemTypes.SSREnum,
                        ItemCode = "GSTP",
                        ItemText = $"{salesTaxRegistrationDetails?.CountryCode}/{salesTaxRegistrationDetails?.CountryDialingCode}{(salesTaxRegistrationDetails?.PhoneNumber.Length > 1 ? salesTaxRegistrationDetails?.PhoneNumber.FirstOrDefault() +"/"+ salesTaxRegistrationDetails?.PhoneNumber.LastOrDefault() : salesTaxRegistrationDetails?.PhoneNumber.FirstOrDefault() )}"
                    },
                    new OrderInstructions
                    {
                        InstructionId = $"{Constants.Application.OrderInstructionId}{++orderInstructionIdCounter}",
                        ItemType = OrderInstructions.ItemTypes.SSREnum,
                        ItemCode = "GSTE",
                        ItemText = $"{salesTaxRegistrationDetails?.CountryCode}/{salesTaxRegistrationDetails?.EmailAddressText?.Replace("@", "//")?.Replace("_", "..")?.Replace("-", "./")}"
                    }
                });
            }

            //To do :Map SSR details
            if (flightServiceResponse?.ServiceListResponse is not null)
            {
                orderInstructions.AddRange(GetFlightServiceItems(orderChangeRq, flightServiceResponse, orderInstructionIdCounter));
            }

            return orderInstructions;
        }

        private static IEnumerable<OrderInstructions> GetFlightServiceItems(IATA_OrderChangeRQ orderCreateRQ, FlightOfferServiceRS flightServiceListResponse, int orderInstructionIdCounter)
        {
            var fligthServiceOfferItems = ((orderCreateRQ.Request.ChangeOrderChoice.Item) as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedALaCarteOfferItem != null))?.SelectedOfferItem;

            return fligthServiceOfferItems?.SelectMany(fltServiceOfferItem =>
            {
                var ssrDetails = flightServiceListResponse.ServiceListResponse.Offers.SelectMany(x => x.OfferItems).FirstOrDefault(y => y.OfferItemId == fltServiceOfferItem.OfferItemRefID);

                return fltServiceOfferItem.PaxRefID.SelectMany(paxRefId =>
                {
                    var paxSegments = ((fltServiceOfferItem.SelectedALaCarteOfferItem.OfferFlightAssociations.Item) as PaxSegmentRefListType)?.PaxSegmentRefID;

                    return paxSegments.Select(paxSeg =>
                       new OrderInstructions()
                       {
                           InstructionId = $"{Constants.Application.OrderInstructionId}{++orderInstructionIdCounter}",
                           ItemType = OrderInstructions.ItemTypes.SSREnum,
                           ItemCode = ssrDetails?.SSRCode,
                           ItemText = $"{Constants.Application.FlightService}/{paxRefId}/{paxSeg}/{ssrDetails?.Classification}/{ssrDetails?.Description}",
                       });

                });

            });
        }
    }
}

