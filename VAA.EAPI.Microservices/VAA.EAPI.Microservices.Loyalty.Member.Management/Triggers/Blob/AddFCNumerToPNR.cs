using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.BulkMemberUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Blob
{
    public class AddFCNumerToPNR
    {
        private readonly IAddFCNumberToPNRCommand _addFCNumberToPNRCommandHandler;

        public AddFCNumerToPNR(IAddFCNumberToPNRCommand addFCNumberToPNRCommandHandler)
        {
            _addFCNumberToPNRCommandHandler = addFCNumberToPNRCommandHandler ?? throw new ArgumentNullException(nameof(addFCNumberToPNRCommandHandler));
        }

        [Function(nameof(AddFCNumerToPNR))]
        public async Task Run([BlobTrigger("stampfcnumbertopnr/{name}", Connection = "BlobConnStringForEnrolledMemberToPromotion")] Stream stream, string name, CancellationToken cancellationToken)
        {
            IEnumerable<MemberDetailsToAddSSR> memberDeatilsToAddSSRs = ExtractAndMapFileData(stream);
            await _addFCNumberToPNRCommandHandler.Handle(new AddFCNumberToPNRCommand() { FileName = name, MemberDeatilsToAddSSR = memberDeatilsToAddSSRs }, cancellationToken); ;
        }

        private IEnumerable<MemberDetailsToAddSSR> ExtractAndMapFileData(Stream content)
        {
            using var blobStreamReader = new StreamReader(content);

            return blobStreamReader?.ReadToEnd()?
                .Split(System.Environment.NewLine)
                .Skip(1)
                .Where(record => record.Length > 0)?
                .Select(record =>
                {
                    string[] recordItems = record.Split(",");
                    return new MemberDetailsToAddSSR
                    {
                        AmperityId = recordItems[0].Trim(),
                        MemberNumber = recordItems[1].Trim(),
                        PNR = recordItems[2].Trim(),
                        FirstName = recordItems[3].Trim(),
                        LastName = recordItems[4].Trim(),
                        Pos = recordItems[5].Trim(),
                        OrderId = recordItems[6].Trim()
                    };
                });
        }
    }
}
