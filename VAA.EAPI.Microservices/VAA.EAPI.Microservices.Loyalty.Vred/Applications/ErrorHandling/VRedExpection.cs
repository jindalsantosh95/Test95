using System;
using System.Linq;
using FluentValidation.Results;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.ErrorHandling
{
    public class VRedException : Exception
    {
        public IEnumerable<string> ValidationErrors { get; set; }

        public VRedException() { }
        public VRedException(string code) : base(code) { }
        public VRedException(string code, Exception innerException) : base(code, innerException) { }

        public VRedException(string message, IEnumerable<ValidationFailure> errors) : base(message)
        {
            // Convert list of FluentValidation error messages to list of strings (the error messages are API error codes)
            ValidationErrors = errors.Select(err => err.ErrorMessage);
        }
    }
}
