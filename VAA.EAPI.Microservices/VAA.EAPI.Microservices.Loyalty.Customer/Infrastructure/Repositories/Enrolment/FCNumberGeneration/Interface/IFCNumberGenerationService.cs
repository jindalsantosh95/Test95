using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.FCNumberGeneration.Interface
{
    public interface IFCNumberGenerationService
    {
        public Task<KeyValuePair<bool, string>> GenerateFCNumber();
    }
}
