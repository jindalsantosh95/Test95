using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VAA.EAPI.Domain.Entities.Common
{
    public class NDCWarning
    {
        public enum StatusEnum
        {
            [EnumMember(Value = "NotProcessed")]
            NotProcessed = 1,
            [EnumMember(Value = "Incomplete")]
            Incomplete = 2,
            [EnumMember(Value = "Complete")]
            Complete = 3,
            [EnumMember(Value = "Unknown")]
            Unknown = 4
        }

        public string Status { get; set; }
        public string Type { get; set; }
        public string ShortText { get; set; }
        public string Code { get; set; }
        public string TagText { get; set; }
        public Link DocURL { get; set; }
        public bool IsDynamicMessage { get; set; }
        public IEnumerable<DynamicMessagesContent> DynamicMessagesContents { get; set; }
    }
}
