using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class EntraCustomAuthExtensionsService : IEntraCustomAuthExtensionsService
    {
        private readonly IMediator _mediator;
        private readonly IVerifyMemberStatusRepository _verifyMemberStatusRepository;
        private readonly TableStorageService<GenerateOtpDbEntities> _tableStorageService;


        public EntraCustomAuthExtensionsService(IMediator mediator, IVerifyMemberStatusRepository verifyMemberStatusRepository, ITableStorageService<GenerateOtpDbEntities> tableStorageService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _verifyMemberStatusRepository = verifyMemberStatusRepository ?? throw new ArgumentNullException(nameof(verifyMemberStatusRepository));
            _tableStorageService = (TableStorageService<GenerateOtpDbEntities>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.OTPGenerateTableName);
        }

        public async Task<Response<ResponseObject>> CustomEmailOtp(EntraRequest entraOtpRequest)
        {
            var correlationIdForEvents = Guid.NewGuid().ToString();

            string email = entraOtpRequest.data.OtpContext.Identifier;
            string otp = entraOtpRequest.data.OtpContext.OneTimeCode;

            await _tableStorageService.InsertOrMergeAsync(GenerateOtpDbEntitiesMapper.MapToDBEntityList(email, int.Parse(otp), 1, 0));

            var emailCloudEvent = EmailTriggeringEventMapper.EventDataMapper(correlationIdForEvents, email, otp, ConfigReader.Get(Constants.Environment.OTPSignTemplate));

            await PublishToEmailEvent(emailCloudEvent, correlationIdForEvents, new CancellationToken());

            ResponseObject responseData = new ResponseObject("microsoft.graph.OnOtpSendResponseData");
            responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                "microsoft.graph.OtpSend.continueWithDefaultBehavior",null) };

            return new Response<ResponseObject>()
            {
                Status = Status.OK,
                Value = responseData
            };
        }

        public async Task<Response<ResponseObject>> CustomToken(EntraRequest entraCode)
        {
            var customerResponse = await _verifyMemberStatusRepository.VerifyMemberStatus(string.Empty, entraCode.data.AuthenticationContext.User.Id, new CancellationToken());

            ClaimsResponse claimsResponse = new()
            {
                CorrelationId = entraCode.data.AuthenticationContext.CorrelationId,
                MemberNumber = customerResponse.MemberNumber,
                MemberStatus = customerResponse.MemberStatus,
                MemberEmail = customerResponse.MemberEmail

            };

            ResponseObject responseData = new ResponseObject("microsoft.graph.onTokenIssuanceStartResponseData");
            responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction("microsoft.graph.tokenIssuanceStart.provideClaimsForToken", claimsResponse) };

            return new Response<ResponseObject>()
            {
                Status = Status.OK,
                Value = responseData
            };
        }

        private async Task PublishToEmailEvent(CloudEvent emailEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {

            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.OTPEmailEventSubject, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(emailEvent, cancellationToken);
            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.OTPEmailEventSubject, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);
                throw;
            }

        }
    }
}
