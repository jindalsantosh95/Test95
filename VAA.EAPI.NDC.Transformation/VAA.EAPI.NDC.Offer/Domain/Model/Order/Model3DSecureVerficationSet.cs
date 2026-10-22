using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public partial class Model3DSecureVerficationSet : IEquatable<Model3DSecureVerficationSet>
    {
        public string TransactionReference { get; set; }
        public string Pareq { get; set; }
        public string Pares { get; set; }
        public string _3DResult { get; set; }
        public string ECI { get; set; }
        public string XID { get; set; }
        public string CAVV { get; set; }
        public string EnrolmentStatus { get; set; }
        public string MerchantData { get; set; }
        public string TransactionType { get; set; }
        public string CustomerAuthStatus { get; set; }
        public Link _3DSecureURL { get; set; }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Model3DSecureVerficationSet)obj);
        }
        public bool Equals(Model3DSecureVerficationSet other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    TransactionReference == other.TransactionReference ||
                    TransactionReference != null &&
                    TransactionReference.Equals(other.TransactionReference)
                ) &&
                (
                    Pareq == other.Pareq ||
                    Pareq != null &&
                    Pareq.Equals(other.Pareq)
                ) &&
                (
                    Pares == other.Pares ||
                    Pares != null &&
                    Pares.Equals(other.Pares)
                ) &&
                (
                    _3DResult == other._3DResult ||
                    _3DResult != null &&
                    _3DResult.Equals(other._3DResult)
                ) &&
                (
                    ECI == other.ECI ||
                    ECI != null &&
                    ECI.Equals(other.ECI)
                ) &&
                (
                    XID == other.XID ||
                    XID != null &&
                    XID.Equals(other.XID)
                ) &&
                (
                    CAVV == other.CAVV ||
                    CAVV != null &&
                    CAVV.Equals(other.CAVV)
                ) &&
                (
                    EnrolmentStatus == other.EnrolmentStatus ||
                    EnrolmentStatus != null &&
                    EnrolmentStatus.Equals(other.EnrolmentStatus)
                ) &&
                (
                    MerchantData == other.MerchantData ||
                    MerchantData != null &&
                    MerchantData.Equals(other.MerchantData)
                );
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (TransactionReference != null)
                    hashCode = hashCode * 59 + TransactionReference.GetHashCode();
                if (Pareq != null)
                    hashCode = hashCode * 59 + Pareq.GetHashCode();
                if (Pares != null)
                    hashCode = hashCode * 59 + Pares.GetHashCode();
                if (_3DResult != null)
                    hashCode = hashCode * 59 + _3DResult.GetHashCode();
                if (ECI != null)
                    hashCode = hashCode * 59 + ECI.GetHashCode();
                if (XID != null)
                    hashCode = hashCode * 59 + XID.GetHashCode();
                if (CAVV != null)
                    hashCode = hashCode * 59 + CAVV.GetHashCode();
                if (EnrolmentStatus != null)
                    hashCode = hashCode * 59 + EnrolmentStatus.GetHashCode();
                if (MerchantData != null)
                    hashCode = hashCode * 59 + MerchantData.GetHashCode();
                return hashCode;
            }
        }
    }
}
