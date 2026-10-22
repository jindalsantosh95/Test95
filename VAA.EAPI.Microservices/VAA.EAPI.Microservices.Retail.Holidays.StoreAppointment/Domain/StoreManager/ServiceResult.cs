using System.Collections.Generic;
using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public bool IsNotFound { get; init; }
    public bool IsConflict { get; init; }
    public bool IsValidationError { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public IReadOnlyDictionary<string, object?>? ErrorDetails { get; init; }
    public T? Value { get; init; }

    public static ServiceResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static ServiceResult<T> NotFound(string message) => new() { IsNotFound = true, ErrorMessage = message };
    public static ServiceResult<T> NotFound(string code, string message) => new() { IsNotFound = true, ErrorCode = code, ErrorMessage = message };
    public static ServiceResult<T> Conflict(string message) => new() { IsConflict = true, ErrorMessage = message };
    public static ServiceResult<T> Conflict(string code, string message) => new() { IsConflict = true, ErrorCode = code, ErrorMessage = message };
    public static ServiceResult<T> Conflict(string code, string message, IReadOnlyDictionary<string, object?> details) =>
        new() { IsConflict = true, ErrorCode = code, ErrorMessage = message, ErrorDetails = details };
    public static ServiceResult<T> ValidationError(string message) => new() { IsValidationError = true, ErrorMessage = message };
    public static ServiceResult<T> ValidationError(string code, string message) => new() { IsValidationError = true, ErrorCode = code, ErrorMessage = message };

    public Result<Response<T>> ToResponse()
    {
        if (IsSuccess)
        {
            return Result.Success(new Response<T> { Status = Status.OK, Value = Value! });
        }

        return Result.Success(new Response<T>
        {
            Status = GetFailureStatus(),
            Errors = new List<ErrorDetails>
            {
                new StoreManagerErrorDetails
                {
                    Code = ErrorCode,
                    Message = ErrorMessage ?? "Unexpected service result.",
                    Details = ErrorDetails
                }
            }
        });
    }

    private Status GetFailureStatus()
    {
        if (IsNotFound)
        {
            return Status.NotFound;
        }

        if (IsConflict)
        {
            return Status.BusinessException;
        }

        return Status.BadRequest;
    }
}
