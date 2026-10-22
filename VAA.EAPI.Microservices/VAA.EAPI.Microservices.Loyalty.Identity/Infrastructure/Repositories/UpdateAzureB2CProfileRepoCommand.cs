using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateAzureB2CProfileRepoCommand 
    {
        public UpdateAzureB2CProfileRequest updateAzureB2CProfileRequest { get; set; }

        public UpdateAzureB2CProfileRepoCommand(UpdateAzureB2CProfileRequest updateDocument)
        {
            updateAzureB2CProfileRequest = updateDocument;
        }
    }
}
