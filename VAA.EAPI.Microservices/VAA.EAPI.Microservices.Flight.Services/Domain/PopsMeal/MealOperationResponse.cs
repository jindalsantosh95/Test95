using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal
{
    public class MealOperationResponse
    {
        public string RecordLocator { get; set; }
        public IEnumerable<FlightSegment> Segments { get; set; }
        
        public static explicit operator MealOperationResponse(MealOrderRequest request)
        {
            return new MealOperationResponse
            {
                RecordLocator = request.RecordLocator,
                Segments = request.Segments
            };
        }
    }

}