using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public IEnumerable<MoreInfo> MoreInfo { get; set; }
        public ErrorCategoryType Category { get; set; }

        public static explicit operator Error(DeltaPaxInfo.Error source)
        {
            if (source is null) return null;

            return new Error()
            {
                Code = source.Code,
                Message = source.Message,
                DeveloperMessage = source.DeveloperMessage,
                MoreInfo = source.MoreInfo?.Select(x => (MoreInfo)x),
                Category = (ErrorCategoryType)source.Category
            };
        }
    }
}