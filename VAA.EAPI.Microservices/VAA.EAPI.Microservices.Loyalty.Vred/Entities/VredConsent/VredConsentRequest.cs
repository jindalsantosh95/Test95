using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredConsent
{
    internal class VredConsentRequest
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }
        public string RequestInformation { get; set; }
        [JsonProperty("purposes")]
        public IEnumerable<Purpose> Purposes { get; set; }
        [JsonProperty("dsDataElements")]
        public DSDataElements DSDataElements { get; set; }
    }
}
