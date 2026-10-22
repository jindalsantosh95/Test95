using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
{
    public class GroupDetailsServiceHandler(
        GroupDocumentServiceQueryHandler groupDocumentServiceQueryHandler,
        GroupHistoryDocumentServiceQueryHandler groupHistoryDocumentServiceQueryHandler,
        MemberAccountDetailsQueryHandler memberAccountDetailsQueryHandler)
        : IGroupDetailsService
    {
        private readonly GroupDocumentServiceQueryHandler _groupDocumentServiceQueryHandler =
            groupDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(groupDocumentServiceQueryHandler));

        private readonly GroupHistoryDocumentServiceQueryHandler _groupHistoryDocumentServiceQueryHandler =
            groupHistoryDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(groupHistoryDocumentServiceQueryHandler));

        private readonly MemberAccountDetailsQueryHandler _memberAccountDetailsQueryHandler =
            memberAccountDetailsQueryHandler ?? throw new ArgumentNullException(nameof(memberAccountDetailsQueryHandler));
        public async Task<Result<Response<HouseholdDetailsResponse>>> GetHouseholdAccountDetails(string customerId, CancellationToken cancellationToken)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(customerId))
                {
                    throw new ArgumentException("CustomerId cannot be null or empty.", nameof(customerId));
                }

                HouseholdDetailsResponse householdDetailsResponse = new HouseholdDetailsResponse() { HouseholdHistory = new List<HouseholdHistory>(), HouseholdMembers = new List<HouseholdMembers>() };
                HouseHoldGroup groupDocByHoHCustId = (await GetGroupDocumentByHoHCustomerId(customerId, cancellationToken)).Value?.Value;
                GroupResponse groupDocByGHCustId = (await GetGroupDocumentByGHCustomerId(customerId, cancellationToken)).Value?.Value;

                if (groupDocByHoHCustId != null && groupDocByHoHCustId.HeadOfGroupCustomerId == customerId)
                {
                    foreach (var groupHist in groupDocByHoHCustId.GroupHistory)
                    {
                        if (groupHist.ExitDate != null && groupHist.ExitDate <= DateTime.UtcNow)
                        {
                            householdDetailsResponse.HouseholdHistory.Add(new HouseholdHistory()
                            {
                                CustomerId = groupHist.CustomerId,
                                StartDate = groupHist.StartDate,
                                EndDate = groupHist.ExitDate,
                                RelationshipType = Constants.Application.ChildRelationshipType,
                                GroupDocId = groupDocByHoHCustId.Id,
                                LastUpdatedBy = groupHist.LastUpdatedBy,
                                LastUpdatedOn = groupHist.LastUpdatedOn,
                                CreatedBy = groupHist.CreatedBy
                            });
                        }
                        else
                        {
                            MemberAccountResponse memberAccountResponse = null;
                            if (!string.IsNullOrWhiteSpace(groupHist.CustomerId))
                            {
                                var memberAccountResult = await _memberAccountDetailsQueryHandler.Handle(
                                    new MemberAccountDetailsQuery { CustomerId = groupHist.CustomerId },
                                    cancellationToken);

                                memberAccountResponse = memberAccountResult.IsSuccess ? memberAccountResult.Value?.Value : null;
                            }
                            householdDetailsResponse.HouseholdMembers.Add(new HouseholdMembers()
                            {
                                CustomerId = groupHist.CustomerId,
                                AssociationDate = groupHist.StartDate,
                                GroupDocId = groupDocByHoHCustId.Id,
                                RetainPointTotal = memberAccountResponse?.loyaltyMemberAccount?.RetainPointTotal ?? "0",
                                RollingPointTotal = memberAccountResponse?.loyaltyMemberAccount?.RollingPointTotal ?? "0",
                                RelationshipType = groupHist.HoHManaged ? Constants.Application.HoHManagedChildRelationshipType : Constants.Application.ChildRelationshipType,
                                LastUpdatedBy = groupHist.LastUpdatedBy,
                                LastUpdatedOn = groupHist.LastUpdatedOn,
                                CreatedBy = groupHist.CreatedBy,
                                CreatedOn = groupHist.CreatedOn
                            });
                        }
                    }
                }

                if (groupDocByGHCustId.GroupDocuments != null)
                {
                    foreach (var groupDoc in groupDocByGHCustId.GroupDocuments)
                    {
                        foreach (var groupHistory in groupDoc.GroupHistory)
                        {
                            if (groupHistory.CustomerId == customerId && groupHistory.ExitDate != null && groupHistory.ExitDate <= DateTime.UtcNow)
                            {
                                householdDetailsResponse.HouseholdHistory.Add(new HouseholdHistory()
                                {
                                    CustomerId = groupDoc.HeadOfGroupCustomerId,
                                    StartDate = groupHistory.StartDate,
                                    EndDate = groupHistory.ExitDate,
                                    RelationshipType = Constants.Application.ParentRelationshipType,
                                    GroupDocId = groupDoc.Id,
                                    LastUpdatedBy = groupHistory.LastUpdatedBy,
                                    LastUpdatedOn = groupHistory.LastUpdatedOn,
                                    CreatedBy = groupHistory.CreatedBy
                                });
                            }
                            else if (groupHistory.CustomerId == customerId && (groupHistory.ExitDate == null || groupHistory.ExitDate > DateTime.UtcNow))
                            {
                                householdDetailsResponse.HeadOfHousehold = new HeadOfHousehold()
                                {
                                    CustomerId = groupDoc.HeadOfGroupCustomerId,
                                    AssociationDate = groupHistory.StartDate,
                                    GroupDocId = groupDoc.Id,
                                    CreatedBy = groupHistory.CreatedBy,
                                    CreatedOn = groupHistory.CreatedOn
                                };
                            }
                        }
                    }
                }

                return new Response<HouseholdDetailsResponse>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = householdDetailsResponse
                };

            }

            catch (ArgumentException ex)
            {
                return Result.Failure<Response<HouseholdDetailsResponse>>(ex.Message);
            }

            catch (Exception ex)
            {

                return Result.Failure<Response<HouseholdDetailsResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }

        private async Task<Result<Response<HouseHoldGroup>>> GetGroupDocumentByHoHCustomerId(string customerId,CancellationToken cancellationToken)
        {
            try
            {
                return await _groupDocumentServiceQueryHandler.Handle(request: new GroupDocumentServiceQuery { CustomerId = customerId },cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Failure<Response<HouseHoldGroup>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }            
        }

        private async Task<Result<Response<GroupResponse>>> GetGroupDocumentByGHCustomerId(string customerId, CancellationToken cancellationToken)
        {
            try
            {
                return await _groupHistoryDocumentServiceQueryHandler.Handle(request: new GroupHistoryDocumentServiceQuery { CustomerId = customerId },cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Failure<Response<GroupResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
