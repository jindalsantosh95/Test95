using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes
{
    public class ContactNoteCommandStatusQuery : CommandStatusQueryBase, IRequest<Result<Response<ContactNoteCommandQueryResponse>>>
    {
    }

    public class ContactNoteCommandStatusQueryHandler : CommandStatusBaseQueryHandler<ContactNoteCommandQueryResponse>, IRequestHandler<ContactNoteCommandStatusQuery, Result<Response<ContactNoteCommandQueryResponse>>>
    {
        public ContactNoteCommandStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<ContactNoteCommandQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<ContactNoteCommandQueryResponse>>> Handle(ContactNoteCommandStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<ContactNoteCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<ContactNoteCosmosDocumentEvent>>>(commandEventOutcome);


            // *** CURRENTLY NON FUNCTIONAL IF THE EVENT HANDLER HAD AN ERROR *** //

            //if (documentEvent.Value.Value.Document.DocType != ConfigReader.Get(Constants.EnvironmentSettings.ContactNoteDocType))
            //    return NotFoundResponse(requestId);

            if (documentEvent.Value.Status == Status.OK)
                return new Response<ContactNoteCommandQueryResponse>()
                {
                    Status = Status.OK,
                    Value = new ContactNoteCommandQueryResponse()
                    {
                        CommandStatus = Status.OK,
                        RequestId = requestId,
                        ContactNote = documentEvent.Value.Value is null ? null     // Document could be null if the request was a delete
                                                                        : new ContactNote(documentEvent.Value.Value.Document)
                    }
                };
            else
                return new Response<ContactNoteCommandQueryResponse>()
                {
                    Status = Status.OK,
                    Value = new ContactNoteCommandQueryResponse()
                    {
                        CommandStatus = Status.BusinessException,
                        RequestId = requestId,
                        Errors = documentEvent.Value.Errors
                    }
                };
        }
    }
}
