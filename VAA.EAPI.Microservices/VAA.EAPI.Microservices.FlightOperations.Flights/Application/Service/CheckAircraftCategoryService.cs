using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Service
{
    public sealed class CheckAircraftCategoryService(ICheckAircraftCategoryDbService checkAircraftCategoryDbService, ILogger<CheckAircraftCategoryService> logger) : ICheckAircraftCategoryService
    {
        private readonly ICheckAircraftCategoryDbService _checkAircraftCategoryDbService = checkAircraftCategoryDbService ?? throw new ArgumentNullException(nameof(checkAircraftCategoryDbService));
        private readonly ILogger<CheckAircraftCategoryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Result<Response<CheckAircraftCategoryResponse>>> CheckAircraftCategoryAsync(CheckAircraftCategoryRequest request, CancellationToken cancellationToken)
        {
            var response = new Response<CheckAircraftCategoryResponse>() { Status = Status.OK };

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var tailNumbers = NormalizeTailNumbers(request.TailNumbers).ToArray();
                if (tailNumbers.Length == 0)
                    throw new ArgumentException("At least one tail number is required.", nameof(request.TailNumbers));

                if (request.CategoryIndex <= 0)
                    throw new ArgumentException("Category index must be greater than zero.", nameof(request.CategoryIndex));

                var tailsWithCategory = await _checkAircraftCategoryDbService.GetTailNumbersWithCategoryAsync(tailNumbers, request.CategoryIndex, cancellationToken);
                var tailsWithCategorySet = tailsWithCategory.ToHashSet(StringComparer.OrdinalIgnoreCase);

                response.Value = new CheckAircraftCategoryResponse
                {
                    AircraftCategoryChecks =
                    [
                        .. tailNumbers.Select(tailNumber => new CheckAircraftCategoryResult
                        {
                            TailNumber = tailNumber,
                            DoesCategoryExist = tailsWithCategorySet.Contains(tailNumber)
                        })
                    ]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aircraft category check failed for {CategoryIndex} with {TailNumberCount} tail numbers.", request?.CategoryIndex, request?.TailNumbers?.Count() ?? 0);

                response.Status = Status.Error;
                response.ErrorMessage = ex.Message;
                response.Errors =
                [
                    new ErrorDetails()
                    {
                        ExceptionType = ex.GetType().Name,
                        Message = ex.Message
                    }
                ];
            }

            return Result.Success(response);
        }

        private static IEnumerable<string> NormalizeTailNumbers(IEnumerable<string>? tailNumbers)
        {
            return tailNumbers?
                .Where(tailNumber => !string.IsNullOrWhiteSpace(tailNumber))
                .Select(tailNumber => tailNumber.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase) ?? [];
        }
    }
}
