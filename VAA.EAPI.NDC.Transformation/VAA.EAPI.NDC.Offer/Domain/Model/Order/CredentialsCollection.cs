using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class CredentialsCollection
    {
        public string CustomerId { get; set; }
        public IEnumerable<Credential> Credentials { get; set; }
    }
}
