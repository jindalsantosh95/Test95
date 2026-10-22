namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface
{
    public interface ISynchronyPayloadCryptoService
    {
        string EncryptData(string data);
    }
}
