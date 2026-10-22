using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IOperationalScheduleProvider
    {
        Task<OperationalScheduleResponse> GetOperationalScheduleAsync(OperationScheduleRequest request);
    }
}
