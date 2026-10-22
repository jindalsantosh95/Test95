using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class OTPRequestTimerCommandHandler : IRequestHandler<OTPRequestTimerCommand, string>
    {

        private readonly ILogger<OTPRequestTimerCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly TableStorageService<GenerateOtpDbEntities> _tableStorageService;

        public OTPRequestTimerCommandHandler(IMediator mediator, ILogger<OTPRequestTimerCommandHandler> logger, ITableStorageService<GenerateOtpDbEntities> tableStorageService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorageService = (TableStorageService<GenerateOtpDbEntities>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.OTPGenerateTableName);
        }

        public async Task<string> Handle(OTPRequestTimerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                List<GenerateOtpDbEntities> OTPRequestData = await _tableStorageService.RetrieveItemsAsync(default);
                if (OTPRequestData != null && OTPRequestData.Count > 0)
                {
                    OTPRequestData.ForEach(async x =>
                    {
                        if (x.Timestamp.DateTime.CompareTo(DateTime.UtcNow.Subtract(ConfigReader.Get<TimeSpan>(Constants.Environment.OTPCreation))) < 0)
                        {
                            await _tableStorageService.DeleteAsync(x);
                        }
                    });
                }
                return "OTP Request Timer Completed Successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Expired OTP record cleanup failed.");
                return "OTP Request Timer failed";
            }
        }

    }
}
