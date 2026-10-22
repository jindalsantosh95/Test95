using System;
using System.Collections.Generic;
using System.Text;

namespace VAA.EAPI.Facade.B2CAPI.Domain.Model
{
    class EnrolmentServiceResponse
    {
        public bool Success { get; set; }
        public EnrolmentResponse EnrolmentData { get; set; }

        public List<FailureResponse> EnrolmentFailedData { get; set; }
    }
}
