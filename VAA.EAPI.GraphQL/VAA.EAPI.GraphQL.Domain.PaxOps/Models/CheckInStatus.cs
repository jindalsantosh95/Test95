namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    public class OnlineCheckInStatus
    {
        public bool Available { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public string DeveloperMessage { get; set; }
        public string DisplayCode { get; set; }
        public string Category { get; set; }

        public static explicit operator OnlineCheckInStatus(Infrastructure.Models.CheckInMS.OnlineCheckInStatus source) => new ()
        {
            Available = source.Available,
            Title = source.Title,
            Message = source.Message,
            Code = source.Code,
            DeveloperMessage = source.DeveloperMessage,
        };

        public static explicit operator OnlineCheckInStatus(CheckInError source)
        {
            return new OnlineCheckInStatus
            {
                Available = false,
                Message = source.Message,
                Code = source.Code,
                Title = source.Title,
                DisplayCode = source.DisplayCode,
                Category = source.Category
            };
        }          
    }
}
