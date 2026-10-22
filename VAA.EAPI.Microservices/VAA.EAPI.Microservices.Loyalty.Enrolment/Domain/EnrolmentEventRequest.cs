using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain
{
    public class EnrolmentEventRequest : IRequest<Result<Response<string>>>
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
