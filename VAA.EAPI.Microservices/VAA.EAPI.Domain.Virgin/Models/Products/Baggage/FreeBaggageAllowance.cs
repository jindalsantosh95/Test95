using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Baggage

{
    public class FreeBaggageAllowance
    {
        public int? AllowedBagCnt { get; set; }
        public int WeightInKilos { get; set; }
        public List<BagDescriptor> BagDescriptors { get; set; }
        public List<DimensionAllowance> DimensionAllowance { get; set; }
        public List<WeightAllowance> WeightAllowance { get; set; }

    }
}
