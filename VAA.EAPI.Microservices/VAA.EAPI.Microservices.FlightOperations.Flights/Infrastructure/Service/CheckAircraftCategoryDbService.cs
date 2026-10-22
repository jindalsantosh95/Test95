using System.Data;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Service
{
    public sealed class CheckAircraftCategoryDbService(ISqlQueryService sqlQueryService) : ICheckAircraftCategoryDbService
    {
        private const int MaxSqlParametersPerBatch = 2000;
        private readonly ISqlQueryService _sqlQueryService = sqlQueryService ?? throw new ArgumentNullException(nameof(sqlQueryService));

        public async Task<IReadOnlyCollection<string>> GetTailNumbersWithCategoryAsync(IEnumerable<string> tailNumbers, int categoryIndex, CancellationToken cancellationToken)
        {
            var tailsWithCategory = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var normalizedTailNumbers = tailNumbers
                .Where(tailNumber => !string.IsNullOrWhiteSpace(tailNumber))
                .Select(tailNumber => tailNumber.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (normalizedTailNumbers.Length == 0)
                return tailsWithCategory;

            var connectionString = ConfigReader.Get(Constants.Environment.AircraftCategorySqlDbConnectionString);
            foreach (var tailNumberBatch in normalizedTailNumbers.Chunk(MaxSqlParametersPerBatch))
            {
                var records = await _sqlQueryService.QueryAsync<AircraftCategoryTailRecord>(
                    connectionString,
                    Constants.Application.CheckAircraftCategoryQuery,
                    new
                    {
                        categoryIndex,
                        tailNumbers = tailNumberBatch
                    },
                    CommandType.Text,
                    cancellationToken);

                foreach (var record in records.Where(record => !string.IsNullOrWhiteSpace(record.TailNumber)))
                {
                    tailsWithCategory.Add(record.TailNumber);
                }
            }

            return tailsWithCategory;
        }
    }
}
