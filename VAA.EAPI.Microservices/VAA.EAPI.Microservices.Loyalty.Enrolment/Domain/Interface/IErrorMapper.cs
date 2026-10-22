using CSharpFunctionalExtensions;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface
{
    public interface IErrorMapper
    {
        Result<Response<EnrolmentResponse>> ErrorsMapper(Exception exception, string description, string errorCodeDescription, string errorCode);
    }
}
