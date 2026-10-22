using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;

public class StoreAppointmentEmailPublisher : IStoreAppointmentEmailPublisher
{
    private readonly IServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

    public StoreAppointmentEmailPublisher(IServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
    {
        _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler;
    }

    public async Task PublishAsync(string appointmentId, AppointmentBookingRequest request, StoreResult? store)
    {
        var serviceBusConnectionString = ConfigReader.Try(Constants.Environment.StoreAppointmentServiceBusConnection, string.Empty);
        var topicName = Constants.Constants.StoreAppointmentTopicName;

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString) || string.IsNullOrWhiteSpace(topicName))
        {
            return;
        }

        var customerEventData = BuildEventData(appointmentId, request.Customer?.Email, Constants.Constants.StoreAppCustomerEmailTemplateId, BuildCustomerXml(request, store, appointmentId));
        await PublishMessage(
            CreateCloudEvent(Constants.Constants.StoreAppointmentEventType, Constants.Constants.StoreAppointmentEventSource, appointmentId, customerEventData),
            serviceBusConnectionString,
            topicName);

        var storeEventData = BuildEventData(appointmentId, ExtractStoreEmailAddress(store), Constants.Constants.StoreAppStoreEmailTemplateId, BuildStoreXml(request, store, appointmentId));
        await PublishMessage(
            CreateCloudEvent(Constants.Constants.StoreAppointmentEventType, Constants.Constants.StoreAppointmentEventSource, appointmentId, storeEventData),
            serviceBusConnectionString,
            topicName);

    }

    private static CloudEvent CreateCloudEvent(string eventType, string eventSource, string appointmentId, StoreAppointmentEmailEventData eventData)
    {
        return new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Source = eventSource,
            Type = eventType,
            Subject = appointmentId.ToString(),
            Time = DateTime.UtcNow.ToString("O"),
            Data = eventData
        };
    }

    private async Task PublishMessage(CloudEvent cloudEvent, string serviceBusConnectionString, string topicName)
    {
        await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
        {
            Message = cloudEvent,
            ServiceBusConnectionString = serviceBusConnectionString,
            ServiceBusTopicName = topicName
        }, CancellationToken.None);
    }

    private static StoreAppointmentEmailEventData BuildEventData(string appointmentId, string? emailAddress, string templateId, string emailDataXml)
    {
        return new StoreAppointmentEmailEventData
        {
            AppointmentReferenceId = appointmentId.ToString(),
            TemplateId = templateId,
            EmailAddress = emailAddress ?? string.Empty,
            EmailDataXml = emailDataXml
        };
    }

    private static string BuildCustomerXml(AppointmentBookingRequest request, StoreResult? store, string appointmentId)
    {
        var address = store?.FullDetails?.Details.Address;
        var root = new XElement("ctx",
            CreateElement("CustomerFirstName", request.Customer?.FirstName),
            CreateElement("StoreName", store?.Name),
            CreateElement("StoreEmailAddress", ExtractStoreEmailAddress(store)),
            CreateElement("CustomerTelephoneNumber", request?.Customer?.Phone),
            CreateElement("StoreAddressLine1", address?.AddressLine1 ?? string.Empty),
            CreateElement("StoreAddressLine2", address?.AddressLine2 ?? string.Empty),
            CreateElement("StoreAddressCity", address?.City ?? string.Empty),
            CreateElement("StoreAddressCounty", address?.County ?? string.Empty),
            CreateElement("StoreAddressPostalCode", address?.PostalCode ?? string.Empty),
            CreateElement("StoreTelephoneNumber", store?.FullDetails?.Details.Phone),
            CreateElement("StoreDirectionsLink", BuildStoreDirectionsLink(request, store)),
            CreateElement("AppointmentType", request.AppointmentType?.ToString()),
            CreateElement("AppointmentReason", request.AppointmentReason),
            CreateElement("ExistingBookingReference", request.BookingReference),
            CreateElement("AppointmentAdditionalDetails", request.AdditionalDetails),
            CreateElement("AppointmentDate", FormatDate(request.Date)),
            CreateElement("AppointmentStartTime", FormatStartTime(request.SlotStart)),
            CreateElement("AddToCalendar", string.Empty),
            CreateElement("LocationTextForAddCalendar", BuildStoreAddress(store)),
            CreateElement("PurposeVisit", request.VisitPurpose),
            CreateElement("PurposeAdditionalDetails", request.VisitPurposeDetails),
            CreateElement("AppointmentReferenceId", appointmentId));

        return root.ToString(SaveOptions.DisableFormatting);
    }

    private static string BuildStoreXml(AppointmentBookingRequest request, StoreResult? store, string appointmentId)
    {
        var root = new XElement("ctx",
            CreateElement("CustomerFirstName", request.Customer?.FirstName),
            CreateElement("CustomerLastName", request.Customer?.LastName),
            CreateElement("CustomerEmailAddress", request.Customer?.Email),
            CreateElement("CustomerPhoneNumber", request.Customer?.Phone),
            CreateElement("AppointmentType", request.AppointmentType?.ToString()),
            CreateElement("AppointmentReason", request.AppointmentReason),
            CreateElement("ExistingBookingReference", request.BookingReference),
            CreateElement("StoreName", store?.Name),
            CreateElement("AppointmentAdditionalDetails", request.AdditionalDetails),
            CreateElement("AppointmentDate", FormatDate(request.Date)),
            CreateElement("AppointmentStartTime", FormatStartTime(request.SlotStart)),
            CreateElement("PurposeVisit", request.VisitPurpose),
            CreateElement("PurposeAdditionalDetails", request.VisitPurposeDetails),
            CreateElement("AppointmentReferenceId", appointmentId));

        return root.ToString(SaveOptions.DisableFormatting);
    }
    private static string BuildStoreAddress(StoreResult? store)
    {
        var address = store?.FullDetails?.Details.Address;
        var addressLines = address is null
            ? Array.Empty<string>()
            : new[]
            {
                address.AddressLine1,
                address.AddressLine2,
                address.City,
                address.County,
                address.PostalCode,
                address.CountryCode
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        return string.Join(", ", addressLines);
    }
    private static string ExtractStoreEmailAddress(StoreResult? store)
    {
        return store?.FullDetails?.Details.EmailAddress ?? string.Empty;
    }

    private static string BuildStoreDirectionsLink(AppointmentBookingRequest request, StoreResult? store)
    {
        var destination = BuildDestination(store);
        if (string.IsNullOrWhiteSpace(destination))
        {
            return string.Empty;
        }

        var queryParameters = new List<string>
        {
            "api=1",
            $"destination={Uri.EscapeDataString(destination)}"
        };

        var origin = request.SearchParameter?.Trim();
        if (!string.IsNullOrWhiteSpace(origin))
        {
            queryParameters.Add($"origin={Uri.EscapeDataString(origin)}");
        }

        return $"{ConfigReader.Try(Constants.Environment.GoogleMapsDirectionsBaseUrl, Constants.Constants.DefaultDirectionsBaseUrl)}?{string.Join("&", queryParameters)}";
    }

    private static string BuildDestination(StoreResult? store)
    {
        if (store is null)
        {
            return string.Empty;
        }

        var destination = string.Join(", ", new[] { store.Label, BuildStoreAddress(store), store.Name }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

        if (!string.IsNullOrWhiteSpace(destination))
        {
            return destination;
        }

        return store.Name;
    }

    private static string FormatDate(DateOnly? date)
    {
        return date?.ToString("dd MMMM yyyy") ?? string.Empty;
    }

    private static string FormatStartTime(DateTimeOffset? startTime)
    {
        if (startTime is null)
        {
            return string.Empty;
        }

        return BritishTimeZoneHelper.ConvertToUkLocal(startTime.Value).ToString("HH:mm");
    }

    private static XElement CreateElement(string name, string? value)
    {
        return new XElement(name, value ?? string.Empty);
    }
}
