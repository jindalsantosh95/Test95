using CSharpFunctionalExtensions;
using System;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface
{
    public interface IErrorMapper
    {
        Result<Response<EnrolmentResponse>> ErrorsMapper(Exception exception, string description, string errorCodeDescription, string errorCode);
    }
}
