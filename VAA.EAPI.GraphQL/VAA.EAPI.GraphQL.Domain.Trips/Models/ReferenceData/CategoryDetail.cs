using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData
{
    [GraphQLName("SSRMealCategoryDetail")]
    public class CategoryDetail
    {
        public string VsCode { get; set; }
        public string DlCode { get; set; }
        public string DlRemark { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Features { get; set; }
    }
}