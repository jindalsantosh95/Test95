using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using AutoAccrualTerms = VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.AutoAccrualPartnerTerms;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class AutoAccrualReferenceValidationServiceTests
    {
        private static AutoAccrualRequest Request(string date = null) => new()
        {
            Producer = "SK",
            ScheduledDepartureDate = date ?? DateTime.Today.ToString("yyyy-MM-dd"),
            FlightSegment = new FlightSegment { Origin = "LHR", Destination = "ARN" },
            FlightDetails = new FlightDetails { OperatingFlight = new Flight { OperatingCarrierCode = "SK" }, MarketingFlight = new Flight { MarketingCarrierCode = "SK" } },
            BookingDetails = new BookingDetails { OperatingCarrierBookingClass = "Y" }
        };

        [Fact]
        public async Task ValidateAsync_WhenAllChecksPass_Completes()
        {
            await new AutoAccrualReferenceValidationService(new FakeReferenceDataService()).ValidateAsync(Request(), CancellationToken.None);
        }

        [Fact]
        public async Task ValidateAsync_WhenIneligibleClassFails_Rejects()
        {
            var reference = new FakeReferenceDataService { IneligibleClassResult = 0 };
            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() => new AutoAccrualReferenceValidationService(reference).ValidateAsync(Request(), CancellationToken.None));
            Assert.Contains("Ineligible booking class", exception.Message);
            Assert.DoesNotContain(Constants.Application.GetNonAccrualClassValidity, reference.CalledProcedures);
        }

        [Fact]
        public async Task ValidateAsync_WhenAgreementDateIsInvalid_Rejects()
        {
            var reference = new FakeReferenceDataService { PartnerTerms = new AutoAccrualTerms { AgreementStartDate = DateTime.Today.AddDays(1), RetroClaimPeriod = 183 } };
            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() => new AutoAccrualReferenceValidationService(reference).ValidateAsync(Request(), CancellationToken.None));
            Assert.Contains("Invalid agreement date", exception.Message);
            Assert.Equal(Constants.Application.AutoAccrualInvalidScheduledDepartureDate, exception.ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenRetroPeriodExceeded_Rejects()
        {
            var reference = new FakeReferenceDataService { PartnerTerms = new AutoAccrualTerms { AgreementStartDate = DateTime.Today.AddYears(-1), RetroClaimPeriod = 1 } };
            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() => new AutoAccrualReferenceValidationService(reference).ValidateAsync(Request(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd")), CancellationToken.None));
            Assert.Contains("Retro claim period exceeded", exception.Message);
            Assert.Equal(Constants.Application.AutoAccrualInvalidScheduledDepartureDate, exception.ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenAirlineActivityDateIsInvalid_RejectsWithScheduledDepartureDateErrorCode()
        {
            var reference = new FakeReferenceDataService { AirlineActivityDateResult = 0 };

            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() =>
                new AutoAccrualReferenceValidationService(reference).ValidateAsync(Request(), CancellationToken.None));

            Assert.Contains("Invalid airline activity date", exception.Message);
            Assert.Equal(Constants.Application.AutoAccrualInvalidScheduledDepartureDate, exception.ErrorCode);
        }

        private sealed class FakeReferenceDataService : ICommonRefDBServices
        {
            public int IneligibleClassResult { get; init; } = 1;
            public int NonAccrualClassResult { get; init; }
            public int AirlineActivityDateResult { get; init; } = 1;
            public AutoAccrualTerms PartnerTerms { get; init; } = new() { AgreementStartDate = DateTime.Today.AddYears(-1), RetroClaimPeriod = 183 };
            public List<string> CalledProcedures { get; } = new();
            public Task<int> GetAutoAccrualValidationCount(string procedure, AutoAccrualRequest request, CancellationToken cancellationToken)
            {
                CalledProcedures.Add(procedure);
                if (procedure == Constants.Application.GetIneligibleClassValidity) return Task.FromResult(IneligibleClassResult);
                if (procedure == Constants.Application.GetNonAccrualClassValidity) return Task.FromResult(NonAccrualClassResult);
                if (procedure == Constants.Application.GetAirlineActivityDateValidity) return Task.FromResult(AirlineActivityDateResult);
                return Task.FromResult(1);
            }
            public Task<AutoAccrualTerms> GetAutoAccrualPartnerTerms(string partnerCode, CancellationToken cancellationToken) => Task.FromResult(PartnerTerms);
            public Task<Result<Response<MemberAccountReferenceData>>> GetMemberAccountReferenceData() => throw new NotSupportedException();
            public Task<Result<Response<PartnersResponse>>> GetPartnersReferenceData() => throw new NotSupportedException();
            public Task<Product> GetProductReferenceDataByName(string productName) => throw new NotSupportedException();
            public BookingDetailsResult ProcessBookingClassData(PreValidationPartnerData data, string bookingClass, string flownClass, DateTime activityDate, string listFareValue, string listTicketValue, string listPaxValue) => throw new NotSupportedException();
            public Task<VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.Distance> GetDistanceOfRouteForInRuleAsync(string origin, string destination, CancellationToken cancellationToken) => throw new NotSupportedException();
        }
    }
}
