using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using Segment = VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment;

namespace VAA.EAPI.NDC.Offer.Validations.SeatAvailability
{
    public interface ISeatAvailabilityValidations<T>
    {
        Response<T> ValidateSeatRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ);

        Response<T> ValidateSeatOrderRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ);

        Response<T> ValidateAgency(ValidationRS validationRS);

        Response<T> ValidateSeatServiceRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ);

        Response<T> ValidateSeatInPNR();

        Response<T> ValidateSeatAvailabilityRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ, OfferResponse offerDoc, string offerId);

        Response<T> ValidateSeatAvailabilitySeatOrderRequest(ValidationRS validationResp, OrderDocument orderDoc);

        IATA_SeatAvailabilityRS MapToErrorSeatAvailabilityRs(ResponseMessage errors);

        Response<T> ValidateEligibleFlights(IEnumerable<FlightSegment> segmentList);

        Response<T> ValidateEligibleVssFlights(IEnumerable<Segment> segmentList);

        Response<T> ValidateSegmentSeats(IEnumerable<FlightSegment> segmentList); 

        Response<T> ValidateHoldBookingPNR(OrderDocument sabreOrderDoc);

        Response<T> ValidateSegmentSeatsWithOrderId(IEnumerable<VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment> segmentList);

        IEnumerable<NDCWarning> PopulateNonVSSegmentWarnings(OfferResponse offerDoc);

        IEnumerable<NDCWarning> PopulateSeatMapResponseWarnings(SeatRS seatMSResponse);

        IEnumerable<NDCWarning> PopulateSeatMapResponseWarningsPostSale(SeatRS seatMSResponse);

        IEnumerable<NDCWarning> PopulateNonVSSegmentWarnings(OrderDocument orderDoc);

        IEnumerable<NDCWarning> PopulateWarningsForPaxWithSeat(OrderDocument orderDoc);
        
        IEnumerable<NDCWarning> PopulateVSSegmentWarnings(OrderDocument orderDoc);

        Response<T> ValidateAlacarteOfferInResponse(IATA_SeatAvailabilityRS seatResponse);

        Response<T> ValidateGDSSegmentSeats(IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> segmentList);

        Response<T> ValidateOrderSearchRSForGdsAgentBooking(Air4OrderSearchMSResponse air4OrderSearchRS, string orgID);

        IEnumerable<NDCWarning> PopulateGDSNonVSSegmentWarnings(OrderSearchResponse orderSearchResponse, IHostEnvironment _hostEnvironment);

        IEnumerable<NDCWarning> PopulateGDSVSSegmentWarnings(OrderSearchResponse orderSearchResponse, IHostEnvironment _hostEnvironment);

        IEnumerable<NDCWarning> PopulateGDSSeatMapResponseWarnings(SeatRS seatMSResponse, IHostEnvironment _hostEnvironment);

        IEnumerable<NDCWarning> PopulateFOPWarning();

    }
}