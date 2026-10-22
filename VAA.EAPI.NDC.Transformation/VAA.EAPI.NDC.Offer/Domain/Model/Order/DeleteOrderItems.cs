using System;
using System.Text;
using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public partial class DeleteOrderItems : IEquatable<DeleteOrderItems>
    {
        public OrderItemNumber OrderItemReshop { get; set; }
        public IEnumerable<string> ProductRetention { get; set; }
        public IEnumerable<string> FlightRetention { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("}\n");
            return sb.ToString();
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DeleteOrderItems)obj);
        }
        public bool Equals(DeleteOrderItems other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return true;

        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;

                return hashCode;
            }
        }
        public static bool operator ==(DeleteOrderItems left, DeleteOrderItems right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DeleteOrderItems left, DeleteOrderItems right)
        {
            return !Equals(left, right);
        }




    }
}
