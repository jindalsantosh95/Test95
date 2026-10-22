using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{
    public class WarningType
    {
        public string Code { get; set; }

        public string DescText { get; set; }

        public string LangCode { get; set; }

        public string TypeCode { get; set; }

        public string TagText { get; set; }


        public static explicit operator WarningType(NDCWarning warning)
        {
            return new WarningType
            {
                Code = warning.Code,
                DescText = warning.ShortText,
                LangCode = Constants.LangCode,
                TypeCode = warning.Type,
                TagText = warning.TagText
            };
        }
    }
}
