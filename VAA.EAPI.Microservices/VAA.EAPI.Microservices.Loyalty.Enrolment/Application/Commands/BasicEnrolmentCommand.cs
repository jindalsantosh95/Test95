using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands
{
    public class BasicEnrolmentCommand : IRequest<Result<Response<string>>>
    {
        public BasicEnrolmentCommand(string channelId, EnrolmentRequest enrolmentRequest)
        {
            ChannelId = channelId;
            EnrolmentRequest = enrolmentRequest;
        }
        public string ChannelId { get; set; }
        public EnrolmentRequest EnrolmentRequest { get; set; }
    }
}
