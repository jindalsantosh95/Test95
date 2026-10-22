using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class ManageCustomerRequestMapper
    {
        public static UpdatePassengerNameRecordRQRequest CustomerPnrReqMapper(ManageCustomerRequest manageCustomerRequest, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers, string token)
        {
            return new()
            {
                UpdatePassengerNameRecordRQ = CustomerUpdatePnr(manageCustomerRequest, passengers),
                MessageHeader = new()
                {
                    Action = Constants.Application.UpdatePNRAction,
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

        public static UpdatePassengerNameRecordRQ CustomerUpdatePnr(ManageCustomerRequest manageCustomerRequest, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            return new()
            {
                Itinerary = new() { id = manageCustomerRequest.Rloc },
                TravelItineraryAddInfo = manageCustomerRequest.CustomerDetails.Any(x => x.LoyaltyMemberDetails != null) ? TravelItineraryAddInfoMapper(manageCustomerRequest.CustomerDetails) : null,
                SpecialReqDetails = manageCustomerRequest.CustomerDetails.Any(x => x.TravelDocuments != null || x.Employer != null || manageCustomerRequest.CustomerDetails.Any(x => !string.IsNullOrEmpty(x.DateOfBirth) && !string.IsNullOrEmpty(x.Gender))) ? SpecialReqDetailsMapper(manageCustomerRequest, passengers) : null,
                PostProcessing = PostProcessingMapper(),
                version = "1.1.0"
            };
        }

        private static UpdatePassengerNameRecordRQTravelItineraryAddInfo TravelItineraryAddInfoMapper(IEnumerable<Customer> customerDetails)
        {
            return new()
            {
                CustomerInfo = new()
                {
                    CustLoyalty = customerDetails.Any(x => x.LoyaltyMemberDetails != null) ? GetCustLoyalty(customerDetails.Where(x => x.LoyaltyMemberDetails != null)).ToArray() : null,
                }
            };
        }

        private static IEnumerable<UpdatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoCustLoyalty> GetCustLoyalty(IEnumerable<Customer> customerDetails)
        {
            return customerDetails.Select((x, index) =>
            {
                var loyaltyMemberDetails = x.LoyaltyMemberDetails;
                return new UpdatePassengerNameRecordRQTravelItineraryAddInfoCustomerInfoCustLoyalty()
                {
                    MembershipID = loyaltyMemberDetails.MemberNumber,
                    NameNumber = $"{x.LastNameNumber}.{x.FirstNameNumber}",
                    ProgramID = loyaltyMemberDetails.ProgramCode,
                    TravelingCarrierCode = loyaltyMemberDetails.ProgramCode
                };
            });
        }

        private static UpdatePassengerNameRecordRQPostProcessing PostProcessingMapper()
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
            };
        }

        private static UpdatePassengerNameRecordRQSpecialReqDetails SpecialReqDetailsMapper(ManageCustomerRequest manageCustomerRequest, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            return new()
            {
                SpecialService = new()
                {
                    SpecialServiceInfo = new()
                    {
                        AdvancePassenger = (!manageCustomerRequest.TicketedNameCorrection || !manageCustomerRequest.UnTicketedNameCorrection) && manageCustomerRequest.CustomerDetails.Any(customer => customer.TravelDocuments != null && customer.DateOfBirth != null && customer.DateOfBirth != DateTime.MinValue.ToString() && !string.IsNullOrEmpty(customer.Gender) || customer.Addresses != null) ? GetAdvancePassenger(manageCustomerRequest.CustomerDetails, passengers, manageCustomerRequest.AmendApis).ToArray() : null,
                        SecureFlight = manageCustomerRequest.CustomerDetails.Any(customer => customer.TravelDocuments != null || (customer.DateOfBirth != null && customer.DateOfBirth != DateTime.MinValue.ToString() && !string.IsNullOrEmpty(customer.Gender))) ? GetSecureFlights(manageCustomerRequest.CustomerDetails, passengers, manageCustomerRequest.AmendApis, manageCustomerRequest.TicketedNameCorrection || manageCustomerRequest.UnTicketedNameCorrection).ToArray() : null,
                        Service = (manageCustomerRequest.TicketedNameCorrection || manageCustomerRequest.UnTicketedNameCorrection) ? GetNameCorrectionServices(manageCustomerRequest, passengers) : manageCustomerRequest.OrderInstructions != null ? GetService(manageCustomerRequest).ToArray() : null
                    }
                }
            };
        }

        private static IEnumerable<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger> GetAdvancePassenger(IEnumerable<Customer> customerDetails, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers, bool isAmendApis)
        {
            var associatedAdults = new Dictionary<string, string>();

            if (isAmendApis && customerDetails.Any(x => x.Ptc == Constants.Application.Infant))
            {
                var infantName = customerDetails.First().Ptc == Constants.Application.Infant ? passengers.First(x => x.nameId.Split(".").First() == customerDetails.First().LastNameNumber) : null;
                var associatedAdult = customerDetails.First().Ptc == Constants.Application.Infant ? passengers.FirstOrDefault(x => x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[2] == infantName.FirstName && x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[1] == infantName.LastName) : null;
                associatedAdults.Add($"{customerDetails.First().LastNameNumber}.{customerDetails.First().FirstNameNumber}", $"{1.1}");
            }
            else
            {
                var adults = customerDetails.Where(x => x.Ptc == "ADT").ToList();

                if (customerDetails.Any(x => x.Ptc == Constants.Application.Infant))
                {
                    foreach (var inf in customerDetails.Where(x => x.Ptc == Constants.Application.Infant))
                    {
                        associatedAdults.Add($"{inf.LastNameNumber}.{inf.FirstNameNumber}", $"{adults.First().LastNameNumber}.{adults.First().FirstNameNumber}");
                        adults.RemoveAt(0);
                    }
                }
            }

            return customerDetails.Where(customer => customer.TravelDocuments != null && customer.DateOfBirth != null && customer.DateOfBirth != DateTime.MinValue.ToString() && !string.IsNullOrEmpty(customer.Gender) || customer.Addresses != null).SelectMany(customer =>
            {
                var docs = customerDetails.Where(x => x.TravelDocuments != null)?.SelectMany(x => x.TravelDocuments).Where(x => (x.PersonName?.FirstName == customer.FirstName && x.PersonName?.LastName == customer.LastName) && (x.DocumentType == Constants.Application.PassportDocType || x.DocumentType == Constants.Application.GreenCardDocType || x.DocumentType == Constants.Application.PTDocType || x.DocumentType == Constants.Application.GCDocType || x.DocumentType == Constants.Application.VIDocType || x.DocumentType == Constants.Application.VisaDocType))?.Select(x =>
                {
                    if (x.DocumentType == Constants.Application.PassportDocType || x.DocumentType == Constants.Application.PTDocType)
                    {
                        return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
                        {
                            Document = new()
                            {
                                Number = x.DocumentNumber,
                                Type = Constants.Application.SabrePassportDocType,
                                ExpirationDate = Convert.ToDateTime(x.ExpirationDate),
                                ExpirationDateSpecified = true,
                                IssueCountry = x.CountryOfIssue,
                                NationalityCountry = x.Nationality
                            },
                            PersonName = new()
                            {
                                Gender = customer.Ptc == Constants.Application.Infant ? Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender + "I") : Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender),
                                GenderSpecified = true,
                                NameNumber = customer.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customer.LastNameNumber}.{customer.FirstNameNumber}").Value : $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                                DateOfBirth = DateTime.Parse(customer.DateOfBirth),
                                DateOfBirthSpecified = true,
                                GivenName = customer.FirstName,
                                Surname = customer.LastName
                            }
                        };
                    }

                    if (x.DocumentType == Constants.Application.VIDocType || x.DocumentType == Constants.Application.VisaDocType)
                    {
                        return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
                        {
                            Document = new()
                            {
                                Number = x.DocumentNumber,
                                Type = Constants.Application.SabreVisaDocType,
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
                        return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
                        {
                            Document = new()
                            {
                                Number = x.DocumentNumber,
                                Type = Constants.Application.SabreGreenCardDocType,
                                ExpirationDate = Convert.ToDateTime(x.ExpirationDate),
                                ExpirationDateSpecified = true,
                                IssueCountry = x.CountryOfIssue,
                                NationalityCountry = x.Nationality
                            },
                            PersonName = new()
                            {
                                Gender = customer.Ptc == Constants.Application.Infant ? Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender + "I") : Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassengerPersonNameGender>(customer.Gender),
                                GenderSpecified = true,
                                NameNumber = customer.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customer.LastNameNumber}.{customer.FirstNameNumber}").Value : $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                                DateOfBirth = DateTime.Parse(customer.DateOfBirth),
                                DateOfBirthSpecified = true,
                                GivenName = customer.FirstName,
                                Surname = customer.LastName
                            }
                        };
                    }


                    return null;

                }).Where(x => x != null);

                if (customer.Addresses != null)
                {
                    docs = docs.Append(new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoAdvancePassenger()
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

        private static IEnumerable<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight> GetSecureFlights(IEnumerable<Customer> customerDetails, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers, bool isAmendApis, bool isNameCorrection)
        {
            var associatedAdults = new Dictionary<string, string>();
            IEnumerable<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight> secureFlights = null;

            if ((isAmendApis || isNameCorrection) && customerDetails.Any(x => x.Ptc == Constants.Application.Infant))
            {
                var infantName = customerDetails.First().Ptc == Constants.Application.Infant ? passengers.First(x => x.nameId.Split(".").First() == customerDetails.First().LastNameNumber) : null;
                var associatedAdult = customerDetails.First().Ptc == Constants.Application.Infant ? passengers.FirstOrDefault(x => x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[2] == infantName.FirstName && x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[1] == infantName.LastName) : null;
                associatedAdults.Add($"{customerDetails.First().LastNameNumber}.{customerDetails.First().FirstNameNumber}", $"{associatedAdult.nameId}");
            }
            else
            {
                var adults = customerDetails.Where(x => x.Ptc == "ADT").ToList();
                if (customerDetails.Any(x => x.Ptc == Constants.Application.Infant))
                {
                    foreach (var inf in customerDetails.Where(x => x.Ptc == Constants.Application.Infant))
                    {
                        associatedAdults.Add($"{inf.LastNameNumber}.{inf.FirstNameNumber}", $"{adults.First().LastNameNumber}.{adults.First().FirstNameNumber}");
                        adults.RemoveAt(0);
                    }
                }
            }

            if (!isNameCorrection)
            {
                secureFlights = customerDetails.Where(customer => customer.TravelDocuments != null && customer.DateOfBirth != null && customer.DateOfBirth != DateTime.MinValue.ToString() && !string.IsNullOrEmpty(customer.Gender))?.SelectMany(customer =>
                {
                    return customerDetails.SelectMany(x => x.TravelDocuments).Where(doc => doc.PersonName?.FirstName == customer.FirstName && doc.PersonName?.LastName == customer.LastName && (doc.DocumentType == Constants.Application.RedressDocType || doc.DocumentType == Constants.Application.KnowntravelerDocType))?.Select(doc =>
                    {
                        return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight()
                        {
                            IssueCountry = doc.CountryOfIssue,
                            PersonName = new()
                            {
                                NameNumber = customer.Ptc == Constants.Application.Infant ? associatedAdults.First(x => x.Key == $"{customer.LastNameNumber}.{customer.FirstNameNumber}").Value : $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                                LapChild = customer.Ptc == Constants.Application.Infant,
                                LapChildSpecified = customer.Ptc == Constants.Application.Infant
                            },
                            RedressNumber = doc.DocumentType == Constants.Application.RedressDocType ? doc.DocumentNumber : null,
                            KnownTravelerNumber = doc.DocumentType == Constants.Application.KnowntravelerDocType ? doc.DocumentNumber : null
                        };
                    });
                });
            }

            if (!isAmendApis || isNameCorrection)
            {
                if (customerDetails.Any(x => x.DateOfBirth != null && x.Gender != null))
                {
                    var sfpds = customerDetails.Where(x => x.DateOfBirth != null && x.Gender != null).Select(x =>
                    {
                        if (x.Ptc == Constants.Application.Infant)
                        {
                            var secureFlight = new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight()
                            {
                                SegmentNumber = "A",
                                PersonName = new()
                                {
                                    DateOfBirth = x.DateOfBirth,
                                    Gender = Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlightPersonNameGender>($"{x.Gender}I"),
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
                            return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight()
                            {
                                SegmentNumber = "A",
                                PersonName = new()
                                {
                                    DateOfBirth = x.DateOfBirth,
                                    Gender = Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlightPersonNameGender>(x.Gender),
                                    GenderSpecified = true,
                                    GivenName = x.FirstName,
                                    Surname = x.LastName,
                                    NameNumber = $"{x.LastNameNumber}.{x.FirstNameNumber}"
                                },

                            };
                        }
                    });
                    secureFlights = secureFlights != null ? secureFlights.Concat(sfpds) : sfpds;
                }
            }

            return secureFlights;

        }

        private static IEnumerable<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService> GetService(ManageCustomerRequest manageCustomerRequest)
        {
            var associatedAdults = new Dictionary<string, string>();
            var adults = manageCustomerRequest.CustomerDetails.Where(x => x.Ptc == "ADT").ToList();

            if (manageCustomerRequest.CustomerDetails.Any(x => x.Ptc == Constants.Application.Infant))
            {
                foreach (var inf in manageCustomerRequest.CustomerDetails.Where(x => x.Ptc == Constants.Application.Infant))
                {
                    associatedAdults.Add($"{inf.LastNameNumber}.{inf.FirstNameNumber}", $"{adults.First().LastNameNumber}.{adults.First().FirstNameNumber}");
                    adults.RemoveAt(0);
                }
            }

            var ssrs = manageCustomerRequest.OrderInstructions.Select(orderInstruction =>
            {
                //Add GST Details as SSRs
                if (orderInstruction.ItemCode == "GSTA" || orderInstruction.ItemCode == "GSTN" || orderInstruction.ItemCode == "GSTP" || orderInstruction.ItemCode == "GSTE")
                {
                    return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                    {
                        SSR_Code = orderInstruction.ItemCode,
                        SegmentNumber = "A",
                        PersonName = new()
                        {
                            NameNumber = $"{manageCustomerRequest.CustomerDetails.First().LastNameNumber}.{manageCustomerRequest.CustomerDetails.First().FirstNameNumber}"
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
                if (orderInstruction.ItemText?.Contains(Constants.Application.FlightService) ?? false)
                {
                    var flightServiceInfo = orderInstruction?.ItemText?.Split('/');
                    var paxId = flightServiceInfo[1];
                    var segmentNumber = flightServiceInfo[2]?.Trim('S', 'E', 'G');
                    var customerDetail = manageCustomerRequest.CustomerDetails.FirstOrDefault(cust => cust.CustId == paxId);

                    return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
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

            }).Where(x => x is not null);

            return ssrs;
        }

        private static UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService[] GetNameCorrectionServices(ManageCustomerRequest manageCustomerRequest, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            var customerDetails = manageCustomerRequest.CustomerDetails.First();
            string text = null;
            var passenger = passengers.First(x => x.passengerType == (customerDetails.Ptc == "CHD" ? "CNN" : customerDetails.Ptc == "GBE" ? "YTH" : customerDetails.Ptc));

            if(customerDetails.FirstName != passenger.FirstName && customerDetails.LastName == passenger.LastName)
            {
                text = "SI YY NME CORRECTION TO FIRST/MIDDLE NAME ONLY";
            }
            else
            {
                text = $"SI YY NME N2 {passenger.FirstName} {passenger.LastName} SHOULD BE {customerDetails.FirstName} {customerDetails.LastName}";
            }

            var ssrServices = new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService[]
            {
                new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                {
                    SSR_Code = "OSI",
                    Text = text,
                    VendorPrefs = new()
                    {
                        Airline = new() { Code = "YY"}
                    }
                }
            };

            bool isInfant = manageCustomerRequest.CustomerDetails.Any(x => x.Ptc == Constants.Application.Infant);

            if (isInfant)
            {
                var infantName = manageCustomerRequest.CustomerDetails.First().Ptc == Constants.Application.Infant ? passengers.First(x => x.nameId.Split(".").First() == manageCustomerRequest.CustomerDetails.First().LastNameNumber) : null;
                var associatedAdult = manageCustomerRequest.CustomerDetails.First().Ptc == Constants.Application.Infant ? passengers.FirstOrDefault(x => x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[2] == infantName.FirstName && x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[1] == infantName.LastName) : null;

                ssrServices = ssrServices.Append(new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService()
                {
                    SSR_Code = Constants.Application.InfantSSRCode,
                    SegmentNumber = "A",
                    Text = $"{manageCustomerRequest.CustomerDetails.First().LastName}/{manageCustomerRequest.CustomerDetails.First().FirstName}/{Convert.ToDateTime(manageCustomerRequest.CustomerDetails.First().DateOfBirth).ToString(Constants.Application.DateFormat).ToUpper()}",
                    PersonName = new() { NameNumber = associatedAdult.nameId }
                }).ToArray();
            }
            return ssrServices;
        }

        public static UpdatePassengerNameRecordRQRequest CustomerPnrReqMapperForDisruption(string rloc, string token)
        {
            return new()
            {
                UpdatePassengerNameRecordRQ = CustomerUpdatePnrForDisruption(rloc),
                MessageHeader = new()
                {
                    Action = Constants.Application.UpdatePNRAction,
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

        private static UpdatePassengerNameRecordRQ CustomerUpdatePnrForDisruption(string rloc)
        {
            return new()
            {
                Itinerary = new() { id = rloc },
                SpecialReqDetails = new()
                {
                     SpecialService = new()
                     {
                         SpecialServiceInfo = new()
                         {
                              Service = new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService[]
                              {
                                  new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                                  {
                                      SSR_Code = Constants.Application.SSRCode,
                                      Text = Constants.Application.OSIText,
                                      VendorPrefs = new()
                                      {
                                          Airline = new() { Code = Constants.Application.OwnerAirline}
                                      }
                                  }
                              }
                         }
                     }
                },
                PostProcessing = PostProcessingMapper(),
                version = "1.1.0"
            };
        }
    }
}
