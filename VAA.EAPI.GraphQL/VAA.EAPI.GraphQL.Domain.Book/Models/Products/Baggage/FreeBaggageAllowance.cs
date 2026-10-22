using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Baggage

{
    public class FreeBaggageAllowance
    {
        public int AllowedBagCnt { get; set; }
        public IEnumerable<BagDescriptor> BagDescriptors { get; set; }
        public Weight Weight { get; set; }
        public IEnumerable<DimensionAllowance> DimensionAllowance { get; set; }
        public IEnumerable<WeightAllowance> WeightAllowance { get; set; }

    }
}
