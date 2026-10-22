using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class ValidateBookingClassQuery : IRequest<Result<Response<BookingDetailsResult>>>
    {
        public BookingClassValidationRequest ValidationRequest { get; set; }

    }
}
