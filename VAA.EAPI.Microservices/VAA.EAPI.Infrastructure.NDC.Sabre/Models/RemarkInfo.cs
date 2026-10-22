using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class RemarkInfo
    {
        [JsonProperty(PropertyName = "FOP_Remark")]
        public IEnumerable<FOPRemark> FOPRemark { get; set; }
    }
}
