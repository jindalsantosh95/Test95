using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS
{
    public static class OrderMSRequestMapper
    {
        public static OrderMSCommand MapOrderMSRequest(IATA_OrderCreateRQ orderCreateRq, OfferResponse offerDoc, bool isGSTDetailsValid, Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse)
        {
            offerDoc.Parties.Participants.First(x => x.PartyType == Model.Participant.PartyTypeEnum.Seller).Contacts = new Contacts()
            {
                Email = orderCreateRq.DistributionChain.First().ContactInfo.EmailAddress.First().EmailAddressText
            };

            return new OrderMSCommand()
            {
                Pos = offerDoc.Pos,
                Parties = new()
                {
                    Participants = offerDoc.Parties.Participants.OrderBy(x => x.Ordinal)
                },
                FareTypeRequest = new()
                {
                    FareTypeCode = offerDoc?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = offerDoc?.FareTypeRequest?.FareSubTypeCode
                },
                ChangeType = ChangeTypeEnum.OrderItemAdded,
                DataList = GetDataList(orderCreateRq, offerDoc.DataList, offerDoc.Offers, isGSTDetailsValid, flightServiceListResponse),
                SelectedOffers = GetSelectedOffer(offerDoc.Offers, offerDoc.DataList),
                TimeLimits = orderCreateRq.Request.PaymentFunctions == null ? GetTicketingTimeLimit(offerDoc.Offers) : null
            };
        }

        private static IEnumerable<TimeLimit> GetTicketingTimeLimit(IEnumerable<OfferDetails> offers)
        {
            return offers.Where(x => x.TicketingTimeLimit != null).Select(offer => new TimeLimit()
            {
                Limit = offer.TicketingTimeLimit,
                Type = TimeLimit.Types.TicketingTimeLimit
            });
        }

        private static IEnumerable<Offers> GetSelectedOffer(IEnumerable<OfferDetails> offers, Model.OfferRS.DataList dataList)
        {
            return offers.Select(offer =>
                new Offers()
                {
                    OfferId = offer.OfferId,
                    OfferItems = offer.OfferItems.OrderBy(x => int.Parse(x.OfferItemId.Split('-').Last())).Select(x =>
                    {
                        return new OfferItem()
                        {
                            OfferItemId = x.OfferItemId,
                            OfferitemPricing = x.OfferitemPricing,
                            TicketDesigCode = x.TicketDesigCode,
                            Services = GetServices(x.Services, dataList)
                        };
                    })
                }
            );
        }

        private static IEnumerable<OfferService> GetServices(IEnumerable<OfferService> services, Model.OfferRS.DataList dataList)
        {
            foreach (var service in services)
            {
                service.FlightSegmentsRefIds = service.ProductSegmentAssociation.OfferSegmentRefIds.Select(sv => dataList.OfferSegments.Where(x => x.OfferSegmentId == sv).Select(y => y.FlightSegmentRefId.Replace("F", ""))).SelectMany(x => x);
            }

            return services;
        }

        private static Model.OrderMS.DataList GetDataList(IATA_OrderCreateRQ orderCreateRq, Model.OfferRS.DataList dataList, IEnumerable<OfferDetails> offers, bool isGSTDetailsValid, Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse)
        {
            var segServiceDefinitionIds = new Dictionary<string, string>();

            return new Model.OrderMS.DataList()
            {
                BaggageAllowances = dataList.BaggageAllowances,
                CustomerDetails = GetCustomerDetails(orderCreateRq, dataList.CustomerDetails, offers, isGSTDetailsValid),
                Journey = GetJourney(dataList.Journey),
                OriginDestinationAssociations = dataList.OriginDestinationAssociations,
                Penalty = dataList.Penalty,
                PriceClass = dataList.PriceClass,
                ServiceDefinitions = GetServiceDefinitions(dataList.ProductComponents, dataList.OfferSegments, segServiceDefinitionIds),
                SegmentDetails = GetSegmentDetails(dataList.FlightSegments, dataList.OfferSegments, segServiceDefinitionIds),
                OrderInstructions = GetOrderInstructions(orderCreateRq, isGSTDetailsValid, flightServiceListResponse)

            };
        }

        private static IEnumerable<OrderInstructions> GetOrderInstructions(IATA_OrderCreateRQ orderCreateRQ, bool isGSTDetailsValid, Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse)
        {
            var orderInstructions = new List<OrderInstructions>();
            int orderInstructionIdCounter = 0;

            //Map GST details
            if (isGSTDetailsValid && orderCreateRQ.Request.DataLists.PaxList.Any(x => x.Employer != null))
            {
                var employerDetails = orderCreateRQ.Request.DataLists.PaxList?.FirstOrDefault(x => x.Employer != null)?.Employer;
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
            if (flightServiceListResponse?.ServiceListResponse is not null)
            {
                orderInstructions.AddRange(GetFlightServiceItems(orderCreateRQ, flightServiceListResponse, orderInstructionIdCounter));
            }

            return orderInstructions;
        }

        private static IEnumerable<OrderInstructions> GetFlightServiceItems(IATA_OrderCreateRQ orderCreateRQ, Model.FlightServiceMS.FlightOfferServiceRS flightServiceListResponse, int orderInstructionIdCounter)
        {
            var flightServiceOfferItems = ((orderCreateRQ.Request.CreateOrder.Item) as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.FirstOrDefault(x => x.SelectedOfferItem.Any(y => y.SelectedALaCarteOfferItem != null))?.SelectedOfferItem;

            return flightServiceOfferItems?.SelectMany(fltServiceOfferItem =>
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

        private static IEnumerable<Journey> GetJourney(IEnumerable<Journey> journeys)
        {
            foreach (var journey in journeys) journey.FlightSegmentRefIds = journey.FlightSegmentRefIds.Select(x => x.Replace("F", ""));
            return journeys;
        }

        private static IEnumerable<ServiceDefinition> GetServiceDefinitions(IEnumerable<ProductComponent> productComponents, IEnumerable<OfferSegment> offerSegments, Dictionary<string, string> segServiceDefinitionId)
        {
            productComponents ??= Enumerable.Empty<ProductComponent>();
            offerSegments ??= Enumerable.Empty<OfferSegment>();

            var result = offerSegments.Select((seg, index) =>
            {
                if (string.IsNullOrWhiteSpace(seg.FlightSegmentRefId))
                    return (serviceDefinitions: null, segServicedefinitionIds: default(KeyValuePair<string, string>));


                var descriptions = productComponents
                    .Where(x => seg.ProductComponentRefIds?.Contains(x.Id) == true)
                    .Select(y => y.Description)
                    .ToList();

                if (!descriptions.Any())
                    return (serviceDefinitions: null, segServicedefinitionIds: default(KeyValuePair<string, string>));

                var svID = $"SV{++index}";

                return (
                    serviceDefinitions: new ServiceDefinition
                    {
                        Description = descriptions,
                        ServiceDefinitionId = svID
                    },
                    segServicedefinitionIds: new KeyValuePair<string, string>(
                        seg.FlightSegmentRefId.Replace("F", ""),
                        svID)
                );
            }).ToList();

            foreach (var kv in result.Select(x => x.segServicedefinitionIds))
            {
                if (!kv.Equals(default(KeyValuePair<string, string>)))
                    segServiceDefinitionId.TryAdd(kv.Key, kv.Value);
            }

            return result.Where(x => x.serviceDefinitions != null).Select(x => x.serviceDefinitions);
        }

        private static IEnumerable<Segment> GetSegmentDetails(IEnumerable<Segment> flightSegments, IEnumerable<OfferSegment> offerSegments, Dictionary<string, string> segServiceDefinitionIds)
        {
            foreach (var oseg in offerSegments)
            {
                var flightSegment = flightSegments.FirstOrDefault(fseg => fseg.SegmentId.Equals(oseg.FlightSegmentRefId));

                flightSegment.CabinType = oseg?.CabinType;
                flightSegment.ClassOfService = oseg?.ClassOfService;
                flightSegment.ServiceDefinitionsRefID = segServiceDefinitionIds.FirstOrDefault(ss => ss.Key == oseg.FlightSegmentRefId.Replace("F", "")).Value;
                flightSegment.SegmentId = flightSegment.SegmentId.Replace("F", "");
                flightSegment.TicketNotValidAfter = oseg.TicketNotValidAfter;
                flightSegment.TicketNotValidBefore = oseg.TicketNotValidBefore;
            }

            return flightSegments;
        }

        private static IEnumerable<Customer> GetCustomerDetails(IATA_OrderCreateRQ orderCreateRq, IEnumerable<Customer> customerDetails, IEnumerable<OfferDetails> offerItems, bool isGSTDetailsValid)
        {
            var customers = new List<Customer>();
            int index = 1;

            var sortedOfferItems = offerItems.First().OfferItems.OrderBy(x => int.Parse(x.OfferItemId.Split('-').Last()));
            var orderedPaxList = sortedOfferItems.SelectMany(x => x.Services.First().ServiceBeneficiaries).Select(paxId => orderCreateRq.Request.DataLists.PaxList.FirstOrDefault(p => p.PaxID == paxId)).Where(p => p != null);

            foreach (var pax in orderedPaxList)
            {
                var matchedCustomers = customerDetails.Where(x => x.CustId == pax.PaxID);
                var itaPtcs = matchedCustomers.Any(c => c.ItaPtc == null) ? null : string.Join(",", matchedCustomers.Select(c => c.ItaPtc));

                customers.Add(new()
                {
                    CustId = pax.PaxID,
                    Ptc = pax.PTC,
                    ItaPtc = itaPtcs,
                    FirstName = pax.Individual.GivenName.FirstOrDefault(),
                    LastName = pax.Individual.Surname.ToString(),
                    DateOfBirth = pax.Individual.BirthdateSpecified ? pax.Individual.Birthdate.ToString("yyyy-MM-dd") : null,
                    Gender = pax.Individual.GenderCodeSpecified ? Enum.GetName(typeof(GenderCodeType), pax.Individual.GenderCode) : null,
                    Contacts = GetContactInfo(pax, orderCreateRq),
                    Employer = (isGSTDetailsValid && pax.Employer != null) ? GetEmployerDetails(pax.Employer) : null,
                    FirstNameNumber = "01",
                    LastNameNumber = index++.ToString().PadLeft(2, '0'),
                    TravelDocuments = GetTravelDocuments(pax),
                    LoyaltyMemberDetails = pax.LoyaltyProgramAccount != null && pax.LoyaltyProgramAccount.Any() ? GetLoyaltyMemberDetails(pax.LoyaltyProgramAccount.First()) : null,
                    Addresses = GetAddressList(pax, orderCreateRq)
                });
            }

            return customers;
        }

        private static Address GetAddressList(PaxType pax, IATA_OrderCreateRQ request)
        {
            var addressDetails = request.Request.DataLists.ContactInfoList.FirstOrDefault(c => c.ContactInfoID == pax.ContactInfoRefID)?.PostalAddress?.FirstOrDefault();

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

        private static LoyaltyMemberDetails GetLoyaltyMemberDetails(LoyaltyProgramAccountType program)
        {
            return new LoyaltyMemberDetails()
            {
                MemberNumber = program.AccountNumber,
                ProgramCode = program.LoyaltyProgram.ProgramCode,
                ProgramName = program.LoyaltyProgram.ProgramName
            };
        }

        private static Employer GetEmployerDetails(EmployerType employer)
        {
            var gstInfo = employer?.SalesTaxRegistration?.FirstOrDefault();

            return new Employer()
            {
                Name = employer.Name,
                SalesTaxRegistration = new SalesTaxRegistration()
                {
                    Address = new Address()
                    {
                        AddressLine1 = gstInfo?.StreetText.FirstOrDefault(),
                        AddressLine2 = gstInfo?.StreetText.LastOrDefault(),
                        City = gstInfo?.CityName,
                        Country = gstInfo?.CountryCode,
                        PostalCode = gstInfo?.PostalCode

                    },
                    Contact = new()
                    {
                        Email = gstInfo?.EmailAddressText,
                        Countrycode = gstInfo?.CountryDialingCode,
                        Phone = gstInfo?.PhoneNumber.Length > 1 ? $"{gstInfo?.PhoneNumber?.FirstOrDefault()}/{gstInfo?.PhoneNumber?.LastOrDefault()}" : gstInfo?.PhoneNumber?.FirstOrDefault()
                    },
                    SalesTaxRegistrationId = gstInfo.SalesTaxRegistrationID
                },
                TaxDeductedAtSourceCapabilityInd = employer.TaxDeductedatSourceCapabilityInd
            };
        }

        private static Contacts GetContactInfo(PaxType pax, IATA_OrderCreateRQ orderCreateRq)
        {
            var contactInfo = orderCreateRq.Request.DataLists.ContactInfoList.FirstOrDefault(x => x.ContactInfoID == pax.ContactInfoRefID);

            if (contactInfo != null)
            {
                return new Contacts()
                {
                    CountryDailingCode = contactInfo?.Phone?.FirstOrDefault()?.CountryDialingCode,
                    PhoneAreaNumber = contactInfo?.Phone?.FirstOrDefault()?.AreaCodeNumber,
                    Phone = contactInfo?.Phone?.FirstOrDefault()?.PhoneNumber,
                    Email = contactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText,
                    PhoneType = contactInfo?.Phone?.FirstOrDefault()?.ContactTypeText,
                    ContactRefusedInd = contactInfo.ContactRefusedInd,
                };
            }

            return null;
        }

        private static List<TravelDocument> GetTravelDocuments(PaxType pax)
        {
            var travelDocuments = new List<TravelDocument>();

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
                foreach (var identityDoc in pax.IdentityDoc)
                {
                    var countryOfResidence = identityDoc.ResidenceCountryCode;

                    travelDocuments.Add(new TravelDocument
                    {
                        DateOfBirth = identityDoc.BirthdateSpecified ? identityDoc.Birthdate.ToShortDateString() : null,
                        Gender = identityDoc.GenderCodeSpecified ? identityDoc.GenderCode.ToString() : null,
                        CountryOfResidence = countryOfResidence,
                        ExpirationDate = identityDoc.ExpiryDateSpecified ? identityDoc.ExpiryDate.ToShortDateString() : null,
                        PersonName = new PersonName
                        {
                            FirstName = identityDoc.GivenName?.First(),
                            MiddleName = identityDoc.MiddleName?.First(),
                            LastName = identityDoc.Surname,
                            Title = identityDoc.TitleName
                        },
                        DocumentNumber = identityDoc.IdentityDocID,
                        DocumentType = identityDoc.IdentityDocTypeCode,
                        IssueDate = identityDoc.IssueDateSpecified ? identityDoc.IssueDate.ToShortDateString() : null,
                        CountryOfIssue = identityDoc.IssuingCountryCode,
                        Nationality = string.IsNullOrWhiteSpace(identityDoc.CitizenshipCountryCode)? countryOfResidence : identityDoc.CitizenshipCountryCode
                    });

                    if (identityDoc.Visa != null)
                    {
                        foreach (VisaType visa in identityDoc.Visa)
                        {
                            travelDocuments.Add(new TravelDocument
                            {
                                ExpirationDate = visa.ExpiryDateSpecified ? visa.ExpiryDate.ToShortDateString() : null,
                                CountryOfResidence = visa.HostCountryCode ?? countryOfResidence,
                                Nationality = countryOfResidence,
                                CountryOfIssue = visa.IssuingCountryCode,
                                IssueDate = visa.IssueDateSpecified ? visa.IssueDate.ToShortDateString() : null,
                                DocumentNumber = visa.VisaID,
                                DocumentType = Constants.Application.VI,
                                PersonName = new PersonName
                                {
                                    FirstName = identityDoc.GivenName?.First(),
                                    MiddleName = identityDoc.MiddleName?.First(),
                                    LastName = identityDoc.Surname,
                                    Title = identityDoc.TitleName
                                },
                            });
                        }
                    }
                }
            }
            return travelDocuments;
        }

    }
}
