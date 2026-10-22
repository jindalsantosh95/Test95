using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FarePointCalc;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class FareCalculationQueryHandler : IRequestHandler<FareCalculationQuery, Result<Response<FareCalculationResponse>>>
    {
        private readonly ILogger<FareCalculationQueryHandler> _logger;
        public FareCalculationQueryHandler(ILogger<FareCalculationQueryHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }

        public async Task<Result<Response<FareCalculationResponse>>> Handle(FareCalculationQuery request, CancellationToken cancellationToken)
        {
            if (request.FareCalculationRequest.Passengers == null && request.FareCalculationRequest.InfantInArms == null) throw new ArgumentNullException(nameof(request.FareCalculationRequest));

            try
            {
                FareCalculationResponse response = new();
                List<PassengerFare> paxFareList = new();
                int paxFare = 0;
                int infantFare = 0;

                if (request.FareCalculationRequest.DtiSegmentInfos != null)
                {
                    var validSegments = request.FareCalculationRequest.DtiSegmentInfos.Where(x => x.SegmentNumber != null && x.DeparturePoint != null && x.FarebasisFareclassCode != null);

                    foreach (var dtiSegmentInfo in validSegments?.Where(x => x.ExpandedPassengerTypeCode != Constants.Application.InfantPTC).DistinctBy(y => y.SegmentNumber))
                    {
                        int endIndex = dtiSegmentInfo.FarebasisFareclassCode.IndexOf("/");
                        string points = (endIndex == -1) ? RegexCompiled.Match(dtiSegmentInfo.FarebasisFareclassCode, @"\d+").Value : Regex.Match(dtiSegmentInfo.FarebasisFareclassCode.Substring(0, endIndex), @"\d+").Value;

                        paxFare += Convert.ToInt32(points) * 100;
                    }

                    foreach (var dtiSegmentInfo in validSegments?.Where(x => x.ExpandedPassengerTypeCode == Constants.Application.InfantPTC).DistinctBy(y => y.SegmentNumber))
                    {
                        int endIndex = dtiSegmentInfo.FarebasisFareclassCode.IndexOf("/");
                        string points = (endIndex == -1) ? RegexCompiled.Match(dtiSegmentInfo.FarebasisFareclassCode, @"\d+").Value : Regex.Match(dtiSegmentInfo.FarebasisFareclassCode.Substring(0, endIndex), @"\d+").Value;

                        infantFare += Convert.ToInt32(points) * 100;
                    }
                }
                if (request.FareCalculationRequest.Passengers != null)
                {
                    paxFareList.AddRange(request.FareCalculationRequest.Passengers.Select(passenger => new PassengerFare
                    {
                        CustomerId = passenger.CustomerId,
                        FirstName = passenger.FirstName,
                        FirstNameNumber = passenger.FirstNameNumber,
                        LastName = passenger.LastName,
                        LastNameNumber = passenger.LastNameNumber,
                        FarePoints = paxFare
                    }));

                    response.TotalFarePoints += paxFare * request.FareCalculationRequest.Passengers.Count();
                }

                if (request.FareCalculationRequest.InfantInArms != null)
                {
                    paxFareList.AddRange(request.FareCalculationRequest.InfantInArms.Select(passenger => new PassengerFare
                    {
                        CustomerId = passenger.CustomerId,
                        FirstName = passenger.FirstName,
                        FirstNameNumber = passenger.FirstNameNumber,
                        LastName = passenger.LastName,
                        LastNameNumber = passenger.LastNameNumber,
                        FarePoints = infantFare
                    }));

                    response.TotalFarePoints += infantFare * request.FareCalculationRequest.InfantInArms.Count();
                }

                response.PaxFares = paxFareList;

                return new Response<FareCalculationResponse>
                {
                    Status = Status.OK,
                    Value = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in Loyalty Member Mgt MS method FarePointCalculation call");
                return Result.Failure<Response<FareCalculationResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}