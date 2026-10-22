using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MillionMiler;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.MillionMiler;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class MillionMilerEmailCommandHandler : IRequestHandler<MillionMilerEmailCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MillionMilerEmailCommandHandler> _logger;
        private readonly ICustomerProvider _customerProvider;

        public MillionMilerEmailCommandHandler(IMediator mediator, ILogger<MillionMilerEmailCommandHandler> logger, ICustomerProvider customerProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));

        }

        public async Task<Result<Response<string>>> Handle(MillionMilerEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                MMCustomer mmCustomer = Json.Deserialize<MMCustomer>(Convert.ToString(request.Data));
                CustomerDTO customer = await _customerProvider.GetCustomer(mmCustomer.CustomerId, null);

                if (customer == null || customer.Customer == null || string.IsNullOrWhiteSpace(customer.Customer.CustomerId))
                {
                    return Result.Failure<Response<string>>("Invalid Request - CustomerId / MemberId");
                }

                MillonMilerCustomer millonMilerCustomer = new MillonMilerCustomer()
                {
                    CustomerId = customer.Customer.CustomerId,
                    MemberId = customer.Customer.LoyaltyNumber,
                    EmailId = customer.Customer.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId?.Trim(),
                    FirstName = customer.Customer.FirstName,
                    LastName = customer.Customer.LastName
                };

                string emailTemplate = Constants.Environment.MMEmailTemplate;

                if (customer.Customer.AwardDetails != null)
                {
                    emailTemplate = Constants.Environment.MMEmailTemplate;
                    if (customer.Customer.AwardDetails.Where(f => f.ProductName == "Lifetime Gold").Count() > 0)
                    {
                        emailTemplate = Constants.Environment.LTGMMEmailTemplate;
                    }

                }

                await _mediator.Send(new MillionMilerEmailTriggerCommand()
                {
                    Id = Guid.NewGuid(),
                    FirstName = millonMilerCustomer.FirstName,
                    LastName = millonMilerCustomer.LastName,
                    CustomerId = millonMilerCustomer.CustomerId,
                    MemberNumber = millonMilerCustomer.MemberId,
                    Email = millonMilerCustomer.EmailId,
                    EmailTemplate = emailTemplate
                });

                #region Create Customer Transaction

                Result<Response<PartnersResponse>> response = await _mediator.Send(new PartnersRetrievalQuery { });
                PartnersResponse partners = response.Value.Value;
                string partnerId = partners.Partners.FirstOrDefault(x => x.PartnerCode == Constants.Application.PartnerCode).PartnerId;

                Domain.Entities.BirthdayBonus.Product productDetail = await _mediator.Send(new GetProductRefrenceDetailsQuery { ProductName = Constants.Application.MillionMilerProductName });

                TransactionCreateEventData transactionCreateEventData = MillionMilerTransactionEventMapper.MapMillionMilerTransactionCreateEventData(customer, partnerId, productDetail);
                var txnCreateCloudEvent = MillionMilerTransactionEventMapper.MapTxnDocCreateEvent(transactionCreateEventData);

                await _mediator.Send(new EventRequestInsertCommand { Id = txnCreateCloudEvent.Id, Type = txnCreateCloudEvent.Type, Data = txnCreateCloudEvent.Data, Subject = txnCreateCloudEvent.Subject });

                try
                {
                    await _mediator.Publish(txnCreateCloudEvent);
                }
                catch
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = txnCreateCloudEvent.Id, Type = txnCreateCloudEvent.Type, Data = txnCreateCloudEvent.Data, Subject = txnCreateCloudEvent.Subject }, cancellationToken);
                    throw;
                }
                #endregion

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.SuccessMessage
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MillionMilerEmailCommandHandler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
