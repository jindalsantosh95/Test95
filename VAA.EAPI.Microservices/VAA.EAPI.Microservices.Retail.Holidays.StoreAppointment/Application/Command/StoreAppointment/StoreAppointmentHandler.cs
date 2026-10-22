using System.Linq;
using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;

public class StoreAppointmentHandler : IStoreAppointmentHandler
{
    private readonly IStoreAppointmentRepository _storeAppointmentRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IStoreAppointmentEmailPublisher _storeAppointmentCrmPublisher;

    public StoreAppointmentHandler(IStoreAppointmentRepository storeAppointmentRepository, IStoreRepository storeRepository, IStoreAppointmentEmailPublisher storeAppointmentCrmPublisher)
    {
        _storeAppointmentRepository = storeAppointmentRepository;
        _storeRepository = storeRepository;
        _storeAppointmentCrmPublisher = storeAppointmentCrmPublisher;
    }

    public async Task<Result<Response<AppointmentBookingResponse>>> SaveAppointmentAsync(AppointmentBookingRequest request)
    {
        ValidateRequest(request);
        var normalizedSlotStart = BritishTimeZoneHelper.ConvertToUkLocal(request.SlotStart!.Value);
        var normalizedSlotEnd = BritishTimeZoneHelper.ConvertToUkLocal(request.SlotEnd!.Value);
        var bookingDate = ValidateAndResolveBookingDate(request.Date!.Value, normalizedSlotStart, normalizedSlotEnd);

        var store = await _storeRepository.GetStoreByIdAsync(request.LocationId!.Trim());
        var storeInitials = BuildStoreInitials(store, request.LocationId);
        var latestAppointmentReferenceId = await _storeAppointmentRepository.GetLatestAppointmentReferenceIdAsync(storeInitials);
        var appointmentId = BuildAppointmentReferenceId(storeInitials, latestAppointmentReferenceId);

        var appointment = new Appointment
        {
            Id = appointmentId,
            AppointmentType = request.AppointmentType ?? AppointmentType.InStore,
            AppointmentReason = request.AppointmentReason ?? string.Empty,
            BookingReference = request.BookingReference ?? string.Empty,
            Date = bookingDate,
            SlotStart = TimeOnly.FromDateTime(normalizedSlotStart.DateTime),
            SlotEnd = TimeOnly.FromDateTime(normalizedSlotEnd.DateTime),
            LocationId = request.LocationId ?? string.Empty,
            VisitPurpose = request.VisitPurpose ?? string.Empty,
            VisitPurposeDetails = request.VisitPurposeDetails ?? string.Empty,
            AdditionalDetails = request.AdditionalDetails ?? string.Empty
        };

        var saveResult = await _storeAppointmentRepository.AddAsync(appointment);
        if (!saveResult.IsSuccess)
        {
            var message = string.IsNullOrWhiteSpace(saveResult.ErrorMessage)
                ? "Requested slot is no longer available."
                : saveResult.ErrorMessage;

            return Result.Success(new Response<AppointmentBookingResponse>
            {
                Status = Status.BadRequest,
                Errors =
                [
                    new ErrorDetails
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = string.IsNullOrWhiteSpace(saveResult.ErrorCode) ? "STORE_SLOT_UNAVAILABLE" : saveResult.ErrorCode,
                        Message = message,
                        ErrorSource = nameof(StoreAppointmentHandler),
                        DeveloperMessage = $"StoreId: {request.LocationId}, Date: {bookingDate:yyyy-MM-dd}, SlotStart: {appointment.SlotStart:HH\\:mm}, SlotEnd: {appointment.SlotEnd:HH\\:mm}"
                    }
                ],
                IgnoreNullValues = false
            });
        }

        var normalizedRequest = BuildNormalizedRequest(request, bookingDate, normalizedSlotStart, normalizedSlotEnd);
        await _storeAppointmentCrmPublisher.PublishAsync(appointmentId, normalizedRequest, store);

        var response = new Response<AppointmentBookingResponse>
        {
            Status = Status.OK,
            Value = new AppointmentBookingResponse
            {
                AppointmentReferenceId = appointmentId,
                Date = bookingDate,
                Store = store?.Name ?? request.LocationId ?? string.Empty,
                SlotDetails = BuildSlotDetails(normalizedSlotStart, normalizedSlotEnd)
            }
        };

        return Result.Success(response);
    }

    private static void ValidateRequest(AppointmentBookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LocationId))
        {
            throw new ArgumentException("LocationId is required.", nameof(request.LocationId));
        }

        if (request.Date is null)
        {
            throw new ArgumentException("Date is required.", nameof(request.Date));
        }

        if (request.SlotStart is null || request.SlotEnd is null)
        {
            throw new ArgumentException("SlotStart and SlotEnd are required.", nameof(request));
        }

        if (request.Customer is null || string.IsNullOrWhiteSpace(request.Customer.Email))
        {
            throw new ArgumentException("Customer.Email is required.", nameof(request.Customer));
        }
    }

    private static string BuildStoreInitials(StoreResult? store, string? locationId)
    {
        var candidate = store?.StoreId;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = locationId;
        }

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return "NA";
        }

        var letters = new string(candidate
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (letters.Length >= 2)
        {
            return letters[..2];
        }

        return letters.PadRight(2, 'X');
    }

    private static string BuildAppointmentReferenceId(string storeInitials, string? latestAppointmentReferenceId)
    {
        const int defaultSequence = 1;
        var nextSequence = defaultSequence;

        if (!string.IsNullOrWhiteSpace(latestAppointmentReferenceId))
        {
            var lastIndex = latestAppointmentReferenceId.LastIndexOf('-');
            if (lastIndex >= 0
                && lastIndex < latestAppointmentReferenceId.Length - 1
                && int.TryParse(latestAppointmentReferenceId[(lastIndex + 1)..], out var parsedSequence)
                && parsedSequence >= 0)
            {
                nextSequence = parsedSequence + 1;
            }
        }

        var normalizedStoreInitials = string.IsNullOrWhiteSpace(storeInitials)
            ? "XX"
            : storeInitials.Trim().ToUpperInvariant();

        return $"BR-{normalizedStoreInitials}-{nextSequence:D6}";
    }

    private static AppointmentBookingRequest BuildNormalizedRequest(
        AppointmentBookingRequest request,
        DateOnly bookingDate,
        DateTimeOffset normalizedSlotStart,
        DateTimeOffset normalizedSlotEnd)
    {
        return new AppointmentBookingRequest
        {
            AppointmentType = request.AppointmentType,
            AppointmentReason = request.AppointmentReason,
            BookingReference = request.BookingReference,
            Date = bookingDate,
            SlotStart = normalizedSlotStart,
            SlotEnd = normalizedSlotEnd,
            LocationId = request.LocationId,
            VisitPurpose = request.VisitPurpose,
            VisitPurposeDetails = request.VisitPurposeDetails,
            AdditionalDetails = request.AdditionalDetails,
            SearchParameter = request.SearchParameter,
            Customer = request.Customer
        };
    }

    private static StoreSlot BuildSlotDetails(DateTimeOffset slotStart, DateTimeOffset slotEnd)
    {
        return new StoreSlot
        {
            StartTime = ParseDateTime(slotStart),
            EndTime = ParseDateTime(slotEnd),
            IsAvailable = true
        };
    }

    private static DateTimeOffset ParseDateTime(DateTimeOffset slot)
    {
        return BritishTimeZoneHelper.ConvertToUkLocal(slot);
    }

    private static DateOnly ValidateAndResolveBookingDate(DateOnly requestedDate, DateTimeOffset normalizedSlotStart, DateTimeOffset normalizedSlotEnd)
    {
        var slotStartDate = DateOnly.FromDateTime(normalizedSlotStart.Date);
        var slotEndDate = DateOnly.FromDateTime(normalizedSlotEnd.Date);

        if (slotStartDate != slotEndDate)
        {
            throw new ArgumentException("SlotStart and SlotEnd must resolve to the same UK-local date.");
        }

        if (requestedDate != slotStartDate)
        {
            throw new ArgumentException("Date must match the UK-local date of SlotStart and SlotEnd.", nameof(requestedDate));
        }

        if (normalizedSlotEnd <= normalizedSlotStart)
        {
            throw new ArgumentException("SlotEnd must be later than SlotStart.");
        }

        return slotStartDate;
    }
}
