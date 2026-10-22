using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.FCNumberGeneration
{
    public interface IFCNumberGenerationRepository
    {
        public Task<Result<Response<EnrolmentResponse>>> GenerateFlyingClubNumber();
    }
}
