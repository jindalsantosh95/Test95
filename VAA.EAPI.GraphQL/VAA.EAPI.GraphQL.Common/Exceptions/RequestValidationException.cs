using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Common.Exceptions
{
    public sealed class RequestValidationException : Exception
    {
        public IEnumerable<string> ValidationErrors { get; }

        public RequestValidationException() { }

        public RequestValidationException(string message) : base(message)
        {
            ValidationErrors = new List<string> { message };
        }

        public RequestValidationException(string message, List<ValidationFailure> errors) : base(message)
        {
            // Convert list of FluentValidation error messages to list of strings (the error messages are API error codes)
            ValidationErrors = errors.Select(err => err.ErrorMessage);
        }

        public RequestValidationException(string message, Exception innerException) : base(message, innerException) { }

        public RequestValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
