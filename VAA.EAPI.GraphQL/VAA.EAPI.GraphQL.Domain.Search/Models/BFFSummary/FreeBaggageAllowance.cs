using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("FreeBaggageAllowance_BFF")]
    public class FreeBaggageAllowance
    {
        public int AllowedBagCnt { get; set; }
        public Weight? Weight { get; set; }
        public DimensionAllowance? DimensionAllowance { get; set; }

        public static explicit operator FreeBaggageAllowance(FreeBag? source)
        {
            if (source is null || source.Pieces == 0)
                return null;

            return new FreeBaggageAllowance()
            {
                AllowedBagCnt = source?.Pieces ?? 0,
                Weight = (Weight?)source?.Weight,
                DimensionAllowance = (DimensionAllowance?)source?.Size
            };
        }
    }
}
