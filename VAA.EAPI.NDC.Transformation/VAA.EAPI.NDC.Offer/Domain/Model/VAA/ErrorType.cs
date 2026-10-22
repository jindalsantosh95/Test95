using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{
    public class ErrorType
    {
        public string Code { get; set; }
        public string DescText { get; set; }
        public string LangCode { get; set; }
        public string TypeCode { get; set; }
        public string TagText { get; set; }
        public string Url { get; set; }

        public static explicit operator ErrorType(ErrorDetails error)
        {
            return new ErrorType
            {
                Code = error.Code,
                DescText = error.Message,
                LangCode = Constants.LangCode,
                TypeCode = error.StatusText,
                TagText = error.TagText,
                Url = error?.Url
            };
        }
    }
}
