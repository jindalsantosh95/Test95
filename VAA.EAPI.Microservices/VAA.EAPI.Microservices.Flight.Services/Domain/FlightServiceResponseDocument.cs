using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Flight.Services.Domain
{
    public class FlightServiceResponseDocument : CosmosDocument
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<OfferDetails> Offers { get; set; }
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
