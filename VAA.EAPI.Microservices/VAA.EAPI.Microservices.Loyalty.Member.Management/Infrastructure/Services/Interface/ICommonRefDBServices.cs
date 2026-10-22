using CSharpFunctionalExtensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface
{
    public interface ICommonRefDBServices
    {
        Task<Result<Response<MemberAccountReferenceData>>> GetMemberAccountReferenceData();
        Task<Result<Response<PartnersResponse>>> GetPartnersReferenceData();
        Task<Product> GetProductReferenceDataByName(string ProductName);
        BookingDetailsResult ProcessBookingClassData(PreValidationPartnerData partnerBookingData, string bookingClass, string flownClass, DateTime activityDate, string listFareValue, string listTicketValue, string listPaxValue);
        Task<int> GetAutoAccrualValidationCount(string procedure, AutoAccrualRequest request, System.Threading.CancellationToken cancellationToken);
        Task<VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.AutoAccrualPartnerTerms> GetAutoAccrualPartnerTerms(string partnerCode, CancellationToken cancellationToken);
        Task<Models.Distance> GetDistanceOfRouteForInRuleAsync(string origin, string destination, CancellationToken cancellationToken);
    }
}
