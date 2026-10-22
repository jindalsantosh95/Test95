using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;

namespace VAA.EAPI.GraphQL.Domain.Trips.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class HolidayPaymentMutation
    {
        private readonly IPaymentServiceProvider _paymentServiceProvider;
        private readonly ILogger<HolidayPaymentMutation> _logger;

        public HolidayPaymentMutation(IPaymentServiceProvider paymentServiceProvider, ILogger<HolidayPaymentMutation> logger)
        {
            _paymentServiceProvider = paymentServiceProvider ?? throw new ArgumentNullException(nameof(paymentServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("holidayPaymentMutation")]
        [GraphQLDescription("Calls Order Payment microservice holiday/payment endpoint and returns payment links.")]
        public async Task<HolidayPaymentResponse> HolidayPayment(HolidayPaymentRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                ArgumentException.ThrowIfNullOrEmpty(request.TripResourceId);
                ArgumentException.ThrowIfNullOrEmpty(request.ReturnUrl);
                ArgumentException.ThrowIfNullOrEmpty(request.ExpiryUrl);
                ArgumentException.ThrowIfNullOrEmpty(request.PayPalClientUrl);
                ArgumentException.ThrowIfNullOrEmpty(request.ContextPath);

                if (request.AmountToPay is not null && request.AmountToPay <= 0m)
                    throw new ArgumentOutOfRangeException(nameof(request.AmountToPay), request.AmountToPay, "amountToPay must be greater than 0 when provided.");

                var bookingReferenceTask = BookingIdHelper.GetMSBookingId(request.TripResourceId);
                var surnameTask = BookingIdHelper.GetSurName(request.TripResourceId);              
                var dateOfBirthTask = BookingIdHelper.GetDOB(request.TripResourceId);

                await Task.WhenAll(bookingReferenceTask, surnameTask, dateOfBirthTask);

                var bookingReference = bookingReferenceTask.Result;
                var surname = surnameTask.Result;
                var dateOfBirth = dateOfBirthTask.Result;

                if (bookingReference.IsNullOrWhiteSpace())
                    throw new ArgumentException("TripResourceId does not contain a valid booking reference.", nameof(request.TripResourceId));

                if (surname.IsNullOrWhiteSpace())
                    throw new ArgumentException("TripResourceId does not contain a valid surname.", nameof(request.TripResourceId));
               
                if (dateOfBirth.IsNullOrWhiteSpace())
                    throw new ArgumentException("TripResourceId does not contain a valid date of birth.", nameof(request.TripResourceId));

                var msResponse = await _paymentServiceProvider.HolidayPaymentAsync(new HolidayPaymentRequestMs
                {
                    BookingReference = bookingReference,                   
                    Surname = surname,                   
                    DateOfBirth = dateOfBirth,
                    AmountToPay = request.AmountToPay,
                    ReturnUrl = request.ReturnUrl,
                    ExpiryUrl = request.ExpiryUrl,
                    PayPalClientUrl = request.PayPalClientUrl,
                    ContextPath = request.ContextPath
                });

                return (HolidayPaymentResponse)msResponse;
            }            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Correlation Id - {CorrelationId} - Channel - {Channel} Holiday Payment Failed.", AsyncContext.Current.CorrelationId, string.Join(',', AsyncContext.Current.Channel));
                throw new Exception(Constants.ErrorCodes.HolidayPaymentErrorCode, ex);
            }
        }        
    }
}
