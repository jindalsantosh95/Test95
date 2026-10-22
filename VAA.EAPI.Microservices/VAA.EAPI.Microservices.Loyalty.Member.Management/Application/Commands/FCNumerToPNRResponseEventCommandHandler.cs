using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.SSRToPNR;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class FCNumerToPNRResponseEventCommandHandler : IRequestHandler<FCNumerToPNRResponseEventCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly TableStorageService<MemberDetailsToAddSSR> _tableStorageService;

        public FCNumerToPNRResponseEventCommandHandler(IMediator mediator, ITableStorageService<MemberDetailsToAddSSR> tableStorageService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _tableStorageService = (TableStorageService<MemberDetailsToAddSSR>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.BlobConnStringForEnrolledMemberToPromotion);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.FCStampingTableName);
        }

        public async Task<Result<Unit>> Handle(FCNumerToPNRResponseEventCommand request, CancellationToken cancellationToken)
        {
            StringBuilder responseFileData = new StringBuilder();
            var responseEventData = Json.DeserializeObject<EventDataForResponse>((JToken)request.Data);
            var fileDescription = TypeDescriptor.GetProperties(typeof(FCStampingResponseFileData)).OfType<PropertyDescriptor>();

            var fileDetails = await _tableStorageService.RetrieveItemsInPartitionAsync(responseEventData.FileName, default); ;

            var responseFileDetails = fileDetails.Select(memberDetail => (FCStampingResponseFileData)memberDetail);

            //Create Header and Content for Response File
            responseFileData = CreateHeaderForResponseFile(responseFileData, fileDescription);
            responseFileData = CreateResponseFile(responseFileData, fileDescription, responseFileDetails);

            //Response file Upload to storage
            await _mediator.Send(new AzureStorageCommand()
            {
                StorageConnection = ConfigReader.Get(Constants.Environment.BlobConnStringForEnrolledMemberToPromotion),
                ContainerName = ConfigReader.Get(Constants.Environment.ContainerNameForAddSSRResponse),
                BlobName = $"PNRListResponse_{DateTime.UtcNow.ToString("ddMMyyyyHHmmss")}.csv",
                FileData = responseFileData.ToString(),
                Action = AzureStorageCommand.Actions.Add
            });

            return Unit.Value;
        }

        private StringBuilder CreateHeaderForResponseFile(StringBuilder responseFileData, IEnumerable<PropertyDescriptor> fileDescription)
        {
            responseFileData.AppendJoin(",", fileDescription.Select(property =>
            {
                var value = property.Name;
                return value != null && value.ToString().Contains(",") ? $"\"{value}\"" : value?.ToString();
            }));

            return responseFileData.Append(Environment.NewLine);
        }

        private StringBuilder CreateResponseFile(StringBuilder responseFileData, IEnumerable<PropertyDescriptor> fileDescription, IEnumerable<FCStampingResponseFileData> memberDeatils)
        {
            foreach (var memberDeatil in memberDeatils)
            {
                responseFileData.AppendJoin(",", fileDescription.Select(property =>
                {
                    var value = property.GetValue(memberDeatil);
                    return value != null && value.ToString().Contains(",") ? $"\"{value}\"" : value?.ToString();
                }));
                responseFileData.Append(Environment.NewLine);
            }

            return responseFileData;
        }
    }
}
