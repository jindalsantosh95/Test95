using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class APIErrorResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string DisplayCode { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }    
        public IEnumerable<APIErrorBatch> BatchResults { get; set; }

        public static explicit operator APIErrorResponse(ErrorDetails errorDetails)
        {
            return new() { Code = errorDetails.Code, Title = errorDetails.Title, Message = errorDetails.Message };
        }

        public static explicit operator ErrorDetails(APIErrorResponse response)
        {
            return new() { Code = response.Code, Title = response.Title, Message = response.Message };
        }
    }
}
