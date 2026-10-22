namespace VAA.EAPI.GraphQL.Domain.Search.Models.Reference
{
    public class CabinAllowance
    {
        public IEnumerable<string>? FareFamilies { get; set; }
        public CarryOnAndCheckedBag? CarryOnBag { get; set; }
        public CarryOnAndCheckedBag? CheckedBag { get; set; }
    }
}
