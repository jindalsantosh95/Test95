using MediatR;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Helper
{
    public class DocumentHelper
    {
        public static async Task SaveSeatOffers(SeatAvailabilityResponse seatAvailabilityResponse, string docId, IMediator mediator, string docType, string channel, string command = "create")
        {
            // await mediator.Send(new CosmosRepoCommand(SeatAvailabilityResponseToRedisCacheMapper.DocumentMapper(seatAvailabilityResponse, docId, docType, channel)));
        }
    }
}