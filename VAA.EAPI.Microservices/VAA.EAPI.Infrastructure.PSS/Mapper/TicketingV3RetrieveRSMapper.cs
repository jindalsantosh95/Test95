using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class TicketingV3RetrieveRSMapper
    {
        public static TicketingV3RetrieveResponse MapToTicketingV3RetrieveResponse(RetrieveResponseType retrieveResponseType)
        {
            if (retrieveResponseType != null && retrieveResponseType.TicketRequestResponse != null)
            {
                return new TicketingV3RetrieveResponse()
                {
                    TicketingV3RequestResponseType = TicketingV3RequestResponseTypeMapper(retrieveResponseType)
                };
            }
            return null;
        }

        public static TicketingV3RequestResponseType[] TicketingV3RequestResponseTypeMapper(RetrieveResponseType retrieveResponseType)
        {
            List<TicketingV3RequestResponseType> ticketingV3RequestResponseTypes = new();
            foreach (var response in retrieveResponseType.TicketRequestResponse)
            {
                ticketingV3RequestResponseTypes.Add(new TicketingV3RequestResponseType()
                {
                    TicketingV3RequestType = TicketingV3RequestTypeMapper(response),
                    TicketingV3ResponseType = TicketingV3ResponseTypeMapper(response)
                });
            }

            return ticketingV3RequestResponseTypes.ToArray();
        }

        public static TicketingV3RequestType TicketingV3RequestTypeMapper(TicketRequestResponseType retrieveResponseType)
        {
            if (retrieveResponseType.TicketRequest != null)
            {
                return new TicketingV3RequestType()
                {
                    ElectronicDocumentNumber = retrieveResponseType.TicketRequest.ElectronicDocumentNumber
                };
            }
            return null;
        }

        public static TicketingV3ResponseType[] TicketingV3ResponseTypeMapper(TicketRequestResponseType retrieveResponseType)
        {
            List<TicketingV3ResponseType> ticketingV3Responses = new();
            foreach (var response in retrieveResponseType.TicketResponse)
            {
                ticketingV3Responses.Add(new TicketingV3ResponseType()
                {
                    TicketInformation = new()
                    {
                        BaseFare = response?.TicketInformation?.BaseFare,
                        CurrencyCodeOfBaseFare = response?.TicketInformation?.CurrencyCodeOfBaseFare,
                        TotalFare = response?.TicketInformation?.TotalFare,
                        CurrencyCodeOfTotalFare = response?.TicketInformation?.CurrencyCodeOfTotalFare
                    },
                    AdditionalTicketLevelData = AdditionalTicketLevelDataMapper(response),
                    DTITaxItems = DTITaxItemMapper(response),
                    TotalTax = new()
                    {
                        TotalTaxAmount = response?.TotalTaxAmount?.TotalTaxAmount,
                        TotalTaxCurrencyCode = response?.TotalTaxAmount?.TotalTaxCurrencyCode
                    }
                });
            }
            return ticketingV3Responses.ToArray();
        }

        public static DTITaxItem[] DTITaxItemMapper(TicketResponseType ticketingV3ResponseType)
        {
            if (ticketingV3ResponseType.DTITaxItem != null)
            {
                List<DTITaxItem> dTITaxItems = new();
                foreach (var item in ticketingV3ResponseType.DTITaxItem)
                {
                    dTITaxItems.Add(new DTITaxItem()
                    {
                        TaxAmount = item.TaxAmount,
                        TaxCode = item.TaxCode,
                        TaxAmountCurrencyCode = item.TaxAmountCurrencyCode
                    });
                }

                return dTITaxItems.ToArray();
            }
            return null;
        }

        public static AdditionalTicketLevelData[] AdditionalTicketLevelDataMapper(TicketResponseType ticketingV3ResponseType)
        {
            if (ticketingV3ResponseType.AdditionalTicketLevelData != null)
            {
                List<AdditionalTicketLevelData> additionalTicketLevelData = new();
                foreach (var data in ticketingV3ResponseType.AdditionalTicketLevelData)
                {
                    additionalTicketLevelData.Add(new AdditionalTicketLevelData()
                    {
                        CustomerId = data.CustomerId,
                        PassengerTypeCode = data.PassengerTypeCode
                    });
                }

                return additionalTicketLevelData.ToArray();
            }
            return null;
        }
    }
}
