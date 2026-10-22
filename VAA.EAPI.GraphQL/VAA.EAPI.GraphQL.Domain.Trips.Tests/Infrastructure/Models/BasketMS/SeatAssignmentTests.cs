using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.BasketMS;

public sealed class SeatAssignmentTests
{
    [Fact]
    public void ConvertsSeatAssignmentWhenActualCabinClassIsMissing()
    {
        var source = new SeatAssignment
        {
            Seat = new Seat
            {
                Name = "25A",
                ActualCabinClass = null
            }
        };

        var result = (VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS.SeatAssignment)(source, "2", "passenger-1");

        Assert.Equal("25A", result.SeatNumber);
        Assert.Null(result.SeatClass.CabinCode);
        Assert.Null(result.SeatClass.CabinProductCode);
    }

    [Fact]
    public void ConvertsSeatAssignmentWhenActualCabinClassIsPresent()
    {
        var source = new SeatAssignment
        {
            Seat = new Seat
            {
                Name = "25A",
                ActualCabinClass = [new CabinClass { Code = "Y", ProductCode = "VSCL" }]
            }
        };

        var result = (VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS.SeatAssignment)(source, "2", "passenger-1");

        Assert.Equal("Y", result.SeatClass.CabinCode);
        Assert.Equal("VSCL", result.SeatClass.CabinProductCode);
    }
}
