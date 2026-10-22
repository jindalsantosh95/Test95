using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class VaAccountEnrolmentCommandHandler : IRequestHandler<VaAccountEnrolmentCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public VaAccountEnrolmentCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Unit>> Handle(VaAccountEnrolmentCommand request, CancellationToken cancellationToken)
        {
            var req = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));

            string validateResponse = ValidateRequest(req);

            if (!string.IsNullOrEmpty(validateResponse))
            {
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(GetErrorResponse(validateResponse)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.DeDupValidateEventType }, cancellationToken);
                return Unit.Value;
            }

            var checkEmailResponse = await _mediator.Send(new CheckEmailQuery() { Email = req.ContactDetails.EmailDetails.First().EmailId });

            if (!string.IsNullOrEmpty(checkEmailResponse.FcNumber) && !string.IsNullOrEmpty(checkEmailResponse.CustomerId))
            {
                if (checkEmailResponse.isBasic)
                {
                    req.CustomerId = checkEmailResponse.CustomerId;
                    req.FcNumber = checkEmailResponse.FcNumber;
                }
                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(GetErrorResponse(Constants.ErrorCodes.DeDupValidationError)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.DeDupValidateEventType }, cancellationToken);
                    return Unit.Value;
                }
            }
            else
            {
                req.CustomerId = Convert.ToString(Guid.NewGuid());
            }

            req.ContactDetails.PhoneDetails.First().PhoneType = req.ContactDetails.PhoneDetails.First().PhoneType ?? PhoneDetails.PhoneTypes.Mobile;
            req.ContactDetails.PhoneDetails.First().PreferredFlag = req.ContactDetails.PhoneDetails.First().PreferredFlag.IsNullOrEmpty() ? "Y" : req.ContactDetails.PhoneDetails.First().PreferredFlag;

            req.ContactDetails.EmailDetails.First().EmailId = req.ContactDetails.EmailDetails.First().EmailId.ToLower();
            req.ContactDetails.EmailDetails.First().Usage = req.ContactDetails.EmailDetails.First().Usage ?? EmailDetails.Usages.Personal;
            req.ContactDetails.EmailDetails.First().PreferredFlag = req.ContactDetails.EmailDetails.First().PreferredFlag.IsNullOrEmpty() ? "Y" : req.ContactDetails.EmailDetails.First().PreferredFlag;

            req.CustomerType = Domain.Enums.CustomerTypeEnum.VirginAccount;
            req.MediaCode = Constants.Application.WebChannelID;
            req.EnrollmentChannel = Constants.Application.WebChannelID;
            req.MemberType = Constants.Application.MemberType;

            if (req.ContactDetails.PhoneDetails.First().CountryCode.StartsWith("+"))
                req.ContactDetails.PhoneDetails.First().CountryCode.TrimStart('+');


            await PublishEvent(EventGridEventMapper.VacEnrolmentDataMapper(req), Guid.NewGuid().ToString(), cancellationToken);

            var result = new Response<EnrolmentResponse>
            {
                Status = Status.OK,
                Value = new()
                {
                    CustomerId = req.CustomerId,
                    Memberdetails = new MemberStatus()
                    {
                        Status = MemberStatus.StatusEnum.Active
                    }
                }
            };
            await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.DeDupValidateEventType }, cancellationToken);
            return Unit.Value;
        }

        private static string ValidateRequest(CreateCustomerEntities request)
        {
            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName) || string.IsNullOrEmpty(request.DateOfBirth) || string.IsNullOrEmpty(request.ContactDetails.EmailDetails.First().EmailId))
                return Constants.ErrorCodes.MandatoryInputFieldsMissing;

            DateTime birthDate = Convert.ToDateTime(request.DateOfBirth);
            int age = DateTime.Now.Year - birthDate.Year;

            if (DateTime.Now < birthDate.AddYears(age))
                age--;

            if (age < Constants.Application.VacAgeLowerLimit || age > Constants.Application.VacAgeUpperLimit)
                return Constants.ErrorCodes.CustomerAgeLimitError;

            return string.Empty;

        }
        private static Response<string> GetErrorResponse(string errorCode)
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }

        private async Task PublishEvent(CloudEvent vacEnrolmentEvent, string id, CancellationToken cancellationToken)
        {

            await _mediator.Send(new EventRequestInsertCommand { Id = id, Type = Constants.Application.VacEnrolmentEventType, Data = vacEnrolmentEvent.Data, Subject = vacEnrolmentEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(vacEnrolmentEvent, cancellationToken);
            }

            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = Constants.Application.VacEnrolmentEventType, Data = vacEnrolmentEvent.Data, Subject = vacEnrolmentEvent.Subject }, cancellationToken);
                throw;
            }
        }

    }
}
