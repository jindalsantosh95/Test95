using System.ComponentModel.DataAnnotations;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public sealed class SvcOrderLookupRequest
    {
        [Required]
        public string Pnr { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
