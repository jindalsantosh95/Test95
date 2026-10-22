using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class PassengerDetails
    {
        public PassengerDetailsRequest PassengerInformation { get; set; }


        public static explicit operator PassengerDetails((LoyaltyMemberDetailsRequest source, ActionCodeType actionCodeType, FQTProgramRefData programDetails, string msBookingId, IEnumerable<LoyaltyProgramAccount> existingLoyaltyProgramAccounts) input)
        {
            var (source, actionCodeType, programDetails, msBookingId, existingLoyaltyProgramAccounts) = input;
            if (source == null) return null;

            return new()
            {
                PassengerInformation = new PassengerDetailsRequest
                {
                    bookingId = msBookingId,

                    Passengers =
                    [
                        new Passenger
                        {
                            PassengerResourceId = source.PassengerId,
                            ActionType = actionCodeType,
                            LoyaltyProgramAccounts = actionCodeType != ActionCodeType.ADD ?
                            [
                                new() {
                                    Number = existingLoyaltyProgramAccounts?.FirstOrDefault()?.Number,
                                    LoyaltyProgram = new LoyaltyProgram
                                    {
                                        Owner =
                                        [
                                            new Carrier
                                            {
                                                Code =  existingLoyaltyProgramAccounts?.FirstOrDefault()?.LoyaltyProgram?.Owner?.FirstOrDefault()?.Code,
                                                Name =   existingLoyaltyProgramAccounts?.FirstOrDefault() ?.LoyaltyProgram ?.Name
                                            }
                                        ]
                                    }
                                }
                            ]:
                            [
                                new() {
                                    Number = source.LoyaltyMemberId,
                                    LoyaltyProgram = new LoyaltyProgram
                                    {
                                        Owner =
                                        [
                                            new Carrier
                                            {
                                                Code = programDetails?.ProgramCode,
                                                Name = programDetails?.ProgramName
                                            }
                                        ]
                                    }
                                }
                            ]
                        }
                    ]
                }
            };
        }
    }
}