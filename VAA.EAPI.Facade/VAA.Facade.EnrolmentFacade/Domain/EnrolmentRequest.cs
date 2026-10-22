using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using VAA.Facade.EnrolmentFacade.Domain.Channel;
using VAA.Facade.EnrolmentFacade.Domain.CustomerDetails;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;

namespace VAA.Facade.EnrolmentFacade.Domain
{
    [DataContract]
    public partial class EnrolmentRequest : ChannelUser, IEquatable<EnrolmentRequest>
    {
        /// <summary>
        /// Gets or Sets CustomerDetails
        /// </summary>

        [DataMember(Name = "customerDetails")]
        public EnrolmentFacade.Domain.CustomerDetails.Customer CustomerDetails { get; set; }

        /// <summary>
        /// Customer address details
        /// </summary>
        /// <value>Customer address details</value>

        [DataMember(Name = "securityQuestions")]
        public List<SecurityQuestions> SecurityQuestions { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class EnrolmentRequest {\n");
            sb.Append("  CustomerDetails: ").Append(CustomerDetails).Append("\n");
            sb.Append("  SecurityQuestions: ").Append(SecurityQuestions).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            return obj is null ? false : ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((EnrolmentRequest)obj);
        }

        /// <summary>
        /// Returns true if EnrolmentRequest instances are equal
        /// </summary>
        /// <param name="other">Instance of EnrolmentRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(EnrolmentRequest other)
        {
            return other is null
                ? false
                : ReferenceEquals(this, other)
                ? true
                : (
                    CustomerDetails == other.CustomerDetails ||
                    CustomerDetails != null &&
                    CustomerDetails.Equals(other.CustomerDetails)
                ) &&
                (
                    SecurityQuestions == other.SecurityQuestions ||
                    SecurityQuestions != null &&
                    SecurityQuestions.SequenceEqual(other.SecurityQuestions)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (CustomerDetails != null)
                {
                    hashCode = hashCode * 59 + CustomerDetails.GetHashCode();
                }
                if (SecurityQuestions != null)
                {
                    hashCode = hashCode * 59 + SecurityQuestions.GetHashCode();
                }
                return hashCode;
            }
        }

        #region Operators

        public static bool operator ==(EnrolmentRequest left, EnrolmentRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EnrolmentRequest left, EnrolmentRequest right)
        {
            return !Equals(left, right);
        }
        #endregion Operators
    }
}
