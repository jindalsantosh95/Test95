using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{

    public class Warning
    {

        public string Type { get; set; }
        public string ShortText { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public string TagText { get; set; }
        public Link DocURL { get; set; }
        public bool IsDynamicMessage { get; set; }
        public IEnumerable<DynamicMessagesContent> DynamicMessagesContents { get; set; }


    }
}
