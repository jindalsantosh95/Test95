using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;
using Preferences = VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.Preferences;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class DeDupCheckCommandHandler : IRequestHandler<DeDupCheckCommand, Result<Response<EnrolmentResponse>>>
    {
        private readonly IDeDupValidationEnrolment _deDupValidation;
        private readonly IFCNumberGenerationRepository _fcNumberGenerationRepository;
        private readonly ILogger<DeDupCheckCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IValidateMediaCodeReferenceDB _validateMediaCodeReferenceDB;

        public DeDupCheckCommandHandler(ILogger<DeDupCheckCommandHandler> logger, IFCNumberGenerationRepository fcNumberGenerationRepository, IDeDupValidationEnrolment dedupValidationEnrolment, IMediator mediator, IValidateMediaCodeReferenceDB validateMediaCodeReferenceDB)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _fcNumberGenerationRepository = fcNumberGenerationRepository ?? throw new ArgumentNullException(nameof(fcNumberGenerationRepository));
            _deDupValidation = dedupValidationEnrolment ?? throw new ArgumentNullException(nameof(dedupValidationEnrolment));
            _validateMediaCodeReferenceDB = validateMediaCodeReferenceDB ?? throw new ArgumentNullException(nameof(validateMediaCodeReferenceDB));
        }

        public async Task<Result<Response<EnrolmentResponse>>> Handle(DeDupCheckCommand enrolmentRequest, CancellationToken cancellationToken)
        {
            try
            {
                var preferredPartnerValidationResult = ValidatePreferredPartner(enrolmentRequest.DeDupRequest.Data);
                if (preferredPartnerValidationResult.Value.Errors != null)
                    return preferredPartnerValidationResult;

                var mediaCodeValidationResult = await ValidateMediaCode(enrolmentRequest.DeDupRequest.Data.CustomerDetails.LoyaltyMember.MediaCode);
                if (mediaCodeValidationResult.Value.Errors != null)
                    return mediaCodeValidationResult;

                CreateCustomerEntities requestEvent = new();

                if (await _mediator.Send(new CheckEmailAndTierQuery(enrolmentRequest.DeDupRequest, requestEvent)))

                    return Result.Success(new Response<EnrolmentResponse>()
                    {
                        Value = new EnrolmentResponse()
                        {
                            CustomerId = requestEvent.CustomerId,
                            Memberdetails = new MemberStatus()
                            {
                                MemberNumber = requestEvent.FcNumber,
                                Status = (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), GetStatus.Status(enrolmentRequest.DeDupRequest).Replace(" ", ""))
                            }
                        },
                        Status = Status.OK
                    });

                var deDupValidationResult = await ValidateDeDupCheck(enrolmentRequest.DeDupRequest);

                return deDupValidationResult.Value.Status == Status.OK ? await CreateFcAndCustomerId(enrolmentRequest.DeDupRequest) : deDupValidationResult;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"DedupCheck Command failed: {ex.Message}");
                return Result.Failure<Response<EnrolmentResponse>>(Json.Serialize(ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString())));
            }
        }

        private async Task<Result<Response<EnrolmentResponse>>> ValidateMediaCode(string mediaCode)
        {
            List<ErrorDetails> mediaCodeValidationError = await _validateMediaCodeReferenceDB.ValidateMediaCodeFromReferenceDB(mediaCode);
            Result.Success(new Response<EnrolmentResponse>() { Errors = mediaCodeValidationError, Value = null, Status = Status.BusinessException });

            return mediaCodeValidationError.Any()
                ? Result.Success(new Response<EnrolmentResponse>() { Errors = mediaCodeValidationError, Value = null, Status = Status.BusinessException })
                : Result.Success(new Response<EnrolmentResponse>() { Status = Status.OK });
        }

        private Result<Response<EnrolmentResponse>> ValidatePreferredPartner(EnrolmentRequest enrolmentRequest)
        {
            var preferences = enrolmentRequest?.CustomerDetails?.Preferences ?? Enumerable.Empty<Preferences>();
            var preferredPartner = preferences.FirstOrDefault(x => x.PreferenceType == Preferences.PreferenceTypes.PreferredPartnerFFP);
            return (preferredPartner != null && !string.IsNullOrEmpty(preferredPartner.PreferenceValue) && preferredPartner.PreferenceValue.Length > 3)
             ? Result.Success(new Response<EnrolmentResponse>()
             {
                 Errors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidPreferredPartnerFFP), Guid.NewGuid().ToString()),
                 Value = null,
                 Status = Status.BusinessException
             }) : Result.Success(new Response<EnrolmentResponse>() { Status = Status.OK });
        }

        private async Task<Result<Response<EnrolmentResponse>>> ValidateDeDupCheck(EnrolmentRequestCommand enrolmentRequest)
        {
            List<ErrorDetails> deDupValidationError = await _deDupValidation.DeDupValidate(enrolmentRequest);
            return deDupValidationError.Any()
                ? Result.Success(new Response<EnrolmentResponse>() { Errors = deDupValidationError, Value = null, Status = Status.BusinessException })
                : Result.Success(new Response<EnrolmentResponse>() { Status = Status.OK });
        }

        private async Task<Result<Response<EnrolmentResponse>>> CreateFcAndCustomerId(EnrolmentRequestCommand enrolmentRequest)
        {
            string fcNumber = (await _fcNumberGenerationRepository?.GenerateFlyingClubNumber()).Value?.Value?.Memberdetails?.FlyingClubCardNumber;
            string customerId = Guid.NewGuid().ToString();

            _logger.LogInformation($"FcNumber {fcNumber} generated successfully for CustomerId: {customerId}");

            return Result.Success(new Response<EnrolmentResponse>()
            {
                Value = new EnrolmentResponse()
                {
                    CustomerId = customerId,
                    Memberdetails = new MemberStatus()
                    {
                        MemberNumber = fcNumber,
                        Status = (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), GetStatus.Status(enrolmentRequest).Replace(" ", ""))
                    }
                },
                Status = Status.OK
            });
        }
    }
}