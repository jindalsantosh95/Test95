using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MemberBenefit
{
    public class MemberCustomAttributesCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<MemberCustomAttributesCreateEvent, Result<Unit>>, IMemberCustomAttributesCreateEventHandler
    {
        public MemberCustomAttributesCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(MemberCustomAttributesCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent memberCustomAttributesEvent, CancellationToken cancellationToken)
        {
            MemberCustomAttributesCommandBase createEvent = Json.DeserializeObject<MemberCustomAttributesCommandBase>((JToken)memberCustomAttributesEvent.Data);
            MemberCustomAttributesDTO dto = new();

            var validationResult = new MemberCustomAttributesCreateValidator().Validate(createEvent.MemberCustomAttributesDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Member Custom attributes create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var existingDoc = (await MemberCustomAttributesCommandBase.GetListOfMemberCustomAttributes(_mediator, createEvent.MemberCustomAttributesDocument.CustomerId, cancellationToken))?.FirstOrDefault();                       

            if (existingDoc is not null)
            {
                existingDoc.LastUpdatedBy = createEvent.MemberCustomAttributesDocument.CreatedBy;                
                existingDoc.ActiveYearCounterForTenureBenefits = createEvent.MemberCustomAttributesDocument.ActiveYearCounterForTenureBenefits.IfIsNullOrWhiteSpace(existingDoc.ActiveYearCounterForTenureBenefits);
                existingDoc.TotalActiveYear = createEvent.MemberCustomAttributesDocument.TotalActiveYear.IfIsNullOrWhiteSpace(existingDoc.TotalActiveYear);
                
                if (createEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear is not null && createEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear.Any())
                {
                    var incomingYear = createEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear.FirstOrDefault();
                    var memberLastFiveVSActivityYears = existingDoc.MemberLastFiveVSActivityYear.ToList();
                    var yearBenefitUpdate = existingDoc.MemberLastFiveVSActivityYear?.FirstOrDefault(m => m.ActivityYear == incomingYear.ActivityYear);

                    if (yearBenefitUpdate != null)
                    {
                        memberLastFiveVSActivityYears.First(m => m.ActivityYear == incomingYear.ActivityYear).BenefitGiven = incomingYear.BenefitGiven;
                    }
                    else if (yearBenefitUpdate is null && existingDoc.MemberLastFiveVSActivityYear.Count() == Constants.Application.MemberActivityYearCount)
                    {
                        var oldestYear = memberLastFiveVSActivityYears.OrderBy(m => m.ActivityYear).First();

                        memberLastFiveVSActivityYears.Remove(oldestYear);
                        memberLastFiveVSActivityYears.Add(incomingYear);
                    }
                    else
                    {
                        memberLastFiveVSActivityYears.Add(createEvent.MemberCustomAttributesDocument.MemberLastFiveVSActivityYear.FirstOrDefault());
                    }

                    existingDoc.MemberLastFiveVSActivityYear = memberLastFiveVSActivityYears;
                }
                dto = (MemberCustomAttributesDTO)existingDoc;
            }
            else
            {
                //data mapping
                dto = (MemberCustomAttributesDTO)createEvent.MemberCustomAttributesDocument;
                dto.Id = memberCustomAttributesEvent.Id;
            }

            return new CosmosDocumentEvent(dto, memberCustomAttributesEvent.EventType);
        }        
    }
}

