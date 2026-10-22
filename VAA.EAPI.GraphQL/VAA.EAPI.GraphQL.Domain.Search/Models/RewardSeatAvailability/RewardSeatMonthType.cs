using HotChocolate.Types;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    public sealed class RewardSeatMonthType : EnumType<RewardSeatMonth>
    {
        protected override void Configure(IEnumTypeDescriptor<RewardSeatMonth> descriptor)
        {
            descriptor.Name("RewardSeatMonth");
            descriptor.Value(RewardSeatMonth.January).Name("JANUARY");
            descriptor.Value(RewardSeatMonth.February).Name("FEBRUARY");
            descriptor.Value(RewardSeatMonth.March).Name("MARCH");
            descriptor.Value(RewardSeatMonth.April).Name("APRIL");
            descriptor.Value(RewardSeatMonth.May).Name("MAY");
            descriptor.Value(RewardSeatMonth.June).Name("JUNE");
            descriptor.Value(RewardSeatMonth.July).Name("JULY");
            descriptor.Value(RewardSeatMonth.August).Name("AUGUST");
            descriptor.Value(RewardSeatMonth.September).Name("SEPTEMBER");
            descriptor.Value(RewardSeatMonth.October).Name("OCTOBER");
            descriptor.Value(RewardSeatMonth.November).Name("NOVEMBER");
            descriptor.Value(RewardSeatMonth.December).Name("DECEMBER");
        }
    }
}
