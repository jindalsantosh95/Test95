using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Helpers
{
    public class AdvisoryInformationTests
    {
        [Fact]
        public void ExplicitOperator_WhenSourceIsNull_ReturnsNull()
        {
            AdvisoryNote source = null!;

            AdvisoryInformation? result = (AdvisoryInformation)source;

            Assert.Null(result);
        }

        [Fact]
        public void ExplicitOperator_WhenNoteTextIsNull_ReturnsNullInformationText()
        {
            var source = new AdvisoryNote
            {
                Priority = "H",
                NoteText = null
            };

            var result = (AdvisoryInformation)source;

            Assert.NotNull(result);
            Assert.Equal("H", result.Priority);
            Assert.Null(result.InformationText);
        }

        [Theory]
        [InlineData(
            "We strongly recommend customers purchase Travel Insurance\nIMMEDIATELY to\r\nensure they are adequately covered.",
            "We strongly recommend customers purchase Travel Insurance IMMEDIATELY to ensure they are adequately covered.")]
        [InlineData(
            "Cancellation and Amendment Policy :\r\nIf cancelled/amended between 24-Aug-2026 (00:00) and 06-Sep-2026 (23:59) there will be a 3526.55 GBP (4655.04 USD) charge \r\n",
            "Cancellation and Amendment Policy : If cancelled/amended between 24-Aug-2026 (00:00) and 06-Sep-2026 (23:59) there will be a 3526.55 GBP (4655.04 USD) charge")]
        [InlineData(
            "*** IMPORTANT INFORMATION FOR ROOM & TICKET BOOKINGS*** \n\nIf you have selected the 'ROOM & TICKET' option, please ensure you have \nselected a ticket type which is located under supplements at point of booking.\n \nIf you do not select a ticket type you will not have a ticket booked, yet may still \nbe charged. ",
            "*** IMPORTANT INFORMATION FOR ROOM & TICKET BOOKINGS*** If you have selected the 'ROOM & TICKET' option, please ensure you have selected a ticket type which is located under supplements at point of booking. If you do not select a ticket type you will not have a ticket booked, yet may still be charged.")]
        [InlineData(
            "\nSet as reconciled on 01/10/2023",
            "Set as reconciled on 01/10/2023")]
        public void ExplicitOperator_WhenNoteTextHasParagraphBreaks_NormalizesToSingleSpaces(string noteText, string expected)
        {
            var source = new AdvisoryNote
            {
                Priority = "H",
                NoteText = noteText
            };

            var result = (AdvisoryInformation)source;

            Assert.NotNull(result);
            Assert.Equal("H", result.Priority);
            Assert.Equal(expected, result.InformationText);
        }
    }
}
