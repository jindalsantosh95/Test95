using Azure.Storage.Blobs.Models;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class PaxNameChange
    {
        public string GivenName { get; set; }
        public string PaxRefID { get; set; }
        public string Surname { get; set; }

    }
}