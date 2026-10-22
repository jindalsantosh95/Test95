using System.IO;
using System;
using System.Threading.Tasks;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using VAA.Facade.EnrolmentFacade.Domain;
using VAA.Facade.EnrolmentFacade.Helper.Serialisation;
using VAA.Facade.EnrolmentFacade.Infrastructure.Extensions;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;
using VAA.Facade.EnrolmentFacade.Mapper;

namespace VAA.Facade.EnrolmentFacade.Triggers.Http
{
    public class FacadeEnrolmentController
    {
        private readonly IEnrolmentRequestCommandHandler _handler;
        private readonly ILogger<FacadeEnrolmentController> _logger;

        public FacadeEnrolmentController(IEnrolmentRequestCommandHandler handler, ILogger<FacadeEnrolmentController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("enrolment")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "customers/enrolment")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("Facade Enrolment API");
            using var reader = new StreamReader(req.Body);
            string requestBody = await reader.ReadToEndAsync();
            EnrolmentRequest enrolmentRequest = Json.DeserializeObject<EnrolmentRequest>(requestBody);
            var request = EnrolmentRequestCommandMapper.Mapper(enrolmentRequest, req);
            return await ResultExtention<EnrolmentResponse>.ExecuteResultAsync(req, await _handler.Handle(request, executionContext.CancellationToken));

        }
    }
}
