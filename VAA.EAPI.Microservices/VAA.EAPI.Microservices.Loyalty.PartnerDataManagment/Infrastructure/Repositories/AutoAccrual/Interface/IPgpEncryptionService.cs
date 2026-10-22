namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface
{
    public interface IPgpEncryptionService
    {
        string Encrypt(string json, string publicKeyContent);
    }
}
