using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MemberBenefit
{
    public class MemberCustomAttributesPatchEventHandler : TransactionBaseEventHandler, IRequestHandler<MemberCustomAttributesPatchEvent, Result<Unit>>, IMemberCustomAttributesPatchEventHandler
    {
        public MemberCustomAttributesPatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(MemberCustomAttributesPatchEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            MemberCustomAttributesCommandBase patchEvent = Json.DeserializeObject<MemberCustomAttributesCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new MemberCustomAttributesPatchValidator().Validate(patchEvent.MemberCustomAttributesDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Member Custom attributes update request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var existingDoc = (await MemberCustomAttributesCommandBase.GetListOfMemberCustomAttributes(_mediator, patchEvent.MemberCustomAttributesDocument.CustomerId, cancellationToken))
                                                        .FirstOrDefault(n => n.CustomerId == patchEvent.MemberCustomAttributesDocument.CustomerId);
            if (existingDoc is null)
                throw new RequestValidationException(Constants.ErrorCodes.CustomerIdNotFoundMessage);

            existingDoc.LastUpdatedBy = patchEvent.MemberCustomAttributesDocument.LastUpdatedBy;
            existingDoc.ActiveYearCounterForTenureBenefits = !string.IsNullOrEmpty(patchEvent.MemberCustomAttributesDocument.ActiveYearCounterForTenureBenefits) ? patchEvent.MemberCustomAttributesDocument.ActiveYearCounterForTenureBenefits : existingDoc.ActiveYearCounterForTenureBenefits;
            existingDoc.TotalActiveYear = !string.IsNullOrEmpty(patchEvent.MemberCustomAttributesDocument.TotalActiveYear) ? patchEvent.MemberCustomAttributesDocument.TotalActiveYear : existingDoc.TotalActiveYear;
            
            if(patchEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear is not null && patchEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear.Any())
            {
                var incomingYear = patchEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear.FirstOrDefault();
                var memberLastFiveVSActivityYears = existingDoc.MemberLastFiveVSActivityYear.ToList();
                var yearBenefitUpdate = existingDoc.MemberLastFiveVSActivityYear.FirstOrDefault(m => m.ActivityYear == incomingYear.ActivityYear);

                if (yearBenefitUpdate != null)
                {
                    yearBenefitUpdate.BenefitGiven = incomingYear.BenefitGiven;

                    memberLastFiveVSActivityYears.FirstOrDefault(m => m.ActivityYear == yearBenefitUpdate.ActivityYear).BenefitGiven = yearBenefitUpdate.BenefitGiven;
                }
                else if (yearBenefitUpdate is null && existingDoc.MemberLastFiveVSActivityYear.Count() == 5)
                {
                    var oldestYear = memberLastFiveVSActivityYears.OrderBy(m => m.ActivityYear).First();

                    memberLastFiveVSActivityYears.Remove(oldestYear);
                    memberLastFiveVSActivityYears.Add(incomingYear);
                }
                else
                {
                    memberLastFiveVSActivityYears.Add(patchEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear.FirstOrDefault());
                }
            
                existingDoc.MemberLastFiveVSActivityYear = memberLastFiveVSActivityYears;
            }
            
            MemberCustomAttributesDTO dto = (MemberCustomAttributesDTO)existingDoc;
            
            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
