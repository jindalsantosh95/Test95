using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class UpsellOptions
    {
        public object Selected { get; set; }
        public IEnumerable<object> Options { get; set; }
        public string Type { get; set; }
    }
}
