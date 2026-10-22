using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Mapper
{
    public static class CreatePassengerNameRecordRQMapper
    {
        public static CreatePassengerNameRecordRQ CreatePnrReqMapper(OrderCreateRequest orderCreateRequest)
        {
            var travelItineraryAddInfo = TravelItineraryAddInfoMapper(orderCreateRequest.DataList.CustomerDetails, orderCreateRequest.Parties, orderCreateRequest.TimeLimits);
            var agentEmail = orderCreateRequest?.Parties?.Participants?.First(x => x.PartyType == Participant.PartyTypeEnum.Seller)?.Contacts?.Email;
            var osiData = orderCreateRequest.Parties.Participants.Select(x => "OD" + x.Ordinal + "/" + x.RequestAgentType + "/" + x.AgencyID + "/" + x.Name);
            
            return new()
            {
                AirBook = AirBookMapper(orderCreateRequest.DataList.SegmentDetails, orderCreateRequest.DataList.CustomerDetails.Count(x => x.Ptc != Constants.Application.Infant)),
                TravelItineraryAddInfo = travelItineraryAddInfo,
                SpecialReqDetails = orderCreateRequest.Parties.Participants != null || orderCreateRequest.DataList.OrderInstructions != null || orderCreateRequest.DataList.CustomerDetails.Any(x => x.TravelDocuments != null || !string.IsNullOrEmpty(x.DateOfBirth) || !string.IsNullOrEmpty(x.Gender)) || orderCreateRequest.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.Child || x.Ptc == Constants.Application.Infant) ? SpecialReqDetailsMapper(orderCreateRequest.DataList.OrderInstructions, orderCreateRequest.DataList.CustomerDetails.Where(x => x.TravelDocuments != null).SelectMany(x => x.TravelDocuments), orderCreateRequest.DataList.CustomerDetails, travelItineraryAddInfo.CustomerInfo.PersonName, agentEmail, osiData, orderCreateRequest.Parties?.CorporateAccount, IsItxFare(orderCreateRequest)) : null,
                PostProcessing = PostProcessingMapper(orderCreateRequest.DataList.SegmentDetails),
                version = "2.5.0"
            };
        }

        private static CreatePassengerNameRecordRQPostProcessing PostProcessingMapper(IEnumerable<Segment> segments)
        {
            return new()
            {
                EndTransaction = new()
                {
                    Source = new()
                    {
                        ReceivedFrom = Constants.Application.SabreSource
                    }
                },
                ARUNK = segments.Any(x => segments.Last() != x && x.DestinationAirportCode != segments.ElementAt(segments.ToList().FindIndex(y => y == x) + 1).OriginAirportCode) ? new() : null
            };
        }


        private static CreatePassengerNameRecordRQSpecialReqDetails SpecialReqDetailsMapper(IEnumerable<OrderInstructions> orderInstructions, IEnumerable<TravelDocument> travelDocuments,IEnumerable<Customer> customerDetails, IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoPersonName> personNames, string agentEmail, IEnumerable<string> osiData, CorporateAccount corporateAccount, bool isItxFare)
        {
            // Get all SSR entries (CTCE, OSI, etc.)
            var ssrs = !string.IsNullOrEmpty(agentEmail) || customerDetails.Any(x => x.Ptc != Constants.Application.Infant && x.Contacts != null) || orderInstructions != null || customerDetails.Any(x => x.Ptc == Constants.Application.Infant || x.Ptc == Constants.Application.Child) || isItxFare ? GetSSRs(orderInstructions, customerDetails, personNames, agentEmail, osiData, corporateAccount, isItxFare)?.ToArray() : null;
            
            // Each SSR becomes its own Service entry
            var services = ssrs?.Select(ssr => new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
            {
                Text = ssr?.Text,
                VendorPrefs = ssr?.VendorPrefs,
                SegmentNumber = ssr?.SegmentNumber,
                SSR_Code = ssr?.SSR_Code,
                PersonName = ssr?.PersonName

            }).ToArray();

            return new CreatePassengerNameRecordRQSpecialReqDetails
            {
                SpecialService = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialService
                {
                    SpecialServiceInfo = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfo
                    {
                        Service = services,
                        AdvancePassenger = travelDocuments?.Any() == true ? [.. GetAdvancePassengerInfo(travelDocuments, customerDetails, personNames)] : null,
                        SecureFlight = (travelDocuments?.Any() == true || customerDetails.Any(x => !string.IsNullOrEmpty(x.DateOfBirth) && !string.IsNullOrEmpty(x.Gender))) ? [.. GetSecureFlights(travelDocuments, customerDetails, personNames)] : null
                    }
                }
            };
        }

        private static IEnumerable<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight> GetSecureFlights(IEnumerable<TravelDocument> travelDocuments, IEnumerable<Customer> customerDetails, IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoPersonName> personNames)
        {
            var associatedAdults = new Dictionary<string, string>();
            var adults = personNames.Where(x => x.PassengerType == "ADT").ToList();

            if (personNames.Any(x => x.PassengerType == Constants.Application.Infant))
            {
                foreach (var inf in personNames.Where(x => x.PassengerType == Constants.Application.Infant))
                {
                    associatedAdults.Add(inf.NameNumber, adults.First().NameNumber);
                    adults.RemoveAt(0);
                }
            }

            var secureFlights = customerDetails.Where(customer => customer.DateOfBirth != null && customer.DateOfBirth != DateTime.MinValue.ToString() && !string.IsNullOrEmpty(customer.Gender)).SelectMany(customer =>
            {
                return travelDocuments.Where(doc => doc.PersonName?.FirstName == customer.FirstName && doc.PersonName?.LastName == customer.LastName && (doc.DocumentType == Constants.Application.AEADocType || doc.DocumentType == Constants.Application.RedressDocType || doc.DocumentType == Constants.Application.CRDocType)).Select(doc =>
                {
                    var secureFlight = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight
                    {
                        IssueCountry = doc.CountryOfIssue,
                        PersonName = new()
                        {
                            NameNumber = customer.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customer.LastNameNumber}.{customer.FirstNameNumber}").Value : $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                            LapChild = customer.Ptc == Constants.Application.Infant,
                            LapChildSpecified = customer.Ptc == Constants.Application.Infant
                        }
                    };

                    if (doc.DocumentType == Constants.Application.RedressDocType || doc.DocumentType == Constants.Application.AEADocType)
                    {
                        secureFlight.RedressNumber = doc.DocumentNumber;
                    }
                    else
                    {
                        secureFlight.KnownTravelerNumber = doc.DocumentNumber;
                    }

                    return secureFlight;
                });
            });

            if (customerDetails.Any(x => x.DateOfBirth != null && x.Gender != null))
            {
                var sfpds = customerDetails.Where(x => x.DateOfBirth != null && x.Gender != null).Select(x =>
                {
                    if (x.Ptc == Constants.Application.Infant)
                    {
                        var secureFlight = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight()
                        {
                            SegmentNumber = "A",
                            PersonName = new()
                            {
                                DateOfBirth = x.DateOfBirth,
                                Gender = Enum.Parse<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlightPersonNameGender>($"{x.Gender}I"),
                                GenderSpecified = true,
                                GivenName = x.FirstName,
                                Surname = x.LastName,
                                NameNumber = associatedAdults.First(a => a.Key == $"{x.LastNameNumber}.{x.FirstNameNumber}").Value
                            },
                        };
                        return secureFlight;
                    }
                    else
                    {
                        return new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight()
                        {
                            SegmentNumber = "A",
                            PersonName = new()
                            {
                                DateOfBirth = x.DateOfBirth,
                                Gender = Enum.Parse<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlightPersonNameGender>(x.Gender),
                                GenderSpecified = true,
                                GivenName = x.FirstName,
                                Surname = x.LastName,
                                NameNumber = personNames.First(p => p.GivenName == x.FirstName && p.Surname == x.LastName).NameNumber
                            },

                        };
                    }
                });
                secureFlights = secureFlights.Concat(sfpds);
            }

            return secureFlights;
        }

        private static IEnumerable<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger> GetAdvancePassengerInfo(IEnumerable<TravelDocument> travelDocuments, IEnumerable<Customer> customerDetails, IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoPersonName> personNames)
        {
            var associatedAdults = new Dictionary<string, string>();
            var adults = personNames.Where(x => x.PassengerType == "ADT").ToList();

            if (personNames.Any(x => x.PassengerType == Constants.Application.Infant))
            {
                foreach (var inf in personNames.Where(x => x.PassengerType == Constants.Application.Infant))
                {
                    associatedAdults.Add(inf.NameNumber, adults.First().NameNumber);
                    adults.RemoveAt(0);
                }
            }

            return customerDetails.Where(customer =>  customer.DateOfBirth != null && customer.DateOfBirth != DateTime.MinValue.ToString() && !string.IsNullOrEmpty(customer.Gender)).SelectMany(customer =>
            {
                var docs = travelDocuments.Where(x => (x.PersonName?.FirstName == customer.FirstName && x.PersonName?.LastName == customer.LastName) && (x.DocumentType == Constants.Application.PassportDocType || x.DocumentType == Constants.Application.GreenCardDocType || x.DocumentType == Constants.Application.PTDocType || x.DocumentType == Constants.Application.GCDocType || x.DocumentType == Constants.Application.VIDocType || x.DocumentType == Constants.Application.VisaDocType)).Select(x =>
                {
                    if(x.DocumentType == Constants.Application.PassportDocType || x.DocumentType == Constants.Application.PTDocType)
                    {
                        return new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
                        {
                            Document = new()
                            {
                                Number = x.DocumentNumber,
                                Type = "P",
                                ExpirationDate = Convert.ToDateTime(x.ExpirationDate),
                                ExpirationDateSpecified = true,
                                IssueCountry = x.CountryOfIssue,
                                NationalityCountry = x.Nationality
                            },
                            PersonName = new()
                            {
                                Gender = customer.Ptc == Constants.Application.Infant ? Enum.Parse<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender + "I") : Enum.Parse<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender),
                                GenderSpecified = true,
                                NameNumber = customer.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customer.LastNameNumber}.{customer.FirstNameNumber}").Value : $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                                DateOfBirth = DateTime.Parse(customer.DateOfBirth),
                                DateOfBirthSpecified = true,
                                GivenName = customer.FirstName,
                                Surname = customer.LastName,
                                LapChild = customer.Ptc == Constants.Application.Infant,
                                LapChildSpecified = customer.Ptc == Constants.Application.Infant
                            }
                        };
                    }

                    if (x.DocumentType == Constants.Application.VIDocType || x.DocumentType == Constants.Application.VisaDocType)
                    {
                        return new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
                        {
                            Document = new()
                            {
                                Number = x.DocumentNumber,
                                Type = "V",
                                Visa = new()
                                {
                                    ExpirationDate = Convert.ToDateTime(x.ExpirationDate),
                                    ExpirationDateSpecified = true,
                                    ApplicableCountry = x.CountryOfIssue,
                                    PlaceOfBirth = x.CountryOfResidence,
                                    PlaceOfIssue = x.CountryOfIssue
                                }
                            },
                            PersonName = new()
                            {
                                NameNumber = customer.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customer.LastNameNumber}.{customer.FirstNameNumber}").Value : $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                                LapChild = customer.Ptc == Constants.Application.Infant,
                                LapChildSpecified = customer.Ptc == Constants.Application.Infant
                            }
                        };
                    }

                    if (x.DocumentType == Constants.Application.GreenCardDocType || x.DocumentType == Constants.Application.GCDocType)
                    {
                        return new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
                        {
                            Document = new()
                            {
                                Number = x.DocumentNumber,
                                Type = "C",
                                ExpirationDate = Convert.ToDateTime(x.ExpirationDate),
                                ExpirationDateSpecified = true,
                                IssueCountry = x.CountryOfIssue,
                                NationalityCountry = x.Nationality
                            },
                            PersonName = new()
                            {
                                Gender = customer.Ptc == Constants.Application.Infant ? Enum.Parse<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender + "I") : Enum.Parse<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender),
                                GenderSpecified = true,
                                NameNumber = customer.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customer.LastNameNumber}.{customer.FirstNameNumber}").Value : $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                                DateOfBirth = DateTime.Parse(customer.DateOfBirth),
                                DateOfBirthSpecified = true,
                                GivenName = customer.FirstName,
                                Surname = customer.LastName,
                                LapChild = customer.Ptc == Constants.Application.Infant,
                                LapChildSpecified = customer.Ptc == Constants.Application.Infant
                            }
                        };
                    }

                    return null;

                }).Where(x => x != null);

                if (customer.Addresses != null)
                {
                    docs = docs.Append(new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
                    {
                        ResidentDestinationAddress = new()
                        {
                            Country = customer.Addresses.Country,
                            City = customer.Addresses.City,
                            Type = "D",
                            Street = customer.Addresses.AddressLine1,
                            State = customer.Addresses.Region,
                            Zip = customer.Addresses.PostalCode
                        }
                    });
                }

                return docs;
            });
        }

        private static IEnumerable<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService> GetSSRs(IEnumerable<OrderInstructions> orderInstructions, IEnumerable<Customer> customerDetails, IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoPersonName> personNames, string agentEmail, IEnumerable<string> osiData, CorporateAccount corporateAccount, bool isItxFare)
        {
            var persondata = GetPersonNames(customerDetails);

            var associatedAdults = new Dictionary<string, string>();
            var adults = customerDetails.Where(x => x.Ptc == "ADT").ToList();

            if (customerDetails.Any(x => x.Ptc == Constants.Application.Infant))
            {
                foreach (var inf in customerDetails.Where(x => x.Ptc == Constants.Application.Infant))
                {
                    associatedAdults.Add($"{inf.LastNameNumber}.{inf.FirstNameNumber}", $"{adults.First().LastNameNumber}.{adults.First().FirstNameNumber}");
                    adults.RemoveAt(0);
                }
            }

            var ssrs = orderInstructions?.Select(orderInstruction =>
            {
                //Add GST Details as SSRs
                if (orderInstruction.ItemCode == "GSTA" || orderInstruction.ItemCode == "GSTN" || orderInstruction.ItemCode == "GSTP" || orderInstruction.ItemCode == "GSTE")
                {
                    return new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                    {
                        SSR_Code = orderInstruction.ItemCode,
                        SegmentNumber = "A",
                        PersonName = new()
                        {
                            NameNumber = persondata.First().NameNumber
                        },
                        VendorPrefs = new()
                        {
                            Airline = new()
                            {
                                Hosted = true,
                            }
                        },
                        Text = orderInstruction.ItemText,
                    };
                }

                //Add Flight Service details as SSRs
                if (orderInstruction?.ItemText?.Contains(Constants.Application.FlightService) ?? false)
                {
                    var flightServiceInfo = orderInstruction?.ItemText?.Split('/');
                    var paxId = flightServiceInfo[1];
                    var segmentNumber = flightServiceInfo[2]?.Trim('S', 'E', 'G');
                    var customerDetail = customerDetails.FirstOrDefault(cust => cust.CustId == paxId);

                    return new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                    {
                        SSR_Code = orderInstruction.ItemCode,
                        SegmentNumber = segmentNumber,
                        PersonName = new()
                        {
                            NameNumber = customerDetail.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customerDetail.LastNameNumber}.{customerDetail.FirstNameNumber}").Value : $"{customerDetail.LastNameNumber}.{customerDetail.FirstNameNumber}"
                        },
                        VendorPrefs = new()
                        {
                            Airline = new()
                            {
                                Hosted = true,
                            }
                        }
                    };
                }
                return null;
            }).Where(x => x is not null) ?? Enumerable.Empty<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService>();

            if (personNames.Any(x => x.PassengerType == Constants.Application.Infant || x.PassengerType == Constants.Application.CNN))
                ssrs = ssrs == null ? GetINFTCHDSSRs(personNames, customerDetails) : ssrs.Concat(GetINFTCHDSSRs(personNames, customerDetails));

            ssrs = ssrs.Concat(osiData.Select(x => new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
            {
                SSR_Code = Constants.Application.OsiSSRCode,
                Text = x,
                VendorPrefs = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefs
                {
                    Airline = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefsAirline
                    {
                        Code = Constants.Application.OwnerAirline
                    }
                }
            }));

            if (!string.IsNullOrWhiteSpace(agentEmail))
            {
                ssrs = ssrs.Append(new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                {
                    SSR_Code = Constants.Application.OsiSSRCode,
                    Text = $"CTCE {agentEmail.Replace("@", "//")}",
                    VendorPrefs = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefs
                    {
                        Airline = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefsAirline
                        {
                            Code = Constants.Application.OwnerAirline
                        }
                    }
                });
            }

            if (isItxFare)
            {
                ssrs = ssrs.Append(
                    new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                    {
                        SSR_Code = Constants.Application.OsiSSRCode,
                        Text = Constants.Application.PrivateFareOsiElement,
                        VendorPrefs = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefs
                        {
                            Airline = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefsAirline
                            {
                                Code = Constants.Application.OwnerAirline
                            }
                        }
                    });
            }

            if (!string.IsNullOrWhiteSpace(corporateAccount?.TicketDesignator))
            {
                ssrs = ssrs.Append(
                    new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                    {
                        SSR_Code = Constants.Application.OsiSSRCode,
                        Text = $"{Constants.Application.OsiCorporateCode} {corporateAccount.TicketDesignator}",
                        VendorPrefs = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefs
                        {
                            Airline = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServiceVendorPrefsAirline
                            {
                                Code = Constants.Application.OwnerAirline
                            }
                        }
                    });
            }

            var contactServices = customerDetails
                .Where(x => x.Ptc != Constants.Application.Infant && x.Contacts != null)
                .SelectMany(GetContactServices);

            ssrs = ssrs.Concat(contactServices);

            return ssrs;
        }

        private static IEnumerable<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService> GetContactServices(Customer customer)
        {
            var services = Enumerable.Empty<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService>();
            var nameNumber = $"{customer.LastNameNumber}.{customer.FirstNameNumber}";

            if (!string.IsNullOrWhiteSpace(customer.Contacts?.Email))
            {
                services = services.Append(new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                {
                    SSR_Code = Constants.Application.CtceSSRCode,
                    SegmentNumber = "A",
                    PersonName = new() { NameNumber = nameNumber },
                    Text = customer.Contacts.Email.Replace("@", "//")
                });
            }

            if (!string.IsNullOrWhiteSpace(customer.Contacts?.Phone))
            {
                services = services.Append(new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                {
                    SSR_Code = Constants.Application.CtcmSSRCode,
                    SegmentNumber = "A",
                    PersonName = new() { NameNumber = nameNumber },
                    Text = $"{customer.Contacts.CountryDailingCode}{customer.Contacts.Phone}".Replace("-", string.Empty)
                });
            }

            if (string.IsNullOrWhiteSpace(customer.Contacts?.Email)
                && string.IsNullOrWhiteSpace(customer.Contacts?.Phone)
                && customer.Contacts?.ContactRefusedInd == true)
            {
                services = services.Append(new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                {
                    SSR_Code = Constants.Application.CtcrSSRCode,
                    SegmentNumber = "A",
                    PersonName = new() { NameNumber = nameNumber }
                });
            }

            return services;
        }


        private static IEnumerable<CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService> GetINFTCHDSSRs(IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoPersonName> personNames, IEnumerable<Customer> customerDetails)
        {
            var adults = personNames.Where(x => x.PassengerType == "ADT").ToList();

            return personNames.Where(x => x.PassengerType == Constants.Application.Infant || x.PassengerType == Constants.Application.CNN).Select(x =>
            {
                if (x.PassengerType == Constants.Application.Infant)
                {
                    var ssr = new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService()
                    {
                        SSR_Code = Constants.Application.InfantSSRCode,
                        SegmentNumber = "A",
                        Text = $"{x.Surname}/{x.GivenName}/{DateTime.ParseExact(customerDetails.First(y => y.FirstName == x.GivenName && y.LastName == x.Surname).DateOfBirth, Constants.Application.DateOnlyFormat, CultureInfo.InvariantCulture).ToString(Constants.Application.DateFormat, CultureInfo.InvariantCulture).ToUpperInvariant()}",
                        PersonName = new() { NameNumber = adults.First().NameNumber }
                    };
                    adults.RemoveAt(0);
                    return ssr;
                }
                else
                {
                    return new CreatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService()
                    {
                        SSR_Code = Constants.Application.ChildSSRCode,
                        SegmentNumber = "A",
                        Text = $"{DateTime.ParseExact(customerDetails.First(y => y.FirstName == x.GivenName && y.LastName == x.Surname).DateOfBirth, Constants.Application.DateOnlyFormat, CultureInfo.InvariantCulture).ToString(Constants.Application.DateFormat, CultureInfo.InvariantCulture).ToUpperInvariant()}",
                        PersonName = new() { NameNumber = x.NameNumber }
                    };
                }
            });
        }

        private static CreatePassengerNameRecordRQAirBook AirBookMapper(IEnumerable<Segment> segmentDetails, int paxCount)
        {
            return new()
            {
                HaltOnStatus = new []
                {
                    new CreatePassengerNameRecordRQAirBookHaltOnStatus()
                    {
                        Code = "NO"
                    },
                    new CreatePassengerNameRecordRQAirBookHaltOnStatus()
                    {
                        Code = "NN"
                    },
                    new CreatePassengerNameRecordRQAirBookHaltOnStatus()
                    {
                        Code = "UC"
                    },
                    new CreatePassengerNameRecordRQAirBookHaltOnStatus()
                    {
                        Code = "UN"
                    },
                    new CreatePassengerNameRecordRQAirBookHaltOnStatus()
                    {
                        Code = "LL"
                    }
                },
                OriginDestinationInformation = GetFlightSegments(segmentDetails, paxCount).ToArray()
            };
        }

        private static IEnumerable<CreatePassengerNameRecordRQAirBookFlightSegment> GetFlightSegments(IEnumerable<Segment> segmentDetails, int paxCount)
        {
            return segmentDetails.Select(x => new CreatePassengerNameRecordRQAirBookFlightSegment
            {
                ArrivalDateTime = x.ScheduledArrivalTime.LocalDateTime,
                DepartureDateTime = x.ScheduledDepartureTime.LocalDateTime,
                DestinationLocation = new() { LocationCode = x.DestinationAirportCode },
                OriginLocation = new() { LocationCode = x.OriginAirportCode },
                FlightNumber = x.MarketingFlightNumber,
                MarketingAirline = new() { FlightNumber = x.MarketingFlightNumber, Code = x.MarketingCarrier },
                NumberInParty = Convert.ToString(paxCount),
                Status = "NN",
                ResBookDesigCode = x.ClassOfService
            });
        }

        private static CreatePassengerNameRecordRQTravelItineraryAddInfo TravelItineraryAddInfoMapper(IEnumerable<Customer> customerDetails, Parties parties, IEnumerable<TimeLimit> timeLimits)
        {
            var hasCtcrOnlyPassenger = customerDetails.Any(x => x.Ptc != Constants.Application.Infant
                && x.Contacts?.ContactRefusedInd == true
                && string.IsNullOrWhiteSpace(x.Contacts?.Email)
                && string.IsNullOrWhiteSpace(x.Contacts?.Phone));
            var primaryContact = hasCtcrOnlyPassenger
                ? customerDetails.Where(x => x.Ptc == "ADT"
                    && !string.IsNullOrWhiteSpace(x.Contacts?.Email)
                    && !string.IsNullOrWhiteSpace(x.Contacts?.Phone)).Take(1).ToArray()
                : customerDetails.Where(x => x.Ptc != Constants.Application.Infant && x.Contacts != null).Take(1).ToArray();

            return new()
            {
                CustomerInfo = new()
                {
                    ContactNumbers = primaryContact.Any(x => x.Contacts?.Phone != null) ? GetContactNumbers(primaryContact.Where(x => x.Contacts?.Phone != null), hasCtcrOnlyPassenger).ToArray() : null,
                    CustLoyalty = customerDetails.Any(x => x.LoyaltyMemberDetails != null) ? GetCustLoyalty(customerDetails.Where(x => x.LoyaltyMemberDetails != null)).ToArray() : null,
                    Corporate = parties.CorporateAccount != null ? new() { ID = parties.CorporateAccount.CorporateCode } : null,
                    PersonName = GetPersonNames(customerDetails).ToArray(),
                    Email = primaryContact.Any(x => x.Contacts?.Email != null) ? GetEmail(primaryContact.Where(x => x.Contacts?.Email != null), hasCtcrOnlyPassenger).ToArray() : null
                },
                AgencyInfo = new()
                {
                    Ticketing = new() { TicketType = "7TAW", TicketTimeLimit = timeLimits != null && timeLimits.Any(x => x.Type == TimeLimit.Types.TicketingTimeLimit) ? Convert.ToDateTime(timeLimits.FirstOrDefault(x => x.Type == TimeLimit.Types.TicketingTimeLimit)?.Limit).ToString("MM-ddTHH:00") : null }
                }
            };
        }

        private static IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoContactNumber> GetContactNumbers(IEnumerable<Customer> customerDetails, bool useActualNameNumber)
        {
            return customerDetails.Select((x, index) =>
            {
                var contacts = x.Contacts;
                return new CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoContactNumber()
                {
                    Phone = contacts.Phone,
                    NameNumber = useActualNameNumber ? $"{x.LastNameNumber}.{x.FirstNameNumber}" : $"{index + 1}.1",
                    PhoneUseType = contacts.PhoneType?[..1]
                };
            });
        }

        private static IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoCustLoyalty> GetCustLoyalty(IEnumerable<Customer> customerDetails)
        {
            return customerDetails.Select((x, index) =>
            {
                var loyaltyMemberDetails = x.LoyaltyMemberDetails;
                return new CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoCustLoyalty()
                {
                    MembershipID = loyaltyMemberDetails.MemberNumber,
                    NameNumber = $"{x.LastNameNumber}.{x.FirstNameNumber}",
                    ProgramID = loyaltyMemberDetails.ProgramCode,
                    TravelingCarrierCode = loyaltyMemberDetails.ProgramCode
                };
            });
        }


        private static IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoPersonName> GetPersonNames(IEnumerable<Customer> customerDetails)
        {
            return customerDetails.Select((x, index) => new CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoPersonName()
            {
                GivenName = x.FirstName,
                Surname = x.LastName,
                PassengerType = x.Ptc == Constants.Application.Child ? Constants.Application.CNN : x.Ptc == Constants.Application.GBECode ? Constants.Application.GBESabreCode : x.Ptc,
                NameNumber = $"{x.LastNameNumber}.{x.FirstNameNumber}",
                Infant = x.Ptc == Constants.Application.Infant,
                InfantSpecified = x.Ptc == Constants.Application.Infant
            });
        }

        private static IEnumerable<CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoEmail> GetEmail(IEnumerable<Customer> customerDetails, bool useActualNameNumber)
        {
            return customerDetails.Select((x, index) =>
            {
                return new CreatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoEmail()
                {
                    Address = x.Contacts.Email,
                    NameNumber = useActualNameNumber ? $"{x.LastNameNumber}.{x.FirstNameNumber}" : $"{index + 1}.1",
                };
            });
        }

        public static CreatePassengerNameRecordRQRequest GetCPNRRequest(string token, OrderCreateRequest orderCreateRequest)
        {


            return new()
            {
                CreatePassengerNameRecordRQ = CreatePnrReqMapper(orderCreateRequest),
                MessageHeader = new()
                {
                    Action = Constants.Application.CreatePNRAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = token }

            };
        }

        private static bool IsItxFare(OrderCreateRequest orderCreateRequest)
        {
            var fareTypeRequest = orderCreateRequest?.FareTypeRequest;

            return fareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode
                && (string.Equals(fareTypeRequest?.FareSubTypeCode, Constants.Application.PrivateFareITX, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(fareTypeRequest?.FareSubTypeCode, Constants.Application.PrivateFareITXPreferred, StringComparison.OrdinalIgnoreCase));
        }
    }
}
