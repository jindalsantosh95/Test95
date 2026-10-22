using System;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.StoreAppointment
{
    public class StoreAppointmentEmailEventProcessCommand : CloudEvent, IRequest<bool>
    {
        public static StoreAppointmentEmailEventProcessCommand FromCloudEvent(CloudEvent emailRequestEventMessage)
        {
            ArgumentNullException.ThrowIfNull(emailRequestEventMessage);

            return new StoreAppointmentEmailEventProcessCommand
            {
                Id = emailRequestEventMessage.Id,
                Data = emailRequestEventMessage.Data,
                Type = emailRequestEventMessage.Type,
                Subject = emailRequestEventMessage.Subject,
                Specversion = emailRequestEventMessage.Specversion
            };
        }
    }
}
