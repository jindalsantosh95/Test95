using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain.Trips
{

    public class SSRMealDetail
    {
        public string Category { get; set; }
        public string CategoryTitle { get; set; }
        public IEnumerable<CategoryDetail> CategoryDetails { get; set; }
    }


}
