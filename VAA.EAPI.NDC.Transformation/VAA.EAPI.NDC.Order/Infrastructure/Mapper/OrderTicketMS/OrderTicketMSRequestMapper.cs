using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS
{
    public static class OrderTicketMSRequestMapper
    {
        public static OrderTicketMSCommand MapOrderTicketMSRequest(IATA_OrderCreateRQ orderCreateRq, OfferResponse offerDoc, OrderDocument orderDocument, string PaymentTransactionId,[Optional] bool isCashPayment, [Optional] bool isEasyPay)
        {
            return new OrderTicketMSCommand()
            {
                Parties = offerDoc.Parties,
                Pos = offerDoc.Pos,
                BookingReferences = GetBookingRef(orderDocument),
                CustomerDetails = GetCustomerDetails(orderCreateRq, offerDoc.DataList.CustomerDetails),
                PriceClass = offerDoc.DataList.PriceClass.Select(pc => new PriceClass
                {
                    Id = pc.Id,
                    FareBasisCode = pc.FareBasisCode,
                    FareTypeCode = pc.FareTypeCode,
                    ProductName = pc.ProductName,
                    ClassOfService = pc.ClassOfService,
                    CabinType = pc.CabinType,
                    TicketDesigCode = !string.IsNullOrEmpty(offerDoc.Parties.CorporateAccount?.CorporateCode)? pc.TicketDesigCode: null
                }),
                OrderInstructions = orderDocument.DataList.OrderInstructions,
                SegmentDetails = MapSegmentDetails(orderDocument.DataList.SegmentDetails,offerDoc.DataList),
                SelectedOffers = GetSelectedOffer(offerDoc.Offers, offerDoc.DataList),
                BaggageAllowances = offerDoc.DataList.BaggageAllowances
            };
        }

        private static IEnumerable<Segment> MapSegmentDetails(IEnumerable<Segment> ordersegments, VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS.DataList dataList)
        {
            return ordersegments.Select(seg =>
            {
                var matchingOfferSegment = dataList.OfferSegments.FirstOrDefault(x => x.FlightSegmentRefId.Trim('F') == seg.SegmentId.ToString());

                if (matchingOfferSegment != null)
                {
                    var matchingPriceClass = dataList.PriceClass.FirstOrDefault(x => x.Id == matchingOfferSegment.PriceClassRefIds);

                    if (matchingPriceClass != null)
                    {
                        seg.FareBasis = matchingPriceClass.FareBasisCode;
                    }
                }

                return seg;
            });
        }


        private static BookingReference GetBookingRef(OrderDocument orderResponse)
        {
            return new BookingReference()
            {
                RLOC = orderResponse.BookingReference.RLOC,
                AirlineDesignator = orderResponse.BookingReference.AirlineDesignator,
                RLOCCreateDate = orderResponse.BookingReference.RLOCCreateDate,
                ExternalRecordLocator = orderResponse.BookingReference.ExternalRecordLocator
            };
        }

        private static IEnumerable<PaymentDetails> GetPaymentDetails(IEnumerable<PaymentFunctionsCreateType> paymentFunctions, [Optional] bool isCashPayment, [Optional] bool isEasyPay)
        {
            return paymentFunctions.Select(pay =>
            {
                var details = pay.PaymentProcessingDetails;
                var paymentItem = details?.PaymentMethod?.Item;

                return new PaymentDetails
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = details?.Amount?.CurCode,
                        MonetaryAmount = details?.Amount != null ? details.Amount.Value.ToString() : null
                    },

                    CardMethod = isCashPayment ? null : details?.PaymentMethod == null && details?.Payer == null ? null :
                        isEasyPay && paymentItem is Domain.OrderCreateRQ.PaymentProcessingIATA_EasyPayType easyPay ? GetEasyPayCardDetails(easyPay) :
                                     paymentItem is Domain.OrderCreateRQ.PaymentProcessingPaymentCardType cardType ? GetCardDetails(cardType) : null
                };
            });
        }

        private static CardDetails GetCardDetails(Domain.OrderCreateRQ.PaymentProcessingPaymentCardType cardType)
        {
            return new CardDetails()
            {
                CardNumber = string.IsNullOrEmpty(cardType?.CardNumber) ? null : cardType?.CardNumber,
                ExpiryDate = cardType?.ExpirationDate,
            };
        }

        private static CardDetails GetEasyPayCardDetails(Domain.OrderCreateRQ.PaymentProcessingIATA_EasyPayType cardType)
        {
            return new CardDetails()
            {
                CardNumber = string.IsNullOrEmpty(cardType?.AccountNumber) ? null : cardType?.AccountNumber,
                ExpiryDate = cardType?.ExpirationDate,
            };
        }

        private static IEnumerable<Customer> GetCustomerDetails(IATA_OrderCreateRQ orderCreateRq, IEnumerable<Customer> customerDetails)
        {
            return orderCreateRq.Request.DataLists.PaxList.Select(pax =>
            new Customer()
            {
                CustId = pax.PaxID,
                Ptc = pax.PTC,
                FirstName = pax.Individual.GivenName.FirstOrDefault(),
                LastName = pax.Individual.Surname.ToString(),
                DateOfBirth = pax.Individual.Birthdate.ToString("yyyy-MM-dd"),
                Gender = Enum.GetName(typeof(Domain.OrderCreateRQ.GenderCodeType), pax.Individual.GenderCode),
                Contacts = GetContactInfo(pax, orderCreateRq),
                Addresses = GetAddress(orderCreateRq.Request.DataLists.ContactInfoList.FirstOrDefault(c => c.ContactInfoID == pax.ContactInfoRefID && c.PostalAddress != null)?.PostalAddress?.FirstOrDefault(x => x.ContactTypeText == "D")),
                TravelDocuments = (pax.IdentityDoc != null || pax.RedressCase != null) ? GetTravelDocuments(pax) : null
            });
        }

        private static Address GetAddress(PostalAddressType address)
        {
            return new Address
            {
                AddressType = address?.ContactTypeText,
                AddressLine1 = address?.BuildingRoomText,
                AddressLine2 = address?.StreetText?.FirstOrDefault(),
                AddressLine3 = address?.PO_BoxCode,
                City = address?.CityName,
                Region = address?.CountrySubDivisionName,
                Country = address?.CountryName,
                PostalCode = address?.PostalCode
            };
        }

        private static List<TravelDocument> GetTravelDocuments(PaxType pax)
        {
            IdentityDocType[] identityDocTypes = pax.IdentityDoc;

            List<TravelDocument> travelDocuments = [];

            if (pax.RedressCase != null)
            {
                foreach (RedressCaseType redressCase in pax.RedressCase)
                {
                    travelDocuments.Add(new TravelDocument
                    {
                        DocumentType = Constants.Application.AEA,
                        DocumentNumber = redressCase?.RedressCaseID,
                        CountryOfIssue = redressCase?.CountryCode,
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
                string countryOfResidence = string.Empty;

                foreach (IdentityDocType identityDoc in identityDocTypes)
                {
                    if (!string.IsNullOrEmpty(identityDoc.ResidenceCountryCode))
                        countryOfResidence = identityDoc.ResidenceCountryCode;

                    travelDocuments.Add(new TravelDocument
                    {
                        DateOfBirth = (identityDoc.BirthdateSpecified) ? identityDoc.Birthdate.ToShortDateString() : null,
                        Gender = (identityDoc.GenderCodeSpecified) ? nameof(identityDoc.GenderCode) : null,
                        CountryOfResidence = countryOfResidence,
                        ExpirationDate = (identityDoc.ExpiryDateSpecified) ? identityDoc.ExpiryDate.ToShortDateString() : null,
                        PersonName = new PersonName
                        {
                            FirstName = identityDoc.GivenName?.FirstOrDefault(),
                            MiddleName = identityDoc.MiddleName?.FirstOrDefault(),
                            LastName = identityDoc.Surname,
                            Title = identityDoc.TitleName
                        },
                        DocumentNumber = identityDoc.IdentityDocID,
                        DocumentType = identityDoc.IdentityDocTypeCode,
                        IssueDate = (identityDoc.IssueDateSpecified) ? identityDoc.IssueDate.ToShortDateString() : null,
                        CountryOfIssue = identityDoc.IssuingCountryCode,
                        Nationality = countryOfResidence
                    });

                    if (identityDoc.Visa != null)
                    {
                        foreach (VisaType v in identityDoc.Visa)
                        {
                            travelDocuments.Add(new TravelDocument
                            {
                                ExpirationDate = (v.ExpiryDateSpecified) ? v.ExpiryDate.ToShortDateString() : null,
                                CountryOfResidence = (v?.HostCountryCode) ?? countryOfResidence,
                                Nationality = countryOfResidence,
                                CountryOfIssue = v?.IssuingCountryCode,
                                IssueDate = (v.IssueDateSpecified) ? v.IssueDate.ToShortDateString() : null,
                                DocumentNumber = v.VisaID,
                                DocumentType = Constants.Application.VI,
                                PersonName = new PersonName
                                {
                                    FirstName = identityDoc.GivenName?.FirstOrDefault(),
                                    MiddleName = identityDoc.MiddleName?.FirstOrDefault(),
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

        private static Contacts GetContactInfo(Domain.OrderCreateRQ.PaxType pax, IATA_OrderCreateRQ orderCreateRq)
        {
            var contactInfo = orderCreateRq.Request.DataLists.ContactInfoList.FirstOrDefault(x => x.ContactInfoID == pax.ContactInfoRefID);

            return new Contacts()
            {
                Countrycode = contactInfo?.Phone?.FirstOrDefault()?.CountryDialingCode,
                PhoneAreaNumber = contactInfo?.Phone?.FirstOrDefault()?.AreaCodeNumber,
                Phone = contactInfo?.Phone?.FirstOrDefault()?.PhoneNumber,
                Email = contactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText
            };
        }

       
        private static IEnumerable<SelectedOffer> GetSelectedOffer(IEnumerable<OfferDetails> offers, Model.OfferRS.DataList dataList)
        {
            return offers.Select(offer =>
                new SelectedOffer()
                {
                    OfferId = offer.OfferId,
                    OfferItems = offer.OfferItems.Select(x =>
                    {
                        return new OfferItem()
                        {
                            OfferItemId = x.OfferItemId,
                            OfferitemPricing = x.OfferitemPricing,
                            Services = GetServices(x.Services, dataList),
                            TicketDesigCode = x.TicketDesigCode,
                        };
                    }).ToList()
                }
            );
        }

        private static IEnumerable<OfferService> GetServices(IEnumerable<OfferService> services, Model.OfferRS.DataList dataList)
        {
            foreach (var service in services)
            {
                service.FlightSegmentsRefIds = service.ProductSegmentAssociation.OfferSegmentRefIds.Select(sv => dataList.OfferSegments.Where(x => x.OfferSegmentId == sv).Select(y => y.FlightSegmentRefId.Replace("F",""))).SelectMany(x => x);
            }

            return services;
        }

    }
}


