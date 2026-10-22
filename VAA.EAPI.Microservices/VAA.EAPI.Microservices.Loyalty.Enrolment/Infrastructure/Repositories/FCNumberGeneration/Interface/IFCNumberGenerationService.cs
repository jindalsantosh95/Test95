using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Interface
{
    public interface IFCNumberGenerationService
    {
        public Task<KeyValuePair<bool, string>> GenerateFCNumber();
    }
}
