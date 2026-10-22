using System;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD
{
    public class SFPDSyncRequest
    {
        [JsonIgnore]
        public HttpMethod RequestType { get; set; }
        public SFPDSyncMutationRequest SfpdData { get; set; }

        public static explicit operator SFPDSyncRequest(SFPDSyncMutationRequest passengerDetailRequest)
        {
            if (passengerDetailRequest is null) throw new ArgumentNullException(nameof(passengerDetailRequest));

            return new SFPDSyncRequest()
            {
                SfpdData = new SFPDSyncMutationRequest
                {
                    CustomerId = passengerDetailRequest.CustomerId,
                    DateOfBirth = Convert.ToDateTime(passengerDetailRequest.DateOfBirth).ToString("yyyy-MM-dd"),
                    EnrollmentChannel = passengerDetailRequest.EnrollmentChannel,
                    FcNumber = passengerDetailRequest.FcNumber,
                    FirstName = passengerDetailRequest.FirstName,
                    LastName = passengerDetailRequest.LastName,
                    Gender = passengerDetailRequest.Gender,
                    CustomerType = Constants.Application.LoyaltySyncCustType
                },
                RequestType = HttpMethod.Post
            };
        }
    }
}

