using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public class VredValidations
    {
        public static List<ErrorDetails> Validate(EnrolmentEventRequest enrolmentRequest)
        {
            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.FirstName))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.FirstNameMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.LastName))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.LastNameMissing), Guid.NewGuid().ToString());
            }

            if (enrolmentRequest.Data.CustomerDetails.AddressDetails.All(x => string.IsNullOrEmpty(x.AddressLine1)))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.AddressLine1Missing), Guid.NewGuid().ToString());
            }

            if (enrolmentRequest.Data.CustomerDetails.AddressDetails.All(x => string.IsNullOrEmpty(x.AddressType.ToString())))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.AddressTypeMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.AddressDetails.FirstOrDefault().Country))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.CountryMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.AddressDetails.FirstOrDefault().City))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.CityMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.DateOfBirth))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.DOBMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.CustomerType.ToString()))
            {
                return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.CustomerTypeMissing), Guid.NewGuid().ToString());
            }

            if (enrolmentRequest.Data.CustomerDetails?.ContactDetails?.PhoneDetails != null)
            {
                if (enrolmentRequest.Data.CustomerDetails.ContactDetails.PhoneDetails.All(x => !string.IsNullOrEmpty(x.PhoneNumber)))
                {
                    if (enrolmentRequest.Data.CustomerDetails.ContactDetails.PhoneDetails.All(x => string.IsNullOrEmpty(x.PhoneType.ToString())))
                    {
                        return Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.PhoneTypeMissing), Guid.NewGuid().ToString());
                    }
                }
            }
            return new List<ErrorDetails>();
        }

    }
}
