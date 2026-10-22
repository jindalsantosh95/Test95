using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain.Trips
{
    public class CategoryDetail
    {
        public string VsCode { get; set; }
        public string DlCode { get; set; }
        public string DlRemark {  get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Features { get; set; }
    }
}