using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class EnrolCustomerCommandHandler : IRequestHandler<EnrolCustomerCommand, bool>
    {
        private readonly ILogger<EnrolCustomerCommandHandler> _logger;
        private readonly IMediator _mediator;

        public EnrolCustomerCommandHandler(ILogger<EnrolCustomerCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator;
        }

        public async Task<bool> Handle(EnrolCustomerCommand enrolCustomerRequest, CancellationToken cancellationToken)
        {
            if (enrolCustomerRequest.CustomerType == CustomerTypeEnum.Basic)
                return await _mediator.Send(new BasicCustomerCommand() { Id = enrolCustomerRequest.Id, Data = Json.Serialize(enrolCustomerRequest) });

            else
            {
                string countryCode = enrolCustomerRequest?.AddressDetails?.FirstOrDefault(x => x.PreferredFlag.Is("Y"))?.CountryCode;
                if (enrolCustomerRequest.EnrollmentChannel == Constants.Application.PowerAppChannelId || enrolCustomerRequest.EnrollmentChannel == Constants.Application.DeltaChannelID || enrolCustomerRequest.EnrollmentChannel == Constants.Application.WebMyaChannelID || enrolCustomerRequest.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId) || countryCode != Convert.ToString(Country.GB) && countryCode != Convert.ToString(Country.US))
                    return await _mediator.Send(new CustomerCommand() { Id = enrolCustomerRequest.Id, Data = Json.Serialize(enrolCustomerRequest) });
                return await _mediator.Send(new AddressLookupEventCommand() { Id = enrolCustomerRequest.Id, Data = Json.Serialize(enrolCustomerRequest) });

            }
        }
    }
}