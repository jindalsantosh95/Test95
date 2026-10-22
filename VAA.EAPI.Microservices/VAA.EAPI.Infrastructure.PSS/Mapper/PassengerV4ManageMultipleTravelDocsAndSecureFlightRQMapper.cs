using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Common.Enums;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class PassengerV4ManageMultipleTravelDocsAndSecureFlightRQMapper
    {
        public static ManageMultipleTravelDocsAndSecureFlightRequestType ManageMultipleTravelDocsAndSecureFlightReq(Customer customer, string sessionId, bool retrieveFlag)
        {
            DocumentDetailDataType[] apisInformation = (customer.TravelDocuments != null) ? GetDocumentDetailData(customer) : null;
            if (!retrieveFlag)
            {
                var dateOfBirth = Convert.ToDateTime(customer.DateOfBirth);

                return new ManageMultipleTravelDocsAndSecureFlightRequestType()
                {
                    RequestInfo = new()
                    {
                        ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                        SessionId = sessionId
                    },
                    PassengerNameItem = GetPassengerNameItem(customer, retrieveFlag),
                    SecureFlightPassengerData = (apisInformation != null && apisInformation.Length > 0) ? null : new()
                    {
                        BirthDay = dateOfBirth.ToString("dd"),
                        BirthMonth = dateOfBirth.ToString("MM"),
                        BirthYear = dateOfBirth.ToString("yy"),
                        Gender = customer.Gender,
                        SurName = customer.LastName,
                        MethodOfDataEntry = Constants.Application.DataEntryMethod
                    },
                    DocumentDetailData = (apisInformation != null && apisInformation.Length > 0) ? apisInformation : null,
                    GovtIssuedNumber = customer.GovtIssuedDocs != null && customer.GovtIssuedDocs.Where(govtIssuedDoc => govtIssuedDoc.DocumentType == DocumentTypes.KnownTraveller.ToString() || govtIssuedDoc.DocumentType == DocumentTypes.Redress.ToString()).Any()
                    ? GetGovtIssuedDocs(customer.GovtIssuedDocs.Where(govtIssuedDoc => govtIssuedDoc.DocumentType == DocumentTypes.KnownTraveller.ToString() || govtIssuedDoc.DocumentType == DocumentTypes.Redress.ToString())) : null,
                };
            }

            return new ManageMultipleTravelDocsAndSecureFlightRequestType()
            {
                RequestInfo = new()
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    SessionId = sessionId
                },
                PassengerNameItem = GetPassengerNameItem(customer, retrieveFlag),
                DocumentDetailData = apisInformation?.Length > 0 ? apisInformation : null,
                GovtIssuedNumber = customer.GovtIssuedDocs != null && customer.GovtIssuedDocs.Where(govtIssuedDoc => govtIssuedDoc.DocumentType == DocumentTypes.KnownTraveller.ToString() || govtIssuedDoc.DocumentType == DocumentTypes.Redress.ToString()).Any()
                    ? GetGovtIssuedDocs(customer.GovtIssuedDocs.Where(govtIssuedDoc => govtIssuedDoc.DocumentType == DocumentTypes.KnownTraveller.ToString() || govtIssuedDoc.DocumentType == DocumentTypes.Redress.ToString())) : null,
            };
        }
        private static PassengerNameItemType GetPassengerNameItem(Customer customer, bool retrieveFlag)
        {
            PassengerNameItemType passengerNameItemType = new PassengerNameItemType()
            {
                InfantIndicator = Constants.Application.InfantIndicator,
                PassengerGivenName = customer.FirstName,
                PassengerNameNumber = $"{customer.LastNameNumber}.{customer.FirstNameNumber}",
                PassengerSurName = customer.LastName,
                StreetAddress = customer.Addresses?.AddressLine1,
                City = customer.Addresses?.City,
                State = customer.Addresses?.Region,
                Country = customer.Addresses?.Country,
                ZipCode = customer.Addresses?.PostalCode
            };
            if (retrieveFlag || customer.Addresses != null)
            {
                passengerNameItemType.RequestOption = Constants.Application.RequestOption;
                passengerNameItemType.PsngrSFPDCanBeSavedOnHost = Constants.Application.CharValueN;
                passengerNameItemType.SFPDStatus = Constants.Application.SfpdStatus;
                passengerNameItemType.VisaChecked = Constants.Application.CharValueN;
                passengerNameItemType.VisaReqdBasedOnNationality = Constants.Application.VisaReqdBasedOnNationality;
                passengerNameItemType.CityOfTransactionOccurence = customer?.Addresses?.DepartureCity;
            }
            return passengerNameItemType;

        }
        private static GovtIssuedNumberType[] GetGovtIssuedDocs(IEnumerable<GovtIssuedDoc> travelDocuments)
        {
            return travelDocuments.Select(travelDoc => new GovtIssuedNumberType
            {
                GovtIssuedNumber = travelDoc.DocumentNumber,
                GovtIssuedNumberType1 = travelDoc.DocumentType == DocumentTypes.Redress.ToString() ? Constants.Application.RedressDocType : Constants.Application.KTNDocType
            }).ToArray();
        }
        public static DocumentDetailDataType[] GetDocumentDetailData(Customer traveller)
        {
            List<DocumentDetailDataType> travelDocuments = new();
            List<TravelDocument> travelDoc = traveller.TravelDocuments.Where(doc => (doc.DocumentType == DocumentTypes.Passport.ToString() ||
            doc.DocumentType == DocumentTypes.Visa.ToString() || doc.DocumentType == DocumentTypes.GreenCard.ToString())).ToList();
            DateTime dateOfBirth, expirationDate, issueDate;

            if (travelDoc != null)
            {
                foreach (TravelDocument doc in travelDoc)
                {
                    dateOfBirth = Convert.ToDateTime(traveller.DateOfBirth);
                    expirationDate = Convert.ToDateTime(doc.ExpirationDate);
                    issueDate = Convert.ToDateTime(doc.IssueDate);

                    travelDocuments.Add(new DocumentDetailDataType
                    {
                        BirthDay = (traveller.DateOfBirth != null) ? dateOfBirth.ToString("dd") : null,
                        BirthMonth = (traveller.DateOfBirth != null) ? dateOfBirth.ToString("MM") : null,
                        BirthYear = (traveller.DateOfBirth != null) ? dateOfBirth.ToString("yy") : null,
                        CountryOfResidence = doc.CountryOfResidence,
                        DocumentExpirationDateDay = (doc.ExpirationDate != null) ? expirationDate.ToString("dd") : null,
                        DocumentExpirationDateMonth = (doc.ExpirationDate != null) ? expirationDate.ToString("MM") : null,
                        DocumentExpirationDateYear = (doc.ExpirationDate != null) ? expirationDate.ToString("yy") : null,
                        DocumentIssueDateDay = (doc.IssueDate != null) ? issueDate.ToString("dd") : null,
                        DocumentIssueDateMonth = (doc.IssueDate != null) ? issueDate.ToString("MM") : null,
                        DocumentIssueDateYear = (doc.IssueDate != null) ? issueDate.ToString("yy") : null,
                        DocumentNumber = doc.DocumentNumber,
                        DocumentType = GetDocumentType(doc.DocumentType),
                        Gender = traveller.Gender,
                        InfantIndicator = (traveller.Ptc == Constants.Application.Infant) ? Constants.Application.CharValueY : Constants.Application.CharValueN,
                        IssuingCountry = doc.CountryOfIssue,
                        Nationality = doc.Nationality,
                        PlaceOfIssuance = null,
                        PsgrDocumentGivenName = doc.PersonName?.FirstName,
                        PsgrDocumentSurname = doc.PersonName?.LastName,
                        SourceOfDocumentOnHost = Constants.Application.SourceOfDocumentOnHost,
                        AuthorizationToRetainTravelDocInfo = Constants.Application.CharValueN,
                        IDVerifiedByAgentIndicator = Constants.Application.CharValueN,
                        MethodOfDataEntry = Constants.Application.MethodOfDataEntryForAgent
                    });
                }
            }

            return travelDocuments.ToArray();
        }

        private static string GetDocumentType(string travelDocumentType)
        {
            string bookingDocumentType = string.Empty;

            if (travelDocumentType == DocumentTypes.Passport.ToString())
                bookingDocumentType = Constants.Application.PassportDocType;
            else if (travelDocumentType == DocumentTypes.Visa.ToString())
                bookingDocumentType = Constants.Application.VisaDocType;
            else if (travelDocumentType == DocumentTypes.GreenCard.ToString())
                bookingDocumentType = Constants.Application.GreenCardDocType;

            return bookingDocumentType;
        }
    }

}