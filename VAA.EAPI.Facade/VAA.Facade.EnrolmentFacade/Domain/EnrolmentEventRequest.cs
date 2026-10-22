using CSharpFunctionalExtensions;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;

namespace VAA.Facade.EnrolmentFacade.Domain
{
    public partial class EnrolmentEventRequest
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public string Id { get; set; }
        public string Time { get; set; }
        public string Datacontenttype { get; set; }
        public EnrolmentRequest Data { get; set; }
    }
}
