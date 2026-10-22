using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Brand.Triggers;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public class BrandMSCommandHandler : IBrandMSCommand
    {
        private readonly ILogger<BrandController> _logger;
        private readonly IAtpcoCommand _atpcoCommand;
       
        public BrandMSCommandHandler(ILogger<BrandController> logger, IAtpcoCommand atpcoCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _atpcoCommand = atpcoCommand ?? throw new ArgumentNullException(nameof(atpcoCommand));
        }

        public async Task<IEnumerable<Models.Brand>> Handle(BrandPTCQueryRequest brandReqAllPtc, IEnumerable<string> totalPTC, CancellationToken cancellationToken)
        {
            var brandMsResponseList = new ConcurrentBag<Models.Brand>();

            var tasks = brandReqAllPtc.BrandQueryRequest.Select(async brandReq =>
            {
                try
                {
                    var ptc = totalPTC.Where(x => x.Contains(brandReq.Itinerary.First().PassengerTypeCodes.First()));
                    var brandResponse = await _atpcoCommand.Send(brandReq, cancellationToken);

                    if (brandResponse != null)
                    {
                        foreach (var bResp in brandResponse.Itineraries)
                        {
                            bResp.PassengerTypeCodes = ptc;
                        }

                        brandMsResponseList.Add(brandResponse);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing BrandMS request");
                }
            });

            await Task.WhenAll(tasks);

            return brandMsResponseList;
        }
    }
}