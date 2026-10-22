using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using VAA.EAPI.GraphQL.Common.Extensions;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData(ContractValue.PlainMember, "PlainMember", "plain_member")]
        [InlineData(ContractValue.EnumMemberOnly, "CardPayment", "card_payment")]
        [InlineData(ContractValue.DisplayOnly, "PaymentPending", "payment_pending")]
        [InlineData(ContractValue.BothAttributes, "ApprovedPayment", "approved_payment")]
        [InlineData(ContractValue.EmptyEnumMember, "", "")]
        public void ToDisplayName_PreservesContractAndAppliesNamingStrategy(
            ContractValue value, string expectedDefault, string expectedSnakeCase)
        {
            var member = typeof(ContractValue).GetField(value.ToString())!;

            Assert.Equal(expectedDefault, value.ToDisplayName());
            Assert.Equal(expectedDefault, member.ToDisplayName());
            Assert.Equal(expectedDefault, value.ToDisplayName(new DefaultNamingStrategy()));
            Assert.Equal(expectedDefault, member.ToDisplayName(new DefaultNamingStrategy()));
            Assert.Equal(expectedSnakeCase, value.ToDisplayName(new SnakeCaseNamingStrategy()));
            Assert.Equal(expectedSnakeCase, member.ToDisplayName(new SnakeCaseNamingStrategy()));
        }

        [Fact]
        public void ToDisplayName_RejectsNullMember()
        {
            var error = Assert.Throws<ArgumentNullException>(() => ((MemberInfo)null!).ToDisplayName());

            Assert.Equal("element", error.ParamName);
        }

        public enum ContractValue
        {
            PlainMember,
            [EnumMember(Value = "CardPayment")]
            EnumMemberOnly,
            [Display(Name = "PaymentPending")]
            DisplayOnly,
            [EnumMember(Value = "ApprovedPayment")]
            [Display(Name = "IgnoredDisplay")]
            BothAttributes,
            [EnumMember(Value = "")]
            [Display(Name = "MustNotReplaceEmptyEnumMember")]
            EmptyEnumMember
        }
    }
}
