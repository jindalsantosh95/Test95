using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class DesignatePrinter : IDesignatePrinter
    {
        private readonly DesignatePrinterPortTypeClient _designatePrinterPortTypeClient;
        private readonly ILogger<DesignatePrinter> _logger;

        public DesignatePrinter(DesignatePrinterPortTypeClient designatePrinterPortTypeClient, ILogger<DesignatePrinter> logger)
        {
            _designatePrinterPortTypeClient = designatePrinterPortTypeClient ?? throw new ArgumentNullException(nameof(designatePrinterPortTypeClient)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DesignatePrinterRQResponse> DesignatePrinterAsync(DesignatePrinterRQRequest designatePrinterRQ)
        {
            try
            {
                return await _designatePrinterPortTypeClient.DesignatePrinterRQAsync(designatePrinterRQ.MessageHeader, designatePrinterRQ.Security, designatePrinterRQ.DesignatePrinterRQ);
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre designate printer Request: {Request}, Error: {ErrorMessage}", designatePrinterRQ != null ? Xml.Serialize(designatePrinterRQ) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
