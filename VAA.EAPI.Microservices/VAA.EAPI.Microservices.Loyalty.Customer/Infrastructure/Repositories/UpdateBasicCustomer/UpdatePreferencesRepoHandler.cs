using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;
using static VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities.Preferences;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.UpdateBasicCustomer
{
    public class UpdatePreferencesRepoHandler : IRequestHandler<UpdatePreferencesRepoCommand, bool>
    {
        private readonly ILogger<UpdatePreferencesRepoHandler> _logger;
        private readonly IMediator _mediator;

        public UpdatePreferencesRepoHandler(ILogger<UpdatePreferencesRepoHandler> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UpdatePreferencesRepoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"UpdatePreferencesRepoHandler - Start");

            List<KeyValuePair<string, object>> keyValues = new()
                {
                    new KeyValuePair<string, object>("partitionKey", request.CreateCustomerEntities.CustomerId),
                    new KeyValuePair<string, object>("docType", DocumentType.CustomerProfile.ToString())
                };
            var customerProfileResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustTxnDBCustPrflCntnr), ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustTxnDB), keyValues, Constants.Application.SearchDbQuery));

            var customerProfile = Json.DeserializeObject<CustomerProfile>((JToken)customerProfileResponse.FirstOrDefault());

            List<Preference> preferenceGrp = new();

            foreach (var pref in customerProfile.Preferences)
            {
                Preference preferences = new()
                {
                    CommsType = pref.CommsType,
                    Active = pref.Active,
                    CaptureCannel = pref.CaptureCannel,
                    CreatedBy = pref.CreatedBy,
                    CreatedOn = DateTime.ParseExact(pref.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = pref.PreferenceType == PreferenceTypes.VAAMarketingPermissions.ToString() ? request.CreateCustomerEntities.EnrollmentChannel : pref.LastUpdatedBy,
                    LastUpdatedOn = pref.PreferenceType == PreferenceTypes.VAAMarketingPermissions.ToString() ? DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat) : DateTime.ParseExact(pref.LastUpdatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat),
                    Persona = pref.Persona,
                    PreferenceId = pref.PreferenceId,
                    PreferenceSubType = pref.PreferenceSubType,
                    PreferenceType = pref.PreferenceType,
                    PreferenceValue = pref.PreferenceType == PreferenceTypes.VAAMarketingPermissions.ToString() ? request.CreateCustomerEntities.Preferences.Find(x => x.PreferenceType == PreferenceTypes.VAAMarketingPermissions).PreferenceValue : pref.PreferenceValue,
                    SourceIdentifier = pref.SourceIdentifier,
                    SourceSystem = pref.SourceSystem,
                };
                preferenceGrp.Add(preferences);
            }
            CustomerProfile customer = new()
            {
                Id = customerProfile.Id,
                PartitionKey = customerProfile.PartitionKey,
                Brand = customerProfile.Brand,
                CreatedBy = customerProfile.CreatedBy,
                CreatedOn = DateTime.ParseExact(customerProfile.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat),
                CustomerId = customerProfile.CustomerId,
                DocType = customerProfile.DocType,
                ReasonForTravel = customerProfile.ReasonForTravel,
                SchemaVersion = customerProfile.SchemaVersion,
                SourceIdentifier = customerProfile.SourceIdentifier,
                SourceSystem = customerProfile.SourceSystem,
                LastUpdatedBy = request.CreateCustomerEntities.EnrollmentChannel,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                Preferences = preferenceGrp.ToArray()
            };

            CosmosModifyServiceCommand cosmosModifyService = new()
            {
                CollectionName = ConfigReader.Get(Constants.Environment.SCVCustTxnDBCustPrflCntnr),
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustTxnDB),
                Event = new CosmosDocumentEvent()
                {
                    Document = new CustomerProfileDTO(customer),
                    ModificationType = CosmosModificationType.Replace
                }
            };
            await _mediator.Send(cosmosModifyService, cancellationToken);
            _logger.LogInformation($"UpdatePreferencesRepoHandler - End");
            return true;
        }
    }
}
