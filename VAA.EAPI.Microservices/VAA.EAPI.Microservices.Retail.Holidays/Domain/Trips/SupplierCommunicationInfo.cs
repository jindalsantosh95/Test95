using Newtonsoft.Json.Linq;
namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class SupplierCommunicationInfo
    {
        public string? ConfirmationNo { get; set; }

        public static explicit operator SupplierCommunicationInfo?(JToken? source)
        {
            if (source is null) return null;

            return new()
            {
                ConfirmationNo = source.Value<string>("confirmationNo")
            };
        }
    }
}