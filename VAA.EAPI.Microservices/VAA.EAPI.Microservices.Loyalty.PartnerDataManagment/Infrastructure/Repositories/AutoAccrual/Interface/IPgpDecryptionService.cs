namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface
{
    public interface IPgpDecryptionService
    {
        string Decrypt(string encryptedPayload, string privateKey);
    }
}

