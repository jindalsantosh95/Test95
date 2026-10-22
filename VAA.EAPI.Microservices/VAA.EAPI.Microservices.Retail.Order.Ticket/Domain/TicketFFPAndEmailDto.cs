using System;
using System.Linq;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain
{
    public class TicketFFPAndEmailDto : CosmosDocument
    {
        public string DocType { get; set; }
        public string TicketNumber { get; set; }
        public string ContactEmail { get; set; }
        public string FFPNumber { get; set; }
        public string FFPCarrierCode { get; set; }
        public string LastUpdatedOn { get; set; }
        public string SchemaVersion { get; set; }

        public TicketFFPAndEmailDto()
        {
            // Required for serialisation
        }

        public static explicit operator TicketFFPAndEmailDto(TicketDto ticketDto) => new()
        {
            Id = ticketDto.Id,
            PartitionKey = DocumentType.TicketFFPAndEmail.ToString(),
            SchemaVersion = Constants.Application.SchemaVersion,
            DocType = DocumentType.TicketFFPAndEmail.ToString(),
            LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
            TicketNumber = ticketDto.Document.TktDocNb,
            FFPNumber = ticketDto.Coupons?.First().EtCpnSt?.First().FFNb,
            FFPCarrierCode = ticketDto.Coupons?.First().EtCpnSt?.First().FFCrrCd,
            ContactEmail = ticketDto.DocumentElectronic?.First().ElectronicAddresses?.First().ElcrAdr,
        };
    }
}
