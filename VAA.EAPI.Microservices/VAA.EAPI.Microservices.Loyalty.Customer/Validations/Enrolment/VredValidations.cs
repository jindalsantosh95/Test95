using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public class VredValidations
    {
        public static List<ErrorDetails> Validate(EnrolmentRequestCommand enrolmentRequest)
        {
            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.FirstName))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.FirstNameMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.LastName))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.LastNameMissing), Guid.NewGuid().ToString());
            }

            if (enrolmentRequest.Data.CustomerDetails.AddressDetails.All(x => string.IsNullOrEmpty(x.AddressLine1)))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.AddressLine1Missing), Guid.NewGuid().ToString());
            }

            if (enrolmentRequest.Data.CustomerDetails.AddressDetails.All(x => string.IsNullOrEmpty(x.AddressType.ToString())))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.AddressTypeMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.AddressDetails.FirstOrDefault().Country))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.CountryMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.AddressDetails.FirstOrDefault().City))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.CityMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.DateOfBirth))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.DOBMissing), Guid.NewGuid().ToString());
            }

            if (string.IsNullOrEmpty(enrolmentRequest.Data.CustomerDetails.CustomerType.ToString()))
            {
                return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.CustomerTypeMissing), Guid.NewGuid().ToString());
            }

            if (enrolmentRequest.Data.CustomerDetails?.ContactDetails?.PhoneDetails != null)
            {
                if (enrolmentRequest.Data.CustomerDetails.ContactDetails.PhoneDetails.All(x => !string.IsNullOrEmpty(x.PhoneNumber)))
                {
                    if (enrolmentRequest.Data.CustomerDetails.ContactDetails.PhoneDetails.All(x => string.IsNullOrEmpty(x.PhoneType.ToString())))
                    {
                        return ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.PhoneTypeMissing), Guid.NewGuid().ToString());
                    }
                }
            }
            return new List<ErrorDetails>();
        }

    }
}
