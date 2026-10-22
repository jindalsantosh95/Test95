using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command
{
    public class OrderEmailCommand : IRequest<Unit>
    {
        public string Email { get; set; }
        public string RecordLocator { get; set; }
    }
}
