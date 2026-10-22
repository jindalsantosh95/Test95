using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Common.Enums;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class PassengerV4ManageMultipleTravelDocsAndSecureFlightRSMapper
    {
        public static Customer MapToPassengerV4Response(Customer customer, ManageMultipleTravelDocsAndSecureFlightResponse response, bool retrieveFlag)
        {
            if (!retrieveFlag)
            {
                var comparedName = response.ManageMultipleTravelDocsAndSecureFlightResponse1?.TeardownContextResponse?.Pnr?.Passenger?.FirstOrDefault(x => x.FirstName.Is(customer.FirstName) && x.LastName.Is(customer.LastName));
                if (response?.ManageMultipleTravelDocsAndSecureFlightResponse1 != null)
                {
                    return new Customer()
                    {
                        FirstName = comparedName?.FirstName,
                        FirstNameNumber = comparedName?.FirstNameNumber,
                        LastName = comparedName?.LastName,
                        LastNameNumber = comparedName?.LastNameNumber,
                        CustId = comparedName?.CustomerId,
                        Title = comparedName?.Title
                    };
                }
                else
                    return null;
            }

            if (response.ManageMultipleTravelDocsAndSecureFlightResponse1?.PassengerNameItem != null && response.ManageMultipleTravelDocsAndSecureFlightResponse1?.SecureFlightPassengerData != null)
            {
                var secureData = response.ManageMultipleTravelDocsAndSecureFlightResponse1.SecureFlightPassengerData;
                var govtIssuedDocs = response.ManageMultipleTravelDocsAndSecureFlightResponse1.GovtIssuedNumber;

                return new Customer()
                {
                    FirstName = secureData?.GivenName,
                    FirstNameNumber = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.PassengerNameNumber?.Split(".").LastOrDefault(),
                    LastName = secureData?.SurName,
                    LastNameNumber = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.PassengerNameNumber?.Split(".").FirstOrDefault(),
                    Gender = secureData?.Gender,
                    DateOfBirth = $"{CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(secureData?.BirthYear))}-{secureData?.BirthMonth}-{secureData?.BirthDay}",
                    Addresses = new Address()
                    {
                        City = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.City,
                        Country = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.Country,
                        PostalCode = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.ZipCode,
                        AddressLine1 = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.StreetAddress,
                        Region = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.State,
                        AddressType = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.City != null ? Constants.Application.AddressAtDestinationCode : null
                    },
                    GovtIssuedDocs = govtIssuedDocs != null && govtIssuedDocs.Any() ? GetGovtIssuedDocs(govtIssuedDocs) : null
                };
            }
            else if (response.ManageMultipleTravelDocsAndSecureFlightResponse1?.PassengerNameItem != null && response.ManageMultipleTravelDocsAndSecureFlightResponse1?.DocumentDetailData != null)
            {
                var documentData = response.ManageMultipleTravelDocsAndSecureFlightResponse1.DocumentDetailData;
                var govtIssuedDocs = response.ManageMultipleTravelDocsAndSecureFlightResponse1.GovtIssuedNumber;

                return new Customer()
                {
                    FirstName = documentData?.First().PsgrDocumentGivenName,
                    FirstNameNumber = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.PassengerNameNumber?.Split(".").LastOrDefault(),
                    LastName = documentData?.First().PsgrDocumentSurname,
                    LastNameNumber = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.PassengerNameNumber?.Split(".").FirstOrDefault(),
                    Gender = documentData?.First().Gender,
                    DateOfBirth = $"{CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(documentData?.First().BirthYear))}-{documentData?.First().BirthMonth}-{documentData?.First().BirthDay}",
                    TravelDocuments = documentData != null && documentData.Any() ? GetTravelDocuments(documentData) : null,
                    Addresses = new Address()
                    {
                        City = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.City,
                        Country = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.Country,
                        PostalCode = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.ZipCode,
                        AddressLine1 = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.StreetAddress,
                        Region = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.State,
                        AddressType = response.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem.City != null ? Constants.Application.AddressAtDestinationCode : null
                    },
                    GovtIssuedDocs = govtIssuedDocs != null && govtIssuedDocs.Any() ? GetGovtIssuedDocs(govtIssuedDocs) : null
                };
            }
            return null;
        }

        private static IEnumerable<GovtIssuedDoc> GetGovtIssuedDocs(GovtIssuedNumberType[] govtIssuedDocs)
        {
            return govtIssuedDocs?.ToList().Select(doc =>
                new GovtIssuedDoc
                {
                    DocumentNumber = doc.GovtIssuedNumber,
                    CountryOfIssue = doc.DocumentApplicableCountry,
                    DocumentType = doc?.GovtIssuedNumberType1 switch
                    {
                        Constants.Application.KTNDocType => DocumentTypes.KnownTraveller.ToString(),
                        Constants.Application.RedressDocType => DocumentTypes.Redress.ToString(),
                        _ => doc?.GovtIssuedNumberType1
                    }
                });
        }

        private static List<TravelDocument> GetTravelDocuments(DocumentDetailDataType[] documentData)
        {
            return documentData?.ToList()?.Select(doc =>
                new TravelDocument()
                {
                    DocumentType = doc?.DocumentType switch
                    {
                        Constants.Application.PassportDocType => DocumentTypes.Passport.ToString(),
                        Constants.Application.VisaDocType => DocumentTypes.Visa.ToString(),
                        Constants.Application.GreenCardDocType => DocumentTypes.GreenCard.ToString(),
                        _ => doc.DocumentType
                    },
                    DocumentNumber = doc?.DocumentNumber,
                    DateOfBirth = $"{CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(doc?.BirthYear))}-{doc?.BirthMonth}-{doc?.BirthDay}",
                    CountryOfIssue = doc?.IssuingCountry,
                    CountryOfResidence = doc?.CountryOfResidence,
                    PersonName = new PersonName()
                    {
                        FirstName = doc?.PsgrDocumentGivenName,
                        LastName = doc?.PsgrDocumentSurname,
                    },
                    ExpirationDate = Constants.Application.FourDigitYearPrefix + $"{doc?.DocumentExpirationDateYear}-{doc?.DocumentExpirationDateMonth}-{doc?.DocumentExpirationDateDay}",
                    Gender = doc?.Gender,
                    IssueDate = doc.DocumentIssueDateYear != null ? $"{CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(doc?.DocumentIssueDateYear))}-{doc?.DocumentIssueDateMonth}-{doc?.DocumentIssueDateDay}" : null,
                    Verified = doc?.IDVerifiedByAgentIndicator == Constants.Application.VerificationIndicator,
                    RegionOfIssuance = doc?.PlaceOfIssuance,
                    Nationality = doc?.Nationality
                }).ToList();
        }
    }
}