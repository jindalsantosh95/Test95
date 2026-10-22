using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public class CustomerDetailsMapper
    {
        public static List<CustomerDetails> Air4ToOrderSearchResponseMapper(RetrieveResponseType response, IEnumerable<Passenger> passengers, List<EAPI.Domain.Virgin.Models.Customer.Customer> flightCustomer)
        {
            var customerDetails = new List<CustomerDetails>();
            int ptcCount = 0;
            foreach (var customer in passengers)
            {
                var ptc = response.TeardownContextResponse.Pnr?.Ptc != null ? response.TeardownContextResponse.Pnr.Ptc.Any(x => x.ApplyToAllFlag == "Y") ? ExpandedPtc(response.TeardownContextResponse.Pnr.Ptc.First().PassengerTypeCode) : ExpandedPtc(response.TeardownContextResponse.Pnr.Ptc[ptcCount].PassengerTypeCode) : null;
                var cust = flightCustomer?.Find(x => x.CustId == customer.CustomerId);

                customerDetails.Add(new()
                {
                    Contacts = new Contacts()
                    {
                        Countrycode = GetCountryCode(response),
                        Email = customer.LastNameNumber == passengers.First().LastNameNumber ? response.TeardownContextResponse.Pnr.OtherServiceInformation?.FirstOrDefault(x => x.FreeformText.Contains('@')) != null ? response.TeardownContextResponse.Pnr.OtherServiceInformation?.FirstOrDefault(x => x.FreeformText.Contains('@')).FreeformText.Split('/')[1].Replace(" ", string.Empty) : null : null,
                        Phone = customer.LastNameNumber == passengers.First().LastNameNumber && response.TeardownContextResponse.Pnr?.Phone?.FirstOrDefault(x => x.FreeformText.Any(y => char.IsDigit(y)))?.FreeformText.Split('/')[0].Where(x => char.IsDigit(x)) != null ? string.Join(string.Empty, response.TeardownContextResponse.Pnr?.Phone?.FirstOrDefault(x => x.FreeformText.Any(y => char.IsDigit(y)))?.FreeformText.Split('/')[0].Where(x => char.IsDigit(x))) : string.Empty
                    },
                    CustId = customer.CustomerId,
                    FirstName = customer.FirstName,
                    FirstNameNumber = customer.FirstNameNumber,
                    LastNameNumber = customer.LastNameNumber,
                    LastName = customer.LastName,
                    PTC = ptc,
                    Title = customer.Title,
                    DateOfBirth = cust != null ? cust.DateOfBirth : string.Empty,
                    Gender = cust != null ? cust.Gender : string.Empty,
                    LoyaltyMemberDetails = GetLoyaltyMemberDetails(response, customer.LastNameNumber),
                    AssociatedWithAdult = ptc == PtcEnum.INF.ToString() ? GetAssociatedAdultCustomerId($"{customer.LastName}/{customer.FirstName}", response) : null,
                    TravelDocuments = cust?.TravelDocuments != null && cust?.TravelDocuments.Count() > 0 ? MapTravelDocument(cust.TravelDocuments) : null,
                    Addresses = cust?.Addresses != null && cust?.Addresses?.AddressLine1 != null ? new()
                    {
                        AddressLine1 = cust.Addresses.AddressLine1,
                        City = cust.Addresses.City,
                        Region = cust.Addresses.Region,
                        PostalCode = cust.Addresses.PostalCode,
                        Country = cust.Addresses.Country,
                        AddressType = cust.Addresses.AddressType
                    } : null,
                    GovtIssuedDocs = cust?.GovtIssuedDocs != null && cust?.GovtIssuedDocs.Count() > 0 ? MapGovtIssuedDocs(cust.GovtIssuedDocs) : null
                });
                ptcCount++;
            }
            return customerDetails;
        }

        private static IEnumerable<GovtIssuedDoc> MapGovtIssuedDocs(IEnumerable<EAPI.Domain.Virgin.Models.Common.GovtIssuedDoc> govtIssuedDocs)
        {
            List<GovtIssuedDoc> govtIssuedDocList = new();

            govtIssuedDocs?.ToList().ForEach(doc =>
            {
                govtIssuedDocList.Add(new GovtIssuedDoc()
                {
                    DocumentType = doc.DocumentType,
                    DocumentNumber = doc.DocumentNumber,
                    CountryOfIssue = doc.CountryOfIssue
                });
            });

            return govtIssuedDocList;
        }

        private static IEnumerable<TravelDocument> MapTravelDocument(List<EAPI.Domain.Virgin.Models.Common.TravelDocument> travelDocuments)
        {
            List<TravelDocument> travelDocList = new();

            travelDocuments?.ForEach(travelDocument =>
            {
                travelDocList.Add(new TravelDocument()
                {
                    DocumentType = travelDocument.DocumentType,
                    DocumentNumber = travelDocument.DocumentNumber,
                    DateOfBirth = travelDocument.DateOfBirth,
                    CountryOfIssue = travelDocument.CountryOfIssue,
                    CountryOfResidence = travelDocument.CountryOfResidence,
                    PersonName = new PersonName()
                    {
                        FirstName = travelDocument.PersonName?.FirstName,
                        LastName = travelDocument.PersonName?.LastName,
                    },
                    ExpirationDate = travelDocument.ExpirationDate,
                    Gender = travelDocument.Gender,
                    IssueDate = travelDocument.IssueDate,
                    Verified = travelDocument.Verified,
                    RegionOfIssuance = travelDocument.RegionOfIssuance,
                    Nationality = travelDocument.Nationality
                });
            });

            return travelDocList;
        }

        private static string GetCountryCode(RetrieveResponseType response)
        {
            if (response.TeardownContextResponse.Pnr.Phone?.Any(x => x.PhoneFieldNumber == Constants.Application.PhoneFieldNumber) == true && !string.IsNullOrEmpty(response.TeardownContextResponse.Pnr.Phone?.First(x => x.PhoneFieldNumber == Constants.Application.PhoneFieldNumber).FreeformText))
            {
                string contactDetails = response.TeardownContextResponse.Pnr.Phone.First(x => x.PhoneFieldNumber == Constants.Application.PhoneFieldNumber).FreeformText;
                return contactDetails.Contains('/') ? contactDetails.Split("/").Last().Split("*").First() : string.Empty;
            }
            else
                return string.Empty;
        }

        public static string ExpandedPtc(string ptc)
        {
            return ptc switch
            {
                "A" => PtcEnum.ADT.ToString(),
                "Y" => PtcEnum.GBE.ToString(),
                "B" => PtcEnum.INF.ToString(),
                "C" => PtcEnum.CHD.ToString(),
                _ => null,
            };
        }

        public static LoyaltyMemberDetails GetLoyaltyMemberDetails(RetrieveResponseType response, string lastNameNumber)
        {
            if (response.TeardownContextResponse.Pnr.FrequentFlyer != null && response.TeardownContextResponse.Pnr.FrequentFlyer.Any(x => x.LastNameNumber == lastNameNumber))
            {
                var frequentFlyer = response.TeardownContextResponse.Pnr.FrequentFlyer.First(x => x.LastNameNumber == lastNameNumber);
                return new LoyaltyMemberDetails()
                {
                    MemberNumber = frequentFlyer.AccountNumber,
                    Status = frequentFlyer.Status,
                    ProgramCode = Constants.Application.VSFlightCode,
                    ProgramName = Constants.Application.VSProgramName
                };
            }
            else
                return null;
        }

        private static string GetAssociatedAdultCustomerId(string infantName, RetrieveResponseType response)
        {
            string customerId = null;

            if (response.TeardownContextResponse.Pnr.SpecialServiceRequest != null && response.TeardownContextResponse.Pnr.SpecialServiceRequest.Any(x => x.RequestCode == Constants.Application.InfantSSRCode))
            {
                foreach (var infantSSR in response.TeardownContextResponse.Pnr.SpecialServiceRequest.Where(x => x.RequestCode == Constants.Application.InfantSSRCode))
                {
                    if (infantSSR.Remarks != null && infantSSR.Remarks.Contains(RegExHelper.RemoveWhiteSpace(infantName)))
                        customerId = response.TeardownContextResponse.Pnr.Passenger.Any(x => x.FirstNameNumber == infantSSR.FirstNameNumber && x.LastNameNumber == infantSSR.LastNameNumber) ? response.TeardownContextResponse.Pnr.Passenger.First(x => x.FirstNameNumber == infantSSR.FirstNameNumber && x.LastNameNumber == infantSSR.LastNameNumber).CustomerId : null;
                }
                return customerId;
            }

            return customerId;
        }
    }
}
