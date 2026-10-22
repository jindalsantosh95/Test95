namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{

    public class DocumentLinkError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }

        public static explicit operator DocumentLinkError(VAA.EAPI.Domain.Entities.Common.ErrorDetails source)
        {
            if (source == null) return null;

            return new()
            {
                Code = source.Code,
                Message = source.Message,
                DeveloperMessage = source.DeveloperMessage,
                ErrorSource = source.ErrorSource
            };
        }
    }
}
