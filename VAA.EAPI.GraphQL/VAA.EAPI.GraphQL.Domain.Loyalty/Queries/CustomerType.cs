using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    public class CustomerType : ObjectType<CustomerResponse>
    {
        private readonly ILogger<CustomerType> _logger;
        private readonly IContactNoteQueryProvider _notesProvider;

        public CustomerType(IContactNoteQueryProvider notesProvider, ILogger<CustomerType> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notesProvider = notesProvider ?? throw new ArgumentNullException(nameof(notesProvider));
        }
        protected override void Configure(IObjectTypeDescriptor<CustomerResponse> descriptor)
        {
            descriptor.Field(fld => fld.ContactNotes)
                      .Resolve((cntx, ct) =>
                      {
                          try
                          {
                              return _notesProvider.Query(cntx.Parent<CustomerResponse>().CustomerId.ToString()).GetAwaiter().GetResult();
                          }
                          catch (HttpRequestException ex)
                          {
                              var content = ex.ReadResponseContentAsStringAsync().GetAwaiter().GetResult();

                              cntx.ReportError(ErrorBuilder.New()
                                           .SetMessage($"Unable to retrieve customer's contact notes [{ex.Message}]")
                                           .SetExtension("APIResponse", content)
                                           .SetCode(((int)ex.StatusCode).ToString())
                                           .Build());

                              return Enumerable.Empty<ContactNote>();
                          }
                      });
        }
    }
}
