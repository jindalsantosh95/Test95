using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.FCNumberGeneration
{
    public interface IFCNumberGenerationRepository
    {
        public Task<Result<Response<EnrolmentResponse>>> GenerateFlyingClubNumber();
    }
}
