using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.V3
{
    public sealed class AddressLookupV3Service : IAddressLookupV3Service
    {
        private const string AddressItemType = "Address";
        private const string ContainerItemType = "Container";
        private const string PostcodeItemType = "Postcode";
        private const int ContainerBatchSize = 5;
        private const int MaximumContainerCalls = 20;
        private const string ContractVersion = "v2";
        private const string CountryCodePattern = "^[A-Z]{2}$";
        private readonly IGbgV3Service gbgService;
        private readonly IAddressLookupV3ReferenceProtector referenceProtector;
        private readonly TelemetryClient telemetryClient;

        public AddressLookupV3Service(
            IGbgV3Service gbgService,
            IAddressLookupV3ReferenceProtector referenceProtector,
            TelemetryClient telemetryClient)
        {
            this.gbgService = gbgService ?? throw new ArgumentNullException(nameof(gbgService));
            this.referenceProtector = referenceProtector ?? throw new ArgumentNullException(nameof(referenceProtector));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        public async Task<AddressLookupV3SuggestionResponse> GetSuggestionsAsync(AddressLookupV3SuggestionRequest request, CancellationToken cancellationToken)
        {
            if (request is null) throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, "InvalidSuggestionRequest", "An address query or container reference is required.");

            cancellationToken.ThrowIfCancellationRequested();

            AddressLookupV3Reference parentReference = null;
            string flowId;
            string countryCode;
            string text = null;
            string container = null;

            if (request.Reference.IsNullOrWhiteSpace())
            {
                ValidateInitialSuggestion(request);
                flowId = Guid.NewGuid().ToString("N");
                countryCode = request.CountryCode.ToUpperInvariant();
                text = request.Query.Trim();
            }
            else
            {
                if (!request.Query.IsNullOrWhiteSpace() || !request.CountryCode.IsNullOrWhiteSpace())
                    throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, "InvalidSuggestionRequest", "A container reference cannot be combined with a query or country.");

                parentReference = ValidateReference(request.Reference, ContainerItemType, PostcodeItemType);
                flowId = parentReference.FlowId;
                countryCode = parentReference.CountryCode;
                container = parentReference.ProviderId;
            }

            AddressLookupV3Candidate[] candidates;
            var operation = parentReference is null ? "initial" : "container";
            var suggestionLimit = GetSuggestionLimit();
            var result = await FindAsync(new GbgV3FindRequest
            {
                Text = text,
                Container = container,
                CountryCode = countryCode,
                Limit = suggestionLimit
            }, flowId, operation, cancellationToken);
            var stage = "expansion";
            try
            {
                var addresses = await ExpandContainersAsync(result?.Items, container, flowId, countryCode, suggestionLimit, cancellationToken);
                stage = "references";
                candidates = CreateCandidates(addresses, flowId, countryCode).ToArray();
            }
            catch (OperationCanceledException)
            {
                TrackSuggestionProcessingOutcome(flowId, stage, "cancelled");
                throw;
            }
            catch
            {
                TrackSuggestionProcessingOutcome(flowId, stage, "failed");
                throw;
            }

            return new AddressLookupV3SuggestionResponse
            {
                FlowId = flowId,
                Candidates = candidates
            };
        }

        public async Task<AddressLookupV3SelectionResponse> GetSelectionAsync(AddressLookupV3SelectionRequest request, CancellationToken cancellationToken)
        {
            if (request is null || request.Reference.IsNullOrWhiteSpace())
                throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, "InvalidSelectionRequest", "An address reference is required.");

            cancellationToken.ThrowIfCancellationRequested();

            var reference = ValidateReference(request.Reference, AddressItemType);
            if (reference.SelectionIntentId.IsNullOrWhiteSpace())
                throw new AddressLookupV3Exception(HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");

            var dispatchId = Guid.NewGuid().ToString("N");

            try
            {
                var retrieveResult = await gbgService.RetrieveAsync(reference.ProviderId, cancellationToken);
                var address = AddressDetailDataMapper.GetAddressDetails(retrieveResult).FirstOrDefault();
                if (address is null)
                {
                    TrackRetrieveOutcome(reference, dispatchId, "not-found");
                    throw new AddressLookupV3Exception(HttpStatusCode.UnprocessableEntity, "AddressUnavailable", "The selected address is no longer available.");
                }

                TrackRetrieveOutcome(reference, dispatchId, "completed");

                return new AddressLookupV3SelectionResponse
                {
                    FlowId = reference.FlowId,
                    Address = address
                };
            }
            catch (OperationCanceledException)
            {
                TrackRetrieveOutcome(reference, dispatchId, "uncertain");
                throw;
            }
            catch (AddressLookupV3Exception exception)
            {
                if (!exception.Code.Is("AddressUnavailable"))
                {
                    TrackRetrieveOutcome(reference, dispatchId, "uncertain");
                }

                throw;
            }
            catch
            {
                TrackRetrieveOutcome(reference, dispatchId, "uncertain");
                throw new AddressLookupV3Exception(HttpStatusCode.ServiceUnavailable, "RetrieveOutcomeUnknown", "Address lookup is temporarily unavailable.");
            }
        }

        private async Task<IEnumerable<FindResponse>> ExpandContainersAsync(IEnumerable<FindResponse> items, string initialContainer,
            string flowId, string countryCode, int suggestionLimit, CancellationToken cancellationToken)
        {
            var addresses = new List<FindResponse>();
            var addressIds = new HashSet<string>(StringComparer.Ordinal);
            var containerIds = new HashSet<string>(StringComparer.Ordinal);
            var pendingContainers = new Queue<string>();
            if (!initialContainer.IsNullOrWhiteSpace()) containerIds.Add(initialContainer);

            void AddResults(IEnumerable<FindResponse> results)
            {
                foreach (var item in (results ?? Enumerable.Empty<FindResponse>())
                    .Where(item => item is not null && !item.Id.IsNullOrWhiteSpace() && !item.Type.IsNullOrWhiteSpace())
                    .Take(suggestionLimit))
                {
                    if (item.Type.Is(AddressItemType))
                    {
                        if (addressIds.Add(item.Id)) addresses.Add(item);
                    }
                    else if (containerIds.Add(item.Id)) pendingContainers.Enqueue(item.Id);
                }
            }

            AddResults(items);
            var containerCalls = 0;
            while (pendingContainers.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (containerCalls == MaximumContainerCalls)
                    throw new AddressLookupV3Exception(HttpStatusCode.UnprocessableEntity, "SearchTooBroad", "Please narrow your address search.");

                var batchSize = Math.Min(ContainerBatchSize, Math.Min(pendingContainers.Count, MaximumContainerCalls - containerCalls));
                var tasks = new List<Task<GbgResponse<FindResponse>>>(batchSize);
                for (var index = 0; index < batchSize; index++)
                {
                    tasks.Add(FindAsync(new GbgV3FindRequest
                    {
                        Container = pendingContainers.Dequeue(),
                        CountryCode = countryCode,
                        Limit = suggestionLimit
                    }, flowId, "container", cancellationToken));
                    containerCalls++;
                }

                var results = await Task.WhenAll(tasks);
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var result in results) AddResults(result?.Items);
            }

            return addresses;
        }

        private async Task<GbgResponse<FindResponse>> FindAsync(GbgV3FindRequest request, string flowId, string operation,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var result = await gbgService.FindAsync(request, cancellationToken);
                TrackFindOutcome(flowId, operation, "success");
                return result;
            }
            catch (OperationCanceledException)
            {
                TrackFindOutcome(flowId, operation, "cancelled");
                throw;
            }
            catch
            {
                TrackFindOutcome(flowId, operation, "failed");
                throw;
            }
        }

        private IEnumerable<AddressLookupV3Candidate> CreateCandidates(IEnumerable<FindResponse> items, string flowId, string countryCode)
        {
            return (items ?? Enumerable.Empty<FindResponse>())
                .Select(item => new AddressLookupV3Candidate
                {
                    Type = item.Type,
                    Text = item.Text,
                    Description = item.Description,
                    Reference = referenceProtector.Protect(new AddressLookupV3Reference
                    {
                        ContractVersion = ContractVersion,
                        ProviderId = item.Id,
                        ItemType = item.Type,
                        FlowId = flowId,
                        SelectionIntentId = item.Type.Is(AddressItemType) ? Guid.NewGuid().ToString("N") : null,
                        CountryCode = countryCode,
                        ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(GetReferenceLifetimeSeconds())
                    })
                });
        }

        private AddressLookupV3Reference ValidateReference(string protectedReference, params string[] permittedItemTypes)
        {
            var reference = referenceProtector.Unprotect(protectedReference);
            if (!reference.ContractVersion.Is(ContractVersion) ||
                reference.ProviderId.IsNullOrWhiteSpace() ||
                reference.ItemType.IsNullOrWhiteSpace() ||
                reference.FlowId.IsNullOrWhiteSpace() ||
                reference.CountryCode.IsNullOrWhiteSpace() ||
                reference.ExpiresAtUtc <= DateTimeOffset.UtcNow ||
                !permittedItemTypes.Any(itemType => itemType.Is(reference.ItemType)))
            {
                throw new AddressLookupV3Exception(HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");
            }

            return reference;
        }

        private static int GetSuggestionLimit()
        {
            var setting = ConfigReader.Get(Constants.Environment.AddressLookupV3SuggestionLimit);
            if (!int.TryParse(setting, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value < 1 || value > 20)
                throw new AddressLookupV3Exception(HttpStatusCode.ServiceUnavailable, "SuggestionLimitUnavailable", "Address lookup is temporarily unavailable.");

            return value;
        }

        private static int GetReferenceLifetimeSeconds()
        {
            var setting = ConfigReader.Get(Constants.Environment.AddressLookupV3ReferenceLifetimeSeconds);
            if (!int.TryParse(setting, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value < 1 || value > 300)
                throw new AddressLookupV3Exception(HttpStatusCode.ServiceUnavailable, "ReferenceLifetimeUnavailable", "Address lookup is temporarily unavailable.");

            return value;
        }

        private static void ValidateInitialSuggestion(AddressLookupV3SuggestionRequest request)
        {
            var minimumLengthSetting = ConfigReader.Get(Constants.Environment.AddressLookupV3MinQueryLength);
            if (!int.TryParse(minimumLengthSetting, NumberStyles.None, CultureInfo.InvariantCulture, out var minimumLength) || minimumLength < 1 || minimumLength > 20)
                throw new AddressLookupV3Exception(HttpStatusCode.ServiceUnavailable, "MinimumQueryLengthUnavailable", "Address lookup is temporarily unavailable.");

            if (request.Query.IsNullOrWhiteSpace() || request.Query.Trim().Length < minimumLength || request.CountryCode.IsNullOrWhiteSpace() || !Regex.IsMatch(request.CountryCode.ToUpperInvariant(), CountryCodePattern, RegexOptions.CultureInvariant))
                throw new AddressLookupV3Exception(HttpStatusCode.BadRequest, "InvalidSuggestionRequest", "A valid address query and country are required.");
        }

        private void TrackFindOutcome(string flowId, string operation, string outcome)
        {
            telemetryClient.TrackEvent("AddressLookupV3Find", new Dictionary<string, string>
            {
                ["flowId"] = flowId,
                ["operation"] = operation,
                ["outcome"] = outcome
            });
        }

        private void TrackSuggestionProcessingOutcome(string flowId, string stage, string outcome)
        {
            telemetryClient.TrackEvent("AddressLookupV3SuggestionProcessing", new Dictionary<string, string>
            {
                ["flowId"] = flowId,
                ["stage"] = stage,
                ["outcome"] = outcome
            });
        }

        private void TrackRetrieveOutcome(AddressLookupV3Reference reference, string dispatchId, string outcome)
        {
            telemetryClient.TrackEvent("AddressLookupV3Retrieve", new Dictionary<string, string>
            {
                ["flowId"] = reference.FlowId,
                ["selectionIntentId"] = reference.SelectionIntentId,
                ["dispatchId"] = dispatchId,
                ["outcome"] = outcome
            });
        }
    }
}
