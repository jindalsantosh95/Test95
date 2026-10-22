using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface ISeatUpgradeHandler
    {
        Task<bool> CheckSeatCabinChange(Basket basket, SeatSelectionRequest request, SeatResponse seatMapResponse);

        Task<bool> IsSeatUpgradeConfirmationValid(Basket basket, IEnumerable<SeatResponse> seatMSResponses);

        Task<Warning> AddErrorForInvalidUpgradeBasket(Basket basket);        
    }
}
