using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.Apns
{
    public class ApnsResponse
    {
        public ApnsResponseReason Reason { get; }
        public string ReasonString { get; }
        public bool IsSuccessful { get; }

        [JsonConstructor]
        ApnsResponse(ApnsResponseReason reason, string reasonString, bool isSuccessful)
        {
            Reason = reason;
            ReasonString = reasonString;
            IsSuccessful = isSuccessful;
        }

        public static ApnsResponse Successful() => new ApnsResponse(ApnsResponseReason.Success, null, true);

        public static ApnsResponse Error(ApnsResponseReason reason, string reasonString) => new ApnsResponse(reason, reasonString, false);
    }
}
