namespace VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignatureType
    {

        private SignedInfoType signedInfoField;

        private SignatureValueType signatureValueField;

        private KeyInfoType keyInfoField;

        private ObjectType[] objectField;

        private string idField;

        /// <remarks/>
        public SignedInfoType SignedInfo
        {
            get
            {
                return signedInfoField;
            }
            set
            {
                signedInfoField = value;
            }
        }

        /// <remarks/>
        public SignatureValueType SignatureValue
        {
            get
            {
                return signatureValueField;
            }
            set
            {
                signatureValueField = value;
            }
        }

        /// <remarks/>
        public KeyInfoType KeyInfo
        {
            get
            {
                return keyInfoField;
            }
            set
            {
                keyInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Object")]
        public ObjectType[] Object
        {
            get
            {
                return objectField;
            }
            set
            {
                objectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("SignedInfo", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignedInfoType
    {

        private CanonicalizationMethodType canonicalizationMethodField;

        private SignatureMethodType signatureMethodField;

        private ReferenceType[] referenceField;

        private string idField;

        /// <remarks/>
        public CanonicalizationMethodType CanonicalizationMethod
        {
            get
            {
                return canonicalizationMethodField;
            }
            set
            {
                canonicalizationMethodField = value;
            }
        }

        /// <remarks/>
        public SignatureMethodType SignatureMethod
        {
            get
            {
                return signatureMethodField;
            }
            set
            {
                signatureMethodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Reference")]
        public ReferenceType[] Reference
        {
            get
            {
                return referenceField;
            }
            set
            {
                referenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("CanonicalizationMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class CanonicalizationMethodType
    {

        private System.Xml.XmlNode[] anyField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        [System.Xml.Serialization.XmlAnyElement()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return algorithmField;
            }
            set
            {
                algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class X509IssuerSerialType
    {

        private string x509IssuerNameField;

        private string x509SerialNumberField;

        /// <remarks/>
        public string X509IssuerName
        {
            get
            {
                return x509IssuerNameField;
            }
            set
            {
                x509IssuerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "integer")]
        public string X509SerialNumber
        {
            get
            {
                return x509SerialNumberField;
            }
            set
            {
                x509SerialNumberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("SignatureMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignatureMethodType
    {

        private string hMACOutputLengthField;

        private System.Xml.XmlNode[] anyField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "integer")]
        public string HMACOutputLength
        {
            get
            {
                return hMACOutputLengthField;
            }
            set
            {
                hMACOutputLengthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        [System.Xml.Serialization.XmlAnyElement()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return algorithmField;
            }
            set
            {
                algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("Reference", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class ReferenceType
    {

        private TransformType[] transformsField;

        private DigestMethodType digestMethodField;

        private byte[] digestValueField;

        private string idField;

        private string uRIField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Transform", IsNullable = false)]
        public TransformType[] Transforms
        {
            get
            {
                return transformsField;
            }
            set
            {
                transformsField = value;
            }
        }

        /// <remarks/>
        public DigestMethodType DigestMethod
        {
            get
            {
                return digestMethodField;
            }
            set
            {
                digestMethodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] DigestValue
        {
            get
            {
                return digestValueField;
            }
            set
            {
                digestValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string URI
        {
            get
            {
                return uRIField;
            }
            set
            {
                uRIField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Type
        {
            get
            {
                return typeField;
            }
            set
            {
                typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("Transform", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class TransformType
    {

        private object[] itemsField;

        private string[] textField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        [System.Xml.Serialization.XmlElement("XPath", typeof(string))]
        public object[] Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string[] Text
        {
            get
            {
                return textField;
            }
            set
            {
                textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return algorithmField;
            }
            set
            {
                algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("DigestMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class DigestMethodType
    {

        private System.Xml.XmlNode[] anyField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        [System.Xml.Serialization.XmlAnyElement()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return algorithmField;
            }
            set
            {
                algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("SignatureValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignatureValueType
    {

        private string idField;

        private byte[] valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText(DataType = "base64Binary")]
        public byte[] Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("KeyInfo", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class KeyInfoType
    {

        private object[] itemsField;

        private ItemsChoiceType2[] itemsElementNameField;

        private string[] textField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        [System.Xml.Serialization.XmlElement("KeyName", typeof(string))]
        [System.Xml.Serialization.XmlElement("KeyValue", typeof(KeyValueType))]
        [System.Xml.Serialization.XmlElement("MgmtData", typeof(string))]
        [System.Xml.Serialization.XmlElement("PGPData", typeof(PGPDataType))]
        [System.Xml.Serialization.XmlElement("RetrievalMethod", typeof(RetrievalMethodType))]
        [System.Xml.Serialization.XmlElement("SPKIData", typeof(SPKIDataType))]
        [System.Xml.Serialization.XmlElement("X509Data", typeof(X509DataType))]
        [System.Xml.Serialization.XmlChoiceIdentifier("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnore()]
        public ItemsChoiceType2[] ItemsElementName
        {
            get
            {
                return itemsElementNameField;
            }
            set
            {
                itemsElementNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string[] Text
        {
            get
            {
                return textField;
            }
            set
            {
                textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("KeyValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class KeyValueType
    {

        private object itemField;

        private string[] textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        [System.Xml.Serialization.XmlElement("DSAKeyValue", typeof(DSAKeyValueType))]
        [System.Xml.Serialization.XmlElement("RSAKeyValue", typeof(RSAKeyValueType))]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string[] Text
        {
            get
            {
                return textField;
            }
            set
            {
                textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("DSAKeyValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class DSAKeyValueType
    {

        private byte[] pField;

        private byte[] qField;

        private byte[] gField;

        private byte[] yField;

        private byte[] jField;

        private byte[] seedField;

        private byte[] pgenCounterField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] P
        {
            get
            {
                return pField;
            }
            set
            {
                pField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] Q
        {
            get
            {
                return qField;
            }
            set
            {
                qField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] G
        {
            get
            {
                return gField;
            }
            set
            {
                gField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] Y
        {
            get
            {
                return yField;
            }
            set
            {
                yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] J
        {
            get
            {
                return jField;
            }
            set
            {
                jField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] Seed
        {
            get
            {
                return seedField;
            }
            set
            {
                seedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] PgenCounter
        {
            get
            {
                return pgenCounterField;
            }
            set
            {
                pgenCounterField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("RSAKeyValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class RSAKeyValueType
    {

        private byte[] modulusField;

        private byte[] exponentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] Modulus
        {
            get
            {
                return modulusField;
            }
            set
            {
                modulusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "base64Binary")]
        public byte[] Exponent
        {
            get
            {
                return exponentField;
            }
            set
            {
                exponentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("PGPData", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class PGPDataType
    {

        private object[] itemsField;

        private ItemsChoiceType1[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        [System.Xml.Serialization.XmlElement("PGPKeyID", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElement("PGPKeyPacket", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlChoiceIdentifier("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnore()]
        public ItemsChoiceType1[] ItemsElementName
        {
            get
            {
                return itemsElementNameField;
            }
            set
            {
                itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#", IncludeInSchema = false)]
    public enum ItemsChoiceType1
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("##any:")]
        Item,

        /// <remarks/>
        PGPKeyID,

        /// <remarks/>
        PGPKeyPacket,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("RetrievalMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class RetrievalMethodType
    {

        private TransformType[] transformsField;

        private string uRIField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Transform", IsNullable = false)]
        public TransformType[] Transforms
        {
            get
            {
                return transformsField;
            }
            set
            {
                transformsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string URI
        {
            get
            {
                return uRIField;
            }
            set
            {
                uRIField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Type
        {
            get
            {
                return typeField;
            }
            set
            {
                typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("SPKIData", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SPKIDataType
    {

        private byte[] sPKISexpField;

        private System.Xml.XmlElement anyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SPKISexp", DataType = "base64Binary")]
        public byte[] SPKISexp
        {
            get
            {
                return sPKISexpField;
            }
            set
            {
                sPKISexpField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        public System.Xml.XmlElement Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("X509Data", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class X509DataType
    {

        private object[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        [System.Xml.Serialization.XmlElement("X509CRL", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElement("X509Certificate", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElement("X509IssuerSerial", typeof(X509IssuerSerialType))]
        [System.Xml.Serialization.XmlElement("X509SKI", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElement("X509SubjectName", typeof(string))]
        [System.Xml.Serialization.XmlChoiceIdentifier("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnore()]
        public ItemsChoiceType[] ItemsElementName
        {
            get
            {
                return itemsElementNameField;
            }
            set
            {
                itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#", IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("##any:")]
        Item,

        /// <remarks/>
        X509CRL,

        /// <remarks/>
        X509Certificate,

        /// <remarks/>
        X509IssuerSerial,

        /// <remarks/>
        X509SKI,

        /// <remarks/>
        X509SubjectName,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#", IncludeInSchema = false)]
    public enum ItemsChoiceType2
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("##any:")]
        Item,

        /// <remarks/>
        KeyName,

        /// <remarks/>
        KeyValue,

        /// <remarks/>
        MgmtData,

        /// <remarks/>
        PGPData,

        /// <remarks/>
        RetrievalMethod,

        /// <remarks/>
        SPKIData,

        /// <remarks/>
        X509Data,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("Object", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class ObjectType
    {

        private System.Xml.XmlNode[] anyField;

        private string idField;

        private string mimeTypeField;

        private string encodingField;

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        [System.Xml.Serialization.XmlAnyElement()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string MimeType
        {
            get
            {
                return mimeTypeField;
            }
            set
            {
                mimeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Encoding
        {
            get
            {
                return encodingField;
            }
            set
            {
                encodingField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("Transforms", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class TransformsType
    {

        private TransformType[] transformField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Transform")]
        public TransformType[] Transform
        {
            get
            {
                return transformField;
            }
            set
            {
                transformField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("Manifest", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class ManifestType
    {

        private ReferenceType[] referenceField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Reference")]
        public ReferenceType[] Reference
        {
            get
            {
                return referenceField;
            }
            set
            {
                referenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("SignatureProperties", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignaturePropertiesType
    {

        private SignaturePropertyType[] signaturePropertyField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SignatureProperty")]
        public SignaturePropertyType[] SignatureProperty
        {
            get
            {
                return signaturePropertyField;
            }
            set
            {
                signaturePropertyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRoot("SignatureProperty", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignaturePropertyType
    {

        private System.Xml.XmlElement[] itemsField;

        private string[] textField;

        private string targetField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        public System.Xml.XmlElement[] Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string[] Text
        {
            get
            {
                return textField;
            }
            set
            {
                textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string Target
        {
            get
            {
                return targetField;
            }
            set
            {
                targetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage", IsNullable = false)]
    public partial class IATA_ServiceListRQ
    {

        private AugmentationPointType augmentationPointField;

        private DistributionChainLinkType[] distributionChainField;

        private IATA_PayloadStandardAttributesType payloadAttributesField;

        private POS_Type pOSField;

        private ServiceListRequestType requestField;

        /// <remarks/>
        public AugmentationPointType AugmentationPoint
        {
            get
            {
                return augmentationPointField;
            }
            set
            {
                augmentationPointField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DistributionChainLink", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes", IsNullable = false)]
        public DistributionChainLinkType[] DistributionChain
        {
            get
            {
                return distributionChainField;
            }
            set
            {
                distributionChainField = value;
            }
        }

        /// <remarks/>
        public IATA_PayloadStandardAttributesType PayloadAttributes
        {
            get
            {
                return payloadAttributesField;
            }
            set
            {
                payloadAttributesField = value;
            }
        }

        /// <remarks/>
        public POS_Type POS
        {
            get
            {
                return pOSField;
            }
            set
            {
                pOSField = value;
            }
        }

        /// <remarks/>
        public ServiceListRequestType Request
        {
            get
            {
                return requestField;
            }
            set
            {
                requestField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AugmentationPointType
    {

        private System.Xml.XmlElement[] anyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DistributionChainLinkType
    {

        private ContactInfoType contactInfoField;

        private string ordinalField;

        private DistributionChainRoleCodeType orgRoleField;

        private ParticipatingOrgType participatingOrgField;

        private SalesAgentType salesAgentField;

        private SalesBranchType salesBranchField;

        /// <remarks/>
        public ContactInfoType ContactInfo
        {
            get
            {
                return contactInfoField;
            }
            set
            {
                contactInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "integer")]
        public string Ordinal
        {
            get
            {
                return ordinalField;
            }
            set
            {
                ordinalField = value;
            }
        }

        /// <remarks/>
        public DistributionChainRoleCodeType OrgRole
        {
            get
            {
                return orgRoleField;
            }
            set
            {
                orgRoleField = value;
            }
        }

        /// <remarks/>
        public ParticipatingOrgType ParticipatingOrg
        {
            get
            {
                return participatingOrgField;
            }
            set
            {
                participatingOrgField = value;
            }
        }

        /// <remarks/>
        public SalesAgentType SalesAgent
        {
            get
            {
                return salesAgentField;
            }
            set
            {
                salesAgentField = value;
            }
        }

        /// <remarks/>
        public SalesBranchType SalesBranch
        {
            get
            {
                return salesBranchField;
            }
            set
            {
                salesBranchField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ContactInfoType
    {

        private string contactInfoIDField;

        private string[] contactPurposeTextField;

        private bool contactRefusedIndField;

        private bool contactRefusedIndFieldSpecified;

        private EmailAddressType[] emailAddressField;

        private IndividualType individualField;

        private string individualRefIDField;

        private OtherAddressType[] otherAddressField;

        private string paxSegmentRefIDField;

        private PhoneType[] phoneField;

        private PostalAddressType[] postalAddressField;

        private string relationshipToPaxField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ContactInfoID
        {
            get
            {
                return contactInfoIDField;
            }
            set
            {
                contactInfoIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ContactPurposeText")]
        public string[] ContactPurposeText
        {
            get
            {
                return contactPurposeTextField;
            }
            set
            {
                contactPurposeTextField = value;
            }
        }

        /// <remarks/>
        public bool ContactRefusedInd
        {
            get
            {
                return contactRefusedIndField;
            }
            set
            {
                contactRefusedIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ContactRefusedIndSpecified
        {
            get
            {
                return contactRefusedIndFieldSpecified;
            }
            set
            {
                contactRefusedIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("EmailAddress")]
        public EmailAddressType[] EmailAddress
        {
            get
            {
                return emailAddressField;
            }
            set
            {
                emailAddressField = value;
            }
        }

        /// <remarks/>
        public IndividualType Individual
        {
            get
            {
                return individualField;
            }
            set
            {
                individualField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IndividualRefID
        {
            get
            {
                return individualRefIDField;
            }
            set
            {
                individualRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OtherAddress")]
        public OtherAddressType[] OtherAddress
        {
            get
            {
                return otherAddressField;
            }
            set
            {
                otherAddressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxSegmentRefID
        {
            get
            {
                return paxSegmentRefIDField;
            }
            set
            {
                paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Phone")]
        public PhoneType[] Phone
        {
            get
            {
                return phoneField;
            }
            set
            {
                phoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PostalAddress")]
        public PostalAddressType[] PostalAddress
        {
            get
            {
                return postalAddressField;
            }
            set
            {
                postalAddressField = value;
            }
        }

        /// <remarks/>
        public string RelationshipToPax
        {
            get
            {
                return relationshipToPaxField;
            }
            set
            {
                relationshipToPaxField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class EmailAddressType
    {

        private string contactTypeTextField;

        private string emailAddressTextField;

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return contactTypeTextField;
            }
            set
            {
                contactTypeTextField = value;
            }
        }

        /// <remarks/>
        public string EmailAddressText
        {
            get
            {
                return emailAddressTextField;
            }
            set
            {
                emailAddressTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IndividualType
    {

        private AddlNameType[] addlNameField;

        private System.DateTime birthdateField;

        private bool birthdateFieldSpecified;

        private string birthplaceTextField;

        private GenderCodeType genderCodeField;

        private bool genderCodeFieldSpecified;

        private string[] givenNameField;

        private string individualIDField;

        private string[] middleNameField;

        private string suffixNameField;

        private string surnameField;

        private string titleNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AddlName")]
        public AddlNameType[] AddlName
        {
            get
            {
                return addlNameField;
            }
            set
            {
                addlNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime Birthdate
        {
            get
            {
                return birthdateField;
            }
            set
            {
                birthdateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool BirthdateSpecified
        {
            get
            {
                return birthdateFieldSpecified;
            }
            set
            {
                birthdateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string BirthplaceText
        {
            get
            {
                return birthplaceTextField;
            }
            set
            {
                birthplaceTextField = value;
            }
        }

        /// <remarks/>
        public GenderCodeType GenderCode
        {
            get
            {
                return genderCodeField;
            }
            set
            {
                genderCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool GenderCodeSpecified
        {
            get
            {
                return genderCodeFieldSpecified;
            }
            set
            {
                genderCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("GivenName", DataType = "token")]
        public string[] GivenName
        {
            get
            {
                return givenNameField;
            }
            set
            {
                givenNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IndividualID
        {
            get
            {
                return individualIDField;
            }
            set
            {
                individualIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("MiddleName", DataType = "token")]
        public string[] MiddleName
        {
            get
            {
                return middleNameField;
            }
            set
            {
                middleNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SuffixName
        {
            get
            {
                return suffixNameField;
            }
            set
            {
                suffixNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Surname
        {
            get
            {
                return surnameField;
            }
            set
            {
                surnameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TitleName
        {
            get
            {
                return titleNameField;
            }
            set
            {
                titleNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AddlNameType
    {

        private string nameField;

        private AddlNameTypeCodeType nameTypeCodeField;

        private bool nameTypeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        public AddlNameTypeCodeType NameTypeCode
        {
            get
            {
                return nameTypeCodeField;
            }
            set
            {
                nameTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool NameTypeCodeSpecified
        {
            get
            {
                return nameTypeCodeFieldSpecified;
            }
            set
            {
                nameTypeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum AddlNameTypeCodeType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("Father Surname")]
        FatherSurname,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("Mother Surname")]
        MotherSurname,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum GenderCodeType
    {

        /// <remarks/>
        F,

        /// <remarks/>
        M,

        /// <remarks/>
        U,

        /// <remarks/>
        X,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OtherAddressType
    {

        private string contactTypeTextField;

        private string otherAddressTextField;

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return contactTypeTextField;
            }
            set
            {
                contactTypeTextField = value;
            }
        }

        /// <remarks/>
        public string OtherAddressText
        {
            get
            {
                return otherAddressTextField;
            }
            set
            {
                otherAddressTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PhoneType
    {

        private string areaCodeNumberField;

        private string contactTypeTextField;

        private string countryDialingCodeField;

        private string extensionNumberField;

        private string phoneNumberField;

        /// <remarks/>
        public string AreaCodeNumber
        {
            get
            {
                return areaCodeNumberField;
            }
            set
            {
                areaCodeNumberField = value;
            }
        }

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return contactTypeTextField;
            }
            set
            {
                contactTypeTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryDialingCode
        {
            get
            {
                return countryDialingCodeField;
            }
            set
            {
                countryDialingCodeField = value;
            }
        }

        /// <remarks/>
        public string ExtensionNumber
        {
            get
            {
                return extensionNumberField;
            }
            set
            {
                extensionNumberField = value;
            }
        }

        /// <remarks/>
        public string PhoneNumber
        {
            get
            {
                return phoneNumberField;
            }
            set
            {
                phoneNumberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PostalAddressType
    {

        private string buildingRoomTextField;

        private string cityNameField;

        private string contactTypeTextField;

        private string countryCodeField;

        private string countryNameField;

        private string countrySubDivisionNameField;

        private string pO_BoxCodeField;

        private string postalCodeField;

        private string[] streetTextField;

        /// <remarks/>
        public string BuildingRoomText
        {
            get
            {
                return buildingRoomTextField;
            }
            set
            {
                buildingRoomTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CityName
        {
            get
            {
                return cityNameField;
            }
            set
            {
                cityNameField = value;
            }
        }

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return contactTypeTextField;
            }
            set
            {
                contactTypeTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return countryCodeField;
            }
            set
            {
                countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryName
        {
            get
            {
                return countryNameField;
            }
            set
            {
                countryNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountrySubDivisionName
        {
            get
            {
                return countrySubDivisionNameField;
            }
            set
            {
                countrySubDivisionNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PO_BoxCode
        {
            get
            {
                return pO_BoxCodeField;
            }
            set
            {
                pO_BoxCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PostalCode
        {
            get
            {
                return postalCodeField;
            }
            set
            {
                postalCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("StreetText")]
        public string[] StreetText
        {
            get
            {
                return streetTextField;
            }
            set
            {
                streetTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum DistributionChainRoleCodeType
    {

        /// <remarks/>
        Carrier,

        /// <remarks/>
        Distributor,

        /// <remarks/>
        Seller,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ParticipatingOrgType
    {

        private string nameField;

        private string orgIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OrgID
        {
            get
            {
                return orgIDField;
            }
            set
            {
                orgIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SalesAgentType
    {

        private string salesAgentIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SalesAgentID
        {
            get
            {
                return salesAgentIDField;
            }
            set
            {
                salesAgentIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SalesBranchType
    {

        private string salesBranchIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SalesBranchID
        {
            get
            {
                return salesBranchIDField;
            }
            set
            {
                salesBranchIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IATA_PayloadStandardAttributesType
    {

        private string altLangIDField;

        private string correlationIDField;

        private string primaryLangIDField;

        private bool retransmissionIndField;

        private bool retransmissionIndFieldSpecified;

        private decimal seqNumberField;

        private bool seqNumberFieldSpecified;

        private System.DateTime timestampField;

        private bool timestampFieldSpecified;

        private string trxIDField;

        private decimal versionNumberField;

        private bool versionNumberFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AltLangID
        {
            get
            {
                return altLangIDField;
            }
            set
            {
                altLangIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CorrelationID
        {
            get
            {
                return correlationIDField;
            }
            set
            {
                correlationIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PrimaryLangID
        {
            get
            {
                return primaryLangIDField;
            }
            set
            {
                primaryLangIDField = value;
            }
        }

        /// <remarks/>
        public bool RetransmissionInd
        {
            get
            {
                return retransmissionIndField;
            }
            set
            {
                retransmissionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RetransmissionIndSpecified
        {
            get
            {
                return retransmissionIndFieldSpecified;
            }
            set
            {
                retransmissionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal SeqNumber
        {
            get
            {
                return seqNumberField;
            }
            set
            {
                seqNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool SeqNumberSpecified
        {
            get
            {
                return seqNumberFieldSpecified;
            }
            set
            {
                seqNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime Timestamp
        {
            get
            {
                return timestampField;
            }
            set
            {
                timestampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TimestampSpecified
        {
            get
            {
                return timestampFieldSpecified;
            }
            set
            {
                timestampFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TrxID
        {
            get
            {
                return trxIDField;
            }
            set
            {
                trxIDField = value;
            }
        }

        /// <remarks/>
        public decimal VersionNumber
        {
            get
            {
                return versionNumberField;
            }
            set
            {
                versionNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool VersionNumberSpecified
        {
            get
            {
                return versionNumberFieldSpecified;
            }
            set
            {
                versionNumberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class POS_Type
    {

        private string agentDutyTextField;

        private CityType cityField;

        private CountryType countryField;

        private bool onlineTrxIndField;

        private bool onlineTrxIndFieldSpecified;

        private System.DateTime requestTimeField;

        private bool requestTimeFieldSpecified;

        /// <remarks/>
        public string AgentDutyText
        {
            get
            {
                return agentDutyTextField;
            }
            set
            {
                agentDutyTextField = value;
            }
        }

        /// <remarks/>
        public CityType City
        {
            get
            {
                return cityField;
            }
            set
            {
                cityField = value;
            }
        }

        /// <remarks/>
        public CountryType Country
        {
            get
            {
                return countryField;
            }
            set
            {
                countryField = value;
            }
        }

        /// <remarks/>
        public bool OnlineTrxInd
        {
            get
            {
                return onlineTrxIndField;
            }
            set
            {
                onlineTrxIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool OnlineTrxIndSpecified
        {
            get
            {
                return onlineTrxIndFieldSpecified;
            }
            set
            {
                onlineTrxIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime RequestTime
        {
            get
            {
                return requestTimeField;
            }
            set
            {
                requestTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RequestTimeSpecified
        {
            get
            {
                return requestTimeFieldSpecified;
            }
            set
            {
                requestTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CityType
    {

        private string cityNameField;

        private string iATA_LocationCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CityName
        {
            get
            {
                return cityNameField;
            }
            set
            {
                cityNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IATA_LocationCode
        {
            get
            {
                return iATA_LocationCodeField;
            }
            set
            {
                iATA_LocationCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CountryType
    {

        private string countryCodeField;

        private string countryNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return countryCodeField;
            }
            set
            {
                countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryName
        {
            get
            {
                return countryNameField;
            }
            set
            {
                countryNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceListRequestType
    {

        private ContactInfoType[] contactInfoListField;

        private DatedMarketingSegmentType[] datedMarketingSegmentListField;

        private DatedOperatingLegType[] datedOperatingLegListField;

        private DatedOperatingSegmentType[] datedOperatingSegmentListField;

        private FareWaiverType[] fareWaiverField;

        private MetadataType metadataField;

        private OfferCriteriaType offerCriteriaField;

        private PaxType[] paxField;

        private PaxSegmentType[] paxSegmentListField;

        private PaymentFunctionsCriteriaType[] paymentFunctionsField;

        private PolicyType[] policyField;

        private ResponseParametersRequestType responseParametersField;

        private ServiceListCoreRequestType serviceListCoreRequestField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("ContactInfo", typeof(ContactInfoType), IsNullable = false)]
        public ContactInfoType[] ContactInfoList
        {
            get
            {
                return contactInfoListField;
            }
            set
            {
                contactInfoListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DatedMarketingSegment", IsNullable = false)]
        public DatedMarketingSegmentType[] DatedMarketingSegmentList
        {
            get
            {
                return datedMarketingSegmentListField;
            }
            set
            {
                datedMarketingSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DatedOperatingLeg", IsNullable = false)]
        public DatedOperatingLegType[] DatedOperatingLegList
        {
            get
            {
                return datedOperatingLegListField;
            }
            set
            {
                datedOperatingLegListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DatedOperatingSegment", IsNullable = false)]
        public DatedOperatingSegmentType[] DatedOperatingSegmentList
        {
            get
            {
                return datedOperatingSegmentListField;
            }
            set
            {
                datedOperatingSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FareWaiver")]
        public FareWaiverType[] FareWaiver
        {
            get
            {
                return fareWaiverField;
            }
            set
            {
                fareWaiverField = value;
            }
        }

        /// <remarks/>
        public MetadataType Metadata
        {
            get
            {
                return metadataField;
            }
            set
            {
                metadataField = value;
            }
        }

        /// <remarks/>
        public OfferCriteriaType OfferCriteria
        {
            get
            {
                return offerCriteriaField;
            }
            set
            {
                offerCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Pax")]
        public PaxType[] Pax
        {
            get
            {
                return paxField;
            }
            set
            {
                paxField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("PaxSegment", IsNullable = false)]
        public PaxSegmentType[] PaxSegmentList
        {
            get
            {
                return paxSegmentListField;
            }
            set
            {
                paxSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaymentFunctions")]
        public PaymentFunctionsCriteriaType[] PaymentFunctions
        {
            get
            {
                return paymentFunctionsField;
            }
            set
            {
                paymentFunctionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Policy")]
        public PolicyType[] Policy
        {
            get
            {
                return policyField;
            }
            set
            {
                policyField = value;
            }
        }

        /// <remarks/>
        public ResponseParametersRequestType ResponseParameters
        {
            get
            {
                return responseParametersField;
            }
            set
            {
                responseParametersField = value;
            }
        }

        /// <remarks/>
        public ServiceListCoreRequestType ServiceListCoreRequest
        {
            get
            {
                return serviceListCoreRequestField;
            }
            set
            {
                serviceListCoreRequestField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedMarketingSegmentType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private TransportPointType arrivalField;

        private string carrierDesigCodeField;

        private string carrierNameField;

        private string datedMarketingSegmentIdField;

        private string datedOperatingSegmentRefIdField;

        private TransportPointType depField;

        private string marketingCarrierFlightNumberTextField;

        private string operationalSuffixTextField;

        private MediaResourceRefType[] richMediaField;

        private bool ticketlessIndField;

        private bool ticketlessIndFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public TransportPointType Arrival
        {
            get
            {
                return arrivalField;
            }
            set
            {
                arrivalField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierDesigCode
        {
            get
            {
                return carrierDesigCodeField;
            }
            set
            {
                carrierDesigCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierName
        {
            get
            {
                return carrierNameField;
            }
            set
            {
                carrierNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DatedMarketingSegmentId
        {
            get
            {
                return datedMarketingSegmentIdField;
            }
            set
            {
                datedMarketingSegmentIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DatedOperatingSegmentRefId
        {
            get
            {
                return datedOperatingSegmentRefIdField;
            }
            set
            {
                datedOperatingSegmentRefIdField = value;
            }
        }

        /// <remarks/>
        public TransportPointType Dep
        {
            get
            {
                return depField;
            }
            set
            {
                depField = value;
            }
        }

        /// <remarks/>
        public string MarketingCarrierFlightNumberText
        {
            get
            {
                return marketingCarrierFlightNumberTextField;
            }
            set
            {
                marketingCarrierFlightNumberTextField = value;
            }
        }

        /// <remarks/>
        public string OperationalSuffixText
        {
            get
            {
                return operationalSuffixTextField;
            }
            set
            {
                operationalSuffixTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }

        /// <remarks/>
        public bool TicketlessInd
        {
            get
            {
                return ticketlessIndField;
            }
            set
            {
                ticketlessIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TicketlessIndSpecified
        {
            get
            {
                return ticketlessIndFieldSpecified;
            }
            set
            {
                ticketlessIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AirlineTaxonomyType
    {

        private string descTextField;

        private string taxonomyCodeField;

        private TaxonomyFeatureType[] taxonomyFeatureField;

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TaxonomyCode
        {
            get
            {
                return taxonomyCodeField;
            }
            set
            {
                taxonomyCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("TaxonomyFeature")]
        public TaxonomyFeatureType[] TaxonomyFeature
        {
            get
            {
                return taxonomyFeatureField;
            }
            set
            {
                taxonomyFeatureField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TaxonomyFeatureType
    {

        private string codesetCodeField;

        private string codesetNameCodeField;

        private string valueTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CodesetCode
        {
            get
            {
                return codesetCodeField;
            }
            set
            {
                codesetCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CodesetNameCode
        {
            get
            {
                return codesetNameCodeField;
            }
            set
            {
                codesetNameCodeField = value;
            }
        }

        /// <remarks/>
        public string ValueText
        {
            get
            {
                return valueTextField;
            }
            set
            {
                valueTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TransportPointType
    {

        private System.DateTime aircraftScheduledDateTimeField;

        private bool aircraftScheduledDateTimeFieldSpecified;

        private string boardingGateIDField;

        private string iATA_LocationCodeField;

        private string stationNameField;

        private string terminalNameField;

        /// <remarks/>
        public System.DateTime AircraftScheduledDateTime
        {
            get
            {
                return aircraftScheduledDateTimeField;
            }
            set
            {
                aircraftScheduledDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AircraftScheduledDateTimeSpecified
        {
            get
            {
                return aircraftScheduledDateTimeFieldSpecified;
            }
            set
            {
                aircraftScheduledDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string BoardingGateID
        {
            get
            {
                return boardingGateIDField;
            }
            set
            {
                boardingGateIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IATA_LocationCode
        {
            get
            {
                return iATA_LocationCodeField;
            }
            set
            {
                iATA_LocationCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string StationName
        {
            get
            {
                return stationNameField;
            }
            set
            {
                stationNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TerminalName
        {
            get
            {
                return terminalNameField;
            }
            set
            {
                terminalNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MediaResourceRefType
    {

        private string contentTypeField;

        private string descTextField;

        private string resourceURIField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ContentType
        {
            get
            {
                return contentTypeField;
            }
            set
            {
                contentTypeField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string ResourceURI
        {
            get
            {
                return resourceURIField;
            }
            set
            {
                resourceURIField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedOperatingLegType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private TransportPointType arrivalField;

        private CarrierAircraftTypeType carrierAircraftTypeField;

        private bool changeofGaugeIndField;

        private bool changeofGaugeIndFieldSpecified;

        private string datedOperatingLegIDField;

        private TransportPointType depField;

        private MeasureType distanceMeasureField;

        private IATA_AircraftTypeType iATA_AircraftTypeField;

        private string onGroundDurationField;

        private MediaResourceRefType[] richMediaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public TransportPointType Arrival
        {
            get
            {
                return arrivalField;
            }
            set
            {
                arrivalField = value;
            }
        }

        /// <remarks/>
        public CarrierAircraftTypeType CarrierAircraftType
        {
            get
            {
                return carrierAircraftTypeField;
            }
            set
            {
                carrierAircraftTypeField = value;
            }
        }

        /// <remarks/>
        public bool ChangeofGaugeInd
        {
            get
            {
                return changeofGaugeIndField;
            }
            set
            {
                changeofGaugeIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ChangeofGaugeIndSpecified
        {
            get
            {
                return changeofGaugeIndFieldSpecified;
            }
            set
            {
                changeofGaugeIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DatedOperatingLegID
        {
            get
            {
                return datedOperatingLegIDField;
            }
            set
            {
                datedOperatingLegIDField = value;
            }
        }

        /// <remarks/>
        public TransportPointType Dep
        {
            get
            {
                return depField;
            }
            set
            {
                depField = value;
            }
        }

        /// <remarks/>
        public MeasureType DistanceMeasure
        {
            get
            {
                return distanceMeasureField;
            }
            set
            {
                distanceMeasureField = value;
            }
        }

        /// <remarks/>
        public IATA_AircraftTypeType IATA_AircraftType
        {
            get
            {
                return iATA_AircraftTypeField;
            }
            set
            {
                iATA_AircraftTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
        public string OnGroundDuration
        {
            get
            {
                return onGroundDurationField;
            }
            set
            {
                onGroundDurationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CarrierAircraftTypeType
    {

        private string carrierAircraftTypeCodeField;

        private string carrierAircraftTypeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierAircraftTypeCode
        {
            get
            {
                return carrierAircraftTypeCodeField;
            }
            set
            {
                carrierAircraftTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierAircraftTypeName
        {
            get
            {
                return carrierAircraftTypeNameField;
            }
            set
            {
                carrierAircraftTypeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MeasureType
    {

        private string unitCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string UnitCode
        {
            get
            {
                return unitCodeField;
            }
            set
            {
                unitCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public decimal Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IATA_AircraftTypeType
    {

        private string iATA_AircraftTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IATA_AircraftTypeCode
        {
            get
            {
                return iATA_AircraftTypeCodeField;
            }
            set
            {
                iATA_AircraftTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedOperatingSegmentType
    {

        private string carrierDesigCodeField;

        private string carrierNameField;

        private string[] datedOperatingLegRefIDField;

        private string datedOperatingSegmentIdField;

        private string disclosureRefIDField;

        private string durationField;

        private string operatingCarrierFlightNumberTextField;

        private string operationalSuffixTextField;

        private bool secureFlightIndField;

        private bool secureFlightIndFieldSpecified;

        private string segmentTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierDesigCode
        {
            get
            {
                return carrierDesigCodeField;
            }
            set
            {
                carrierDesigCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierName
        {
            get
            {
                return carrierNameField;
            }
            set
            {
                carrierNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return datedOperatingLegRefIDField;
            }
            set
            {
                datedOperatingLegRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DatedOperatingSegmentId
        {
            get
            {
                return datedOperatingSegmentIdField;
            }
            set
            {
                datedOperatingSegmentIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DisclosureRefID
        {
            get
            {
                return disclosureRefIDField;
            }
            set
            {
                disclosureRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
        public string Duration
        {
            get
            {
                return durationField;
            }
            set
            {
                durationField = value;
            }
        }

        /// <remarks/>
        public string OperatingCarrierFlightNumberText
        {
            get
            {
                return operatingCarrierFlightNumberTextField;
            }
            set
            {
                operatingCarrierFlightNumberTextField = value;
            }
        }

        /// <remarks/>
        public string OperationalSuffixText
        {
            get
            {
                return operationalSuffixTextField;
            }
            set
            {
                operationalSuffixTextField = value;
            }
        }

        /// <remarks/>
        public bool SecureFlightInd
        {
            get
            {
                return secureFlightIndField;
            }
            set
            {
                secureFlightIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool SecureFlightIndSpecified
        {
            get
            {
                return secureFlightIndFieldSpecified;
            }
            set
            {
                secureFlightIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SegmentTypeCode
        {
            get
            {
                return segmentTypeCodeField;
            }
            set
            {
                segmentTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareWaiverType
    {

        private string fareRuleWaiverCodeField;

        private string fareWaiverTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string FareRuleWaiverCode
        {
            get
            {
                return fareRuleWaiverCodeField;
            }
            set
            {
                fareRuleWaiverCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string FareWaiverTypeCode
        {
            get
            {
                return fareWaiverTypeCodeField;
            }
            set
            {
                fareWaiverTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MetadataType
    {

        private CodesetMetadataType[] codesetMetadataField;

        private FieldMetadataType[] fieldMetadataField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CodesetMetadata")]
        public CodesetMetadataType[] CodesetMetadata
        {
            get
            {
                return codesetMetadataField;
            }
            set
            {
                codesetMetadataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FieldMetadata")]
        public FieldMetadataType[] FieldMetadata
        {
            get
            {
                return fieldMetadataField;
            }
            set
            {
                fieldMetadataField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CodesetMetadataType
    {

        private string codesetNameField;

        private decimal codesetVersionNumberField;

        private bool codesetVersionNumberFieldSpecified;

        private LangUsageType langUsageField;

        private string ownerIDField;

        private string ownerNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CodesetName
        {
            get
            {
                return codesetNameField;
            }
            set
            {
                codesetNameField = value;
            }
        }

        /// <remarks/>
        public decimal CodesetVersionNumber
        {
            get
            {
                return codesetVersionNumberField;
            }
            set
            {
                codesetVersionNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool CodesetVersionNumberSpecified
        {
            get
            {
                return codesetVersionNumberFieldSpecified;
            }
            set
            {
                codesetVersionNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public LangUsageType LangUsage
        {
            get
            {
                return langUsageField;
            }
            set
            {
                langUsageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerID
        {
            get
            {
                return ownerIDField;
            }
            set
            {
                ownerIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return ownerNameField;
            }
            set
            {
                ownerNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LangUsageType
    {

        private string langCodeField;

        private string langUsageTextField;

        /// <remarks/>
        public string LangCode
        {
            get
            {
                return langCodeField;
            }
            set
            {
                langCodeField = value;
            }
        }

        /// <remarks/>
        public string LangUsageText
        {
            get
            {
                return langUsageTextField;
            }
            set
            {
                langUsageTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FieldMetadataType
    {

        private bool mandatoryIndField;

        private bool mandatoryIndFieldSpecified;

        private string pathTextField;

        /// <remarks/>
        public bool MandatoryInd
        {
            get
            {
                return mandatoryIndField;
            }
            set
            {
                mandatoryIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MandatoryIndSpecified
        {
            get
            {
                return mandatoryIndFieldSpecified;
            }
            set
            {
                mandatoryIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string PathText
        {
            get
            {
                return pathTextField;
            }
            set
            {
                pathTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferCriteriaType
    {

        private BaggagePricingCriteriaType baggagePricingCriteriaField;

        private FareCriteriaType[] fareCriteriaField;

        private PricingMethodCriteriaType[] pricingMethodCriteriaField;

        private ProgramCriteriaType[] programCriteriaField;

        private PromotionType[] promotionCriteriaField;

        private SeatCriteriaType[] seatCriteriaField;

        private ServiceCriteriaType[] serviceCriteriaField;

        private SpecialServiceType[] specialNeedsCriteriaField;

        private TripPurposeCodeType tripPurposeCodeField;

        private bool tripPurposeCodeFieldSpecified;

        private PrefLevelType waitListCriteriaField;

        /// <remarks/>
        public BaggagePricingCriteriaType BaggagePricingCriteria
        {
            get
            {
                return baggagePricingCriteriaField;
            }
            set
            {
                baggagePricingCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FareCriteria")]
        public FareCriteriaType[] FareCriteria
        {
            get
            {
                return fareCriteriaField;
            }
            set
            {
                fareCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PricingMethodCriteria")]
        public PricingMethodCriteriaType[] PricingMethodCriteria
        {
            get
            {
                return pricingMethodCriteriaField;
            }
            set
            {
                pricingMethodCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ProgramCriteria")]
        public ProgramCriteriaType[] ProgramCriteria
        {
            get
            {
                return programCriteriaField;
            }
            set
            {
                programCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PromotionCriteria")]
        public PromotionType[] PromotionCriteria
        {
            get
            {
                return promotionCriteriaField;
            }
            set
            {
                promotionCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SeatCriteria")]
        public SeatCriteriaType[] SeatCriteria
        {
            get
            {
                return seatCriteriaField;
            }
            set
            {
                seatCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ServiceCriteria")]
        public ServiceCriteriaType[] ServiceCriteria
        {
            get
            {
                return serviceCriteriaField;
            }
            set
            {
                serviceCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SpecialNeedsCriteria")]
        public SpecialServiceType[] SpecialNeedsCriteria
        {
            get
            {
                return specialNeedsCriteriaField;
            }
            set
            {
                specialNeedsCriteriaField = value;
            }
        }

        /// <remarks/>
        public TripPurposeCodeType TripPurposeCode
        {
            get
            {
                return tripPurposeCodeField;
            }
            set
            {
                tripPurposeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TripPurposeCodeSpecified
        {
            get
            {
                return tripPurposeCodeFieldSpecified;
            }
            set
            {
                tripPurposeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PrefLevelType WaitListCriteria
        {
            get
            {
                return waitListCriteriaField;
            }
            set
            {
                waitListCriteriaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BaggagePricingCriteriaType
    {

        private string baggageOptionCodeField;

        private string commercialAgreementIDField;

        private bool deferralIndField;

        private bool deferralIndFieldSpecified;

        private bool fixedPrePaidIndField;

        private bool fixedPrePaidIndFieldSpecified;

        private bool includeSettlementIndField;

        private bool includeSettlementIndFieldSpecified;

        private string optionalChargesCodeField;

        private string requestedActionCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string BaggageOptionCode
        {
            get
            {
                return baggageOptionCodeField;
            }
            set
            {
                baggageOptionCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CommercialAgreementID
        {
            get
            {
                return commercialAgreementIDField;
            }
            set
            {
                commercialAgreementIDField = value;
            }
        }

        /// <remarks/>
        public bool DeferralInd
        {
            get
            {
                return deferralIndField;
            }
            set
            {
                deferralIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DeferralIndSpecified
        {
            get
            {
                return deferralIndFieldSpecified;
            }
            set
            {
                deferralIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool FixedPrePaidInd
        {
            get
            {
                return fixedPrePaidIndField;
            }
            set
            {
                fixedPrePaidIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool FixedPrePaidIndSpecified
        {
            get
            {
                return fixedPrePaidIndFieldSpecified;
            }
            set
            {
                fixedPrePaidIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool IncludeSettlementInd
        {
            get
            {
                return includeSettlementIndField;
            }
            set
            {
                includeSettlementIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IncludeSettlementIndSpecified
        {
            get
            {
                return includeSettlementIndFieldSpecified;
            }
            set
            {
                includeSettlementIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OptionalChargesCode
        {
            get
            {
                return optionalChargesCodeField;
            }
            set
            {
                optionalChargesCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string RequestedActionCode
        {
            get
            {
                return requestedActionCodeField;
            }
            set
            {
                requestedActionCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareCriteriaType
    {

        private string[] fareBasisCodeField;

        private string[] fareSubTypeCodeField;

        private string[] fareTypeCodeField;

        private GroupFareCriteriaType[] groupFareCriteriaField;

        private bool noAdvancePurchaseIndField;

        private bool noAdvancePurchaseIndFieldSpecified;

        private bool noPenaltyIndField;

        private bool noPenaltyIndFieldSpecified;

        private PrefLevelType prefLevelField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FareBasisCode", DataType = "token")]
        public string[] FareBasisCode
        {
            get
            {
                return fareBasisCodeField;
            }
            set
            {
                fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FareSubTypeCode", DataType = "token")]
        public string[] FareSubTypeCode
        {
            get
            {
                return fareSubTypeCodeField;
            }
            set
            {
                fareSubTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FareTypeCode", DataType = "token")]
        public string[] FareTypeCode
        {
            get
            {
                return fareTypeCodeField;
            }
            set
            {
                fareTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("GroupFareCriteria")]
        public GroupFareCriteriaType[] GroupFareCriteria
        {
            get
            {
                return groupFareCriteriaField;
            }
            set
            {
                groupFareCriteriaField = value;
            }
        }

        /// <remarks/>
        public bool NoAdvancePurchaseInd
        {
            get
            {
                return noAdvancePurchaseIndField;
            }
            set
            {
                noAdvancePurchaseIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool NoAdvancePurchaseIndSpecified
        {
            get
            {
                return noAdvancePurchaseIndFieldSpecified;
            }
            set
            {
                noAdvancePurchaseIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool NoPenaltyInd
        {
            get
            {
                return noPenaltyIndField;
            }
            set
            {
                noPenaltyIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool NoPenaltyIndSpecified
        {
            get
            {
                return noPenaltyIndFieldSpecified;
            }
            set
            {
                noPenaltyIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return prefLevelField;
            }
            set
            {
                prefLevelField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class GroupFareCriteriaType
    {

        private string[] fareBasisCodeField;

        private string[] paxSegmentRefIDField;

        private PrefLevelType prefLevelField;

        private string pTCField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FareBasisCode", DataType = "token")]
        public string[] FareBasisCode
        {
            get
            {
                return fareBasisCodeField;
            }
            set
            {
                fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return paxSegmentRefIDField;
            }
            set
            {
                paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return prefLevelField;
            }
            set
            {
                prefLevelField = value;
            }
        }

        /// <remarks/>
        public string PTC
        {
            get
            {
                return pTCField;
            }
            set
            {
                pTCField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PrefLevelType
    {

        private PrefLevelCodeType prefLevelCodeField;

        /// <remarks/>
        public PrefLevelCodeType PrefLevelCode
        {
            get
            {
                return prefLevelCodeField;
            }
            set
            {
                prefLevelCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum PrefLevelCodeType
    {

        /// <remarks/>
        Exclude,

        /// <remarks/>
        Preferred,

        /// <remarks/>
        Required,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PricingMethodCriteriaType
    {

        private string bestPricingOptionTextField;

        /// <remarks/>
        public string BestPricingOptionText
        {
            get
            {
                return bestPricingOptionTextField;
            }
            set
            {
                bestPricingOptionTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgramCriteriaType
    {

        private string companyIndexTextField;

        private string memberStatusTextField;

        private string nameField;

        private PrePaidCertificateType[] prePaidCertificateField;

        private ProgamContractType progamContractField;

        private ProgramAccountType[] programAccountField;

        private ProgramOwnerType programOwnerField;

        private string typeCodeField;

        /// <remarks/>
        public string CompanyIndexText
        {
            get
            {
                return companyIndexTextField;
            }
            set
            {
                companyIndexTextField = value;
            }
        }

        /// <remarks/>
        public string MemberStatusText
        {
            get
            {
                return memberStatusTextField;
            }
            set
            {
                memberStatusTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PrePaidCertificate")]
        public PrePaidCertificateType[] PrePaidCertificate
        {
            get
            {
                return prePaidCertificateField;
            }
            set
            {
                prePaidCertificateField = value;
            }
        }

        /// <remarks/>
        public ProgamContractType ProgamContract
        {
            get
            {
                return progamContractField;
            }
            set
            {
                progamContractField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ProgramAccount")]
        public ProgramAccountType[] ProgramAccount
        {
            get
            {
                return programAccountField;
            }
            set
            {
                programAccountField = value;
            }
        }

        /// <remarks/>
        public ProgramOwnerType ProgramOwner
        {
            get
            {
                return programOwnerField;
            }
            set
            {
                programOwnerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PrePaidCertificateType
    {

        private string appTextField;

        private System.DateTime effectiveDateTimeField;

        private bool effectiveDateTimeFieldSpecified;

        private System.DateTime expiryDateTimeField;

        private bool expiryDateTimeFieldSpecified;

        private decimal numberField;

        private bool numberFieldSpecified;

        /// <remarks/>
        public string AppText
        {
            get
            {
                return appTextField;
            }
            set
            {
                appTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime EffectiveDateTime
        {
            get
            {
                return effectiveDateTimeField;
            }
            set
            {
                effectiveDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool EffectiveDateTimeSpecified
        {
            get
            {
                return effectiveDateTimeFieldSpecified;
            }
            set
            {
                effectiveDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime ExpiryDateTime
        {
            get
            {
                return expiryDateTimeField;
            }
            set
            {
                expiryDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ExpiryDateTimeSpecified
        {
            get
            {
                return expiryDateTimeFieldSpecified;
            }
            set
            {
                expiryDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal Number
        {
            get
            {
                return numberField;
            }
            set
            {
                numberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool NumberSpecified
        {
            get
            {
                return numberFieldSpecified;
            }
            set
            {
                numberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgamContractType
    {

        private string contractIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ContractID
        {
            get
            {
                return contractIDField;
            }
            set
            {
                contractIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgramAccountType
    {

        private string accountIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AccountID
        {
            get
            {
                return accountIDField;
            }
            set
            {
                accountIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgramOwnerType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Carrier", typeof(CarrierType))]
        [System.Xml.Serialization.XmlElement("Org", typeof(OrgType))]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CarrierType
    {

        private string airlineDesigCodeField;

        private bool duplicateDesigIndField;

        private bool duplicateDesigIndFieldSpecified;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AirlineDesigCode
        {
            get
            {
                return airlineDesigCodeField;
            }
            set
            {
                airlineDesigCodeField = value;
            }
        }

        /// <remarks/>
        public bool DuplicateDesigInd
        {
            get
            {
                return duplicateDesigIndField;
            }
            set
            {
                duplicateDesigIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DuplicateDesigIndSpecified
        {
            get
            {
                return duplicateDesigIndFieldSpecified;
            }
            set
            {
                duplicateDesigIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OrgType
    {

        private ContactInfoType[] contactInfoField;

        private string nameField;

        private string orgIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ContactInfo")]
        public ContactInfoType[] ContactInfo
        {
            get
            {
                return contactInfoField;
            }
            set
            {
                contactInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OrgID
        {
            get
            {
                return orgIDField;
            }
            set
            {
                orgIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PromotionType
    {

        private string ownerNameField;

        private string[] paxRefIDField;

        private string promotionIDField;

        private PromotionIssuerType promotionIssuerField;

        private RemarkType[] remarkField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return ownerNameField;
            }
            set
            {
                ownerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PromotionID
        {
            get
            {
                return promotionIDField;
            }
            set
            {
                promotionIDField = value;
            }
        }

        /// <remarks/>
        public PromotionIssuerType PromotionIssuer
        {
            get
            {
                return promotionIssuerField;
            }
            set
            {
                promotionIssuerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return remarkField;
            }
            set
            {
                remarkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return uRLField;
            }
            set
            {
                uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PromotionIssuerType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Carrier", typeof(CarrierType))]
        [System.Xml.Serialization.XmlElement("Org", typeof(OrgType))]
        [System.Xml.Serialization.XmlElement("TravelAgency", typeof(TravelAgencyType))]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TravelAgencyType
    {

        private string agencyIDField;

        private ContactInfoType[] contactInfoField;

        private decimal iATA_NumberField;

        private bool iATA_NumberFieldSpecified;

        private string nameField;

        private string pseudoCityIDField;

        private TravelAgentType travelAgentField;

        private TravelAgencyTypeCodeType typeCodeField;

        private bool typeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AgencyID
        {
            get
            {
                return agencyIDField;
            }
            set
            {
                agencyIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ContactInfo")]
        public ContactInfoType[] ContactInfo
        {
            get
            {
                return contactInfoField;
            }
            set
            {
                contactInfoField = value;
            }
        }

        /// <remarks/>
        public decimal IATA_Number
        {
            get
            {
                return iATA_NumberField;
            }
            set
            {
                iATA_NumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IATA_NumberSpecified
        {
            get
            {
                return iATA_NumberFieldSpecified;
            }
            set
            {
                iATA_NumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PseudoCityID
        {
            get
            {
                return pseudoCityIDField;
            }
            set
            {
                pseudoCityIDField = value;
            }
        }

        /// <remarks/>
        public TravelAgentType TravelAgent
        {
            get
            {
                return travelAgentField;
            }
            set
            {
                travelAgentField = value;
            }
        }

        /// <remarks/>
        public TravelAgencyTypeCodeType TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TypeCodeSpecified
        {
            get
            {
                return typeCodeFieldSpecified;
            }
            set
            {
                typeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TravelAgentType
    {

        private string travelAgentIDField;

        private TravelAgencyTypeCodeType typeCodeField;

        private bool typeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TravelAgentID
        {
            get
            {
                return travelAgentIDField;
            }
            set
            {
                travelAgentIDField = value;
            }
        }

        /// <remarks/>
        public TravelAgencyTypeCodeType TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TypeCodeSpecified
        {
            get
            {
                return typeCodeFieldSpecified;
            }
            set
            {
                typeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TravelAgencyTypeCodeType
    {

        /// <remarks/>
        OnlineTravelAgency,

        /// <remarks/>
        TravelAgency,

        /// <remarks/>
        TravelManagementCompany,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RemarkType
    {

        private bool displayIndField;

        private bool displayIndFieldSpecified;

        private string remarkTextField;

        private System.DateTime timestampField;

        private bool timestampFieldSpecified;

        /// <remarks/>
        public bool DisplayInd
        {
            get
            {
                return displayIndField;
            }
            set
            {
                displayIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DisplayIndSpecified
        {
            get
            {
                return displayIndFieldSpecified;
            }
            set
            {
                displayIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string RemarkText
        {
            get
            {
                return remarkTextField;
            }
            set
            {
                remarkTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime Timestamp
        {
            get
            {
                return timestampField;
            }
            set
            {
                timestampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TimestampSpecified
        {
            get
            {
                return timestampFieldSpecified;
            }
            set
            {
                timestampFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SeatCriteriaType
    {

        private string columnIDField;

        private string rowNumberField;

        private string[] seatCharacteristicCodeField;

        private string[] seatProfileCriteriaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ColumnID
        {
            get
            {
                return columnIDField;
            }
            set
            {
                columnIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "integer")]
        public string RowNumber
        {
            get
            {
                return rowNumberField;
            }
            set
            {
                rowNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SeatCharacteristicCode", DataType = "token")]
        public string[] SeatCharacteristicCode
        {
            get
            {
                return seatCharacteristicCodeField;
            }
            set
            {
                seatCharacteristicCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("SeatCharacteristicCode", typeof(string), DataType = "token", IsNullable = false)]
        public string[] SeatProfileCriteria
        {
            get
            {
                return seatProfileCriteriaField;
            }
            set
            {
                seatProfileCriteriaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceCriteriaType
    {

        private bool includeIndField;

        private bool includeIndFieldSpecified;

        private PrefLevelType prefLevelField;

        private string rFICField;

        private string[] rFISCField;

        private bool servicePricingOnlyIndField;

        private bool servicePricingOnlyIndFieldSpecified;

        private string[] taxonomyCodeField;

        /// <remarks/>
        public bool IncludeInd
        {
            get
            {
                return includeIndField;
            }
            set
            {
                includeIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IncludeIndSpecified
        {
            get
            {
                return includeIndFieldSpecified;
            }
            set
            {
                includeIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return prefLevelField;
            }
            set
            {
                prefLevelField = value;
            }
        }

        /// <remarks/>
        public string RFIC
        {
            get
            {
                return rFICField;
            }
            set
            {
                rFICField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RFISC")]
        public string[] RFISC
        {
            get
            {
                return rFISCField;
            }
            set
            {
                rFISCField = value;
            }
        }

        /// <remarks/>
        public bool ServicePricingOnlyInd
        {
            get
            {
                return servicePricingOnlyIndField;
            }
            set
            {
                servicePricingOnlyIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ServicePricingOnlyIndSpecified
        {
            get
            {
                return servicePricingOnlyIndFieldSpecified;
            }
            set
            {
                servicePricingOnlyIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("TaxonomyCode", DataType = "token")]
        public string[] TaxonomyCode
        {
            get
            {
                return taxonomyCodeField;
            }
            set
            {
                taxonomyCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SpecialServiceType
    {

        private MeasureType addlDataMeasureField;

        private string freeTextField;

        private decimal qtyField;

        private bool qtyFieldSpecified;

        private string specialServiceCodeField;

        /// <remarks/>
        public MeasureType AddlDataMeasure
        {
            get
            {
                return addlDataMeasureField;
            }
            set
            {
                addlDataMeasureField = value;
            }
        }

        /// <remarks/>
        public string FreeText
        {
            get
            {
                return freeTextField;
            }
            set
            {
                freeTextField = value;
            }
        }

        /// <remarks/>
        public decimal Qty
        {
            get
            {
                return qtyField;
            }
            set
            {
                qtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool QtySpecified
        {
            get
            {
                return qtyFieldSpecified;
            }
            set
            {
                qtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SpecialServiceCode
        {
            get
            {
                return specialServiceCodeField;
            }
            set
            {
                specialServiceCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TripPurposeCodeType
    {

        /// <remarks/>
        Business,

        /// <remarks/>
        BusinessAndLeisure,

        /// <remarks/>
        Leisure,

        /// <remarks/>
        Other,

        /// <remarks/>
        Unknown,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxType
    {

        private System.DateTime birthdateField;

        private bool birthdateFieldSpecified;

        private string citizenshipCountryCodeField;

        private string contactInfoRefIDField;

        private EmployerType employerField;

        private FOID_Type fOIDField;

        private IdentityDocType[] identityDocField;

        private IndividualType individualField;

        private LangUsageType[] langUsageField;

        private LoyaltyProgramAccountType[] loyaltyProgramAccountField;

        private string paxIDField;

        private string paxRefIDField;

        private bool profileConsentIndField;

        private bool profileConsentIndFieldSpecified;

        private string profileID_TextField;

        private string pTCField;

        private RedressCaseType[] redressCaseField;

        private RemarkType[] remarkField;

        private string residenceCountryCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime Birthdate
        {
            get
            {
                return birthdateField;
            }
            set
            {
                birthdateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool BirthdateSpecified
        {
            get
            {
                return birthdateFieldSpecified;
            }
            set
            {
                birthdateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CitizenshipCountryCode
        {
            get
            {
                return citizenshipCountryCodeField;
            }
            set
            {
                citizenshipCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ContactInfoRefID
        {
            get
            {
                return contactInfoRefIDField;
            }
            set
            {
                contactInfoRefIDField = value;
            }
        }

        /// <remarks/>
        public EmployerType Employer
        {
            get
            {
                return employerField;
            }
            set
            {
                employerField = value;
            }
        }

        /// <remarks/>
        public FOID_Type FOID
        {
            get
            {
                return fOIDField;
            }
            set
            {
                fOIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("IdentityDoc")]
        public IdentityDocType[] IdentityDoc
        {
            get
            {
                return identityDocField;
            }
            set
            {
                identityDocField = value;
            }
        }

        /// <remarks/>
        public IndividualType Individual
        {
            get
            {
                return individualField;
            }
            set
            {
                individualField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LangUsage")]
        public LangUsageType[] LangUsage
        {
            get
            {
                return langUsageField;
            }
            set
            {
                langUsageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LoyaltyProgramAccount")]
        public LoyaltyProgramAccountType[] LoyaltyProgramAccount
        {
            get
            {
                return loyaltyProgramAccountField;
            }
            set
            {
                loyaltyProgramAccountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxID
        {
            get
            {
                return paxIDField;
            }
            set
            {
                paxIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        public bool ProfileConsentInd
        {
            get
            {
                return profileConsentIndField;
            }
            set
            {
                profileConsentIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ProfileConsentIndSpecified
        {
            get
            {
                return profileConsentIndFieldSpecified;
            }
            set
            {
                profileConsentIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string ProfileID_Text
        {
            get
            {
                return profileID_TextField;
            }
            set
            {
                profileID_TextField = value;
            }
        }

        /// <remarks/>
        public string PTC
        {
            get
            {
                return pTCField;
            }
            set
            {
                pTCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RedressCase")]
        public RedressCaseType[] RedressCase
        {
            get
            {
                return redressCaseField;
            }
            set
            {
                redressCaseField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return remarkField;
            }
            set
            {
                remarkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ResidenceCountryCode
        {
            get
            {
                return residenceCountryCodeField;
            }
            set
            {
                residenceCountryCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class EmployerType
    {

        private string nameField;

        private SalesTaxRegistrationType[] salesTaxRegistrationField;

        private TaxExemptionQualificationCodeType taxationExemptionQualifierCodeField;

        private bool taxationExemptionQualifierCodeFieldSpecified;

        private bool taxDeductedatSourceCapabilityIndField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SalesTaxRegistration")]
        public SalesTaxRegistrationType[] SalesTaxRegistration
        {
            get
            {
                return salesTaxRegistrationField;
            }
            set
            {
                salesTaxRegistrationField = value;
            }
        }

        /// <remarks/>
        public TaxExemptionQualificationCodeType TaxationExemptionQualifierCode
        {
            get
            {
                return taxationExemptionQualifierCodeField;
            }
            set
            {
                taxationExemptionQualifierCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TaxationExemptionQualifierCodeSpecified
        {
            get
            {
                return taxationExemptionQualifierCodeFieldSpecified;
            }
            set
            {
                taxationExemptionQualifierCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool TaxDeductedatSourceCapabilityInd
        {
            get
            {
                return taxDeductedatSourceCapabilityIndField;
            }
            set
            {
                taxDeductedatSourceCapabilityIndField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SalesTaxRegistrationType
    {

        private string cityNameField;

        private string countryCodeField;

        private string countryDialingCodeField;

        private string countrySubDivisionNameField;

        private string emailAddressTextField;

        private string[] phoneNumberField;

        private string postalCodeField;

        private string salesTaxRegistrationIDField;

        private string[] streetTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CityName
        {
            get
            {
                return cityNameField;
            }
            set
            {
                cityNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return countryCodeField;
            }
            set
            {
                countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryDialingCode
        {
            get
            {
                return countryDialingCodeField;
            }
            set
            {
                countryDialingCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountrySubDivisionName
        {
            get
            {
                return countrySubDivisionNameField;
            }
            set
            {
                countrySubDivisionNameField = value;
            }
        }

        /// <remarks/>
        public string EmailAddressText
        {
            get
            {
                return emailAddressTextField;
            }
            set
            {
                emailAddressTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PhoneNumber")]
        public string[] PhoneNumber
        {
            get
            {
                return phoneNumberField;
            }
            set
            {
                phoneNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PostalCode
        {
            get
            {
                return postalCodeField;
            }
            set
            {
                postalCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SalesTaxRegistrationID
        {
            get
            {
                return salesTaxRegistrationIDField;
            }
            set
            {
                salesTaxRegistrationIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("StreetText")]
        public string[] StreetText
        {
            get
            {
                return streetTextField;
            }
            set
            {
                streetTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TaxExemptionQualificationCodeType
    {

        /// <remarks/>
        PSU,

        /// <remarks/>
        SEZ,

        /// <remarks/>
        UN,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FOID_Type
    {

        private string fOID_IDField;

        private string fOID_TypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string FOID_ID
        {
            get
            {
                return fOID_IDField;
            }
            set
            {
                fOID_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string FOID_TypeCode
        {
            get
            {
                return fOID_TypeCodeField;
            }
            set
            {
                fOID_TypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IdentityDocType
    {

        private AddlNameType[] addlNameField;

        private System.DateTime birthdateField;

        private bool birthdateFieldSpecified;

        private string birthplaceTextField;

        private string citizenshipCountryCodeField;

        private System.DateTime expiryDateField;

        private bool expiryDateFieldSpecified;

        private GenderCodeType genderCodeField;

        private bool genderCodeFieldSpecified;

        private string[] givenNameField;

        private string identityDocIDField;

        private string identityDocTypeCodeField;

        private System.DateTime issueDateField;

        private bool issueDateFieldSpecified;

        private string issuingCountryCodeField;

        private string[] middleNameField;

        private string residenceCountryCodeField;

        private string suffixNameField;

        private string surnameField;

        private string titleNameField;

        private VisaType[] visaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AddlName")]
        public AddlNameType[] AddlName
        {
            get
            {
                return addlNameField;
            }
            set
            {
                addlNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime Birthdate
        {
            get
            {
                return birthdateField;
            }
            set
            {
                birthdateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool BirthdateSpecified
        {
            get
            {
                return birthdateFieldSpecified;
            }
            set
            {
                birthdateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string BirthplaceText
        {
            get
            {
                return birthplaceTextField;
            }
            set
            {
                birthplaceTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CitizenshipCountryCode
        {
            get
            {
                return citizenshipCountryCodeField;
            }
            set
            {
                citizenshipCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime ExpiryDate
        {
            get
            {
                return expiryDateField;
            }
            set
            {
                expiryDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ExpiryDateSpecified
        {
            get
            {
                return expiryDateFieldSpecified;
            }
            set
            {
                expiryDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public GenderCodeType GenderCode
        {
            get
            {
                return genderCodeField;
            }
            set
            {
                genderCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool GenderCodeSpecified
        {
            get
            {
                return genderCodeFieldSpecified;
            }
            set
            {
                genderCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("GivenName", DataType = "token")]
        public string[] GivenName
        {
            get
            {
                return givenNameField;
            }
            set
            {
                givenNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IdentityDocID
        {
            get
            {
                return identityDocIDField;
            }
            set
            {
                identityDocIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IdentityDocTypeCode
        {
            get
            {
                return identityDocTypeCodeField;
            }
            set
            {
                identityDocTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime IssueDate
        {
            get
            {
                return issueDateField;
            }
            set
            {
                issueDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IssueDateSpecified
        {
            get
            {
                return issueDateFieldSpecified;
            }
            set
            {
                issueDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IssuingCountryCode
        {
            get
            {
                return issuingCountryCodeField;
            }
            set
            {
                issuingCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("MiddleName", DataType = "token")]
        public string[] MiddleName
        {
            get
            {
                return middleNameField;
            }
            set
            {
                middleNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ResidenceCountryCode
        {
            get
            {
                return residenceCountryCodeField;
            }
            set
            {
                residenceCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SuffixName
        {
            get
            {
                return suffixNameField;
            }
            set
            {
                suffixNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Surname
        {
            get
            {
                return surnameField;
            }
            set
            {
                surnameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TitleName
        {
            get
            {
                return titleNameField;
            }
            set
            {
                titleNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Visa")]
        public VisaType[] Visa
        {
            get
            {
                return visaField;
            }
            set
            {
                visaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class VisaType
    {

        private decimal entryQtyField;

        private bool entryQtyFieldSpecified;

        private System.DateTime expiryDateField;

        private bool expiryDateFieldSpecified;

        private string hostCountryCodeField;

        private System.DateTime issueDateField;

        private bool issueDateFieldSpecified;

        private string issuingCountryCodeField;

        private string stayDurationField;

        private string visaIDField;

        private string visaTypeCodeField;

        /// <remarks/>
        public decimal EntryQty
        {
            get
            {
                return entryQtyField;
            }
            set
            {
                entryQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool EntryQtySpecified
        {
            get
            {
                return entryQtyFieldSpecified;
            }
            set
            {
                entryQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime ExpiryDate
        {
            get
            {
                return expiryDateField;
            }
            set
            {
                expiryDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ExpiryDateSpecified
        {
            get
            {
                return expiryDateFieldSpecified;
            }
            set
            {
                expiryDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string HostCountryCode
        {
            get
            {
                return hostCountryCodeField;
            }
            set
            {
                hostCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime IssueDate
        {
            get
            {
                return issueDateField;
            }
            set
            {
                issueDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IssueDateSpecified
        {
            get
            {
                return issueDateFieldSpecified;
            }
            set
            {
                issueDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IssuingCountryCode
        {
            get
            {
                return issuingCountryCodeField;
            }
            set
            {
                issuingCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
        public string StayDuration
        {
            get
            {
                return stayDurationField;
            }
            set
            {
                stayDurationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string VisaID
        {
            get
            {
                return visaIDField;
            }
            set
            {
                visaIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string VisaTypeCode
        {
            get
            {
                return visaTypeCodeField;
            }
            set
            {
                visaTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LoyaltyProgramAccountType
    {

        private string accountNumberField;

        private LoyaltyProgramType loyaltyProgramField;

        private string signInIDField;

        private string tierCodeField;

        private string tierNameField;

        private string tierPriorityTextField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AccountNumber
        {
            get
            {
                return accountNumberField;
            }
            set
            {
                accountNumberField = value;
            }
        }

        /// <remarks/>
        public LoyaltyProgramType LoyaltyProgram
        {
            get
            {
                return loyaltyProgramField;
            }
            set
            {
                loyaltyProgramField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SignInID
        {
            get
            {
                return signInIDField;
            }
            set
            {
                signInIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TierCode
        {
            get
            {
                return tierCodeField;
            }
            set
            {
                tierCodeField = value;
            }
        }

        /// <remarks/>
        public string TierName
        {
            get
            {
                return tierNameField;
            }
            set
            {
                tierNameField = value;
            }
        }

        /// <remarks/>
        public string TierPriorityText
        {
            get
            {
                return tierPriorityTextField;
            }
            set
            {
                tierPriorityTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return uRLField;
            }
            set
            {
                uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LoyaltyProgramType
    {

        private AllianceType allianceField;

        private CarrierType carrierField;

        private string programCodeField;

        private string programNameField;

        private string providerNameField;

        private string uRLField;

        /// <remarks/>
        public AllianceType Alliance
        {
            get
            {
                return allianceField;
            }
            set
            {
                allianceField = value;
            }
        }

        /// <remarks/>
        public CarrierType Carrier
        {
            get
            {
                return carrierField;
            }
            set
            {
                carrierField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ProgramCode
        {
            get
            {
                return programCodeField;
            }
            set
            {
                programCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ProgramName
        {
            get
            {
                return programNameField;
            }
            set
            {
                programNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ProviderName
        {
            get
            {
                return providerNameField;
            }
            set
            {
                providerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return uRLField;
            }
            set
            {
                uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AllianceType
    {

        private string allianceCodeField;

        private CarrierType[] carrierField;

        private string nameField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AllianceCode
        {
            get
            {
                return allianceCodeField;
            }
            set
            {
                allianceCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Carrier")]
        public CarrierType[] Carrier
        {
            get
            {
                return carrierField;
            }
            set
            {
                carrierField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return uRLField;
            }
            set
            {
                uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RedressCaseType
    {

        private string countryCodeField;

        private string programNameField;

        private string redressCaseIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return countryCodeField;
            }
            set
            {
                countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ProgramName
        {
            get
            {
                return programNameField;
            }
            set
            {
                programNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string RedressCaseID
        {
            get
            {
                return redressCaseIDField;
            }
            set
            {
                redressCaseIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxSegmentType
    {

        private CabinTypeAssociationChoiceType cabinTypeAssociationChoiceField;

        private string datedMarketingSegmentRefIdField;

        private string marketingCarrierRBD_CodeField;

        private string operatingCarrierRBD_CodeField;

        private string paxSegmentIDField;

        private SettlementInfoType settlementInfoField;

        /// <remarks/>
        public CabinTypeAssociationChoiceType CabinTypeAssociationChoice
        {
            get
            {
                return cabinTypeAssociationChoiceField;
            }
            set
            {
                cabinTypeAssociationChoiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DatedMarketingSegmentRefId
        {
            get
            {
                return datedMarketingSegmentRefIdField;
            }
            set
            {
                datedMarketingSegmentRefIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string MarketingCarrierRBD_Code
        {
            get
            {
                return marketingCarrierRBD_CodeField;
            }
            set
            {
                marketingCarrierRBD_CodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OperatingCarrierRBD_Code
        {
            get
            {
                return operatingCarrierRBD_CodeField;
            }
            set
            {
                operatingCarrierRBD_CodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxSegmentID
        {
            get
            {
                return paxSegmentIDField;
            }
            set
            {
                paxSegmentIDField = value;
            }
        }

        /// <remarks/>
        public SettlementInfoType SettlementInfo
        {
            get
            {
                return settlementInfoField;
            }
            set
            {
                settlementInfoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CabinTypeAssociationChoiceType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LegCabinAssociations", typeof(LegCabinAssociationsType))]
        [System.Xml.Serialization.XmlElement("SegmentCabinType", typeof(CabinTypeType))]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LegCabinAssociationsType
    {

        private LegsAssociatedCabinType[] legsAssociatedCabinField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LegsAssociatedCabin")]
        public LegsAssociatedCabinType[] LegsAssociatedCabin
        {
            get
            {
                return legsAssociatedCabinField;
            }
            set
            {
                legsAssociatedCabinField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LegsAssociatedCabinType
    {

        private string[] datedOperatingLegRefIDField;

        private CabinTypeType legCabinTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return datedOperatingLegRefIDField;
            }
            set
            {
                datedOperatingLegRefIDField = value;
            }
        }

        /// <remarks/>
        public CabinTypeType LegCabinType
        {
            get
            {
                return legCabinTypeField;
            }
            set
            {
                legCabinTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CabinTypeType
    {

        private string cabinTypeCodeField;

        private string cabinTypeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CabinTypeCode
        {
            get
            {
                return cabinTypeCodeField;
            }
            set
            {
                cabinTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CabinTypeName
        {
            get
            {
                return cabinTypeNameField;
            }
            set
            {
                cabinTypeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SettlementInfoType
    {

        private CommissionType[] commissionField;

        private FeeType[] feeField;

        private string methodCodeField;

        private AmountType settlementAmountField;

        private SurchargeType[] surchargeField;

        private TaxType[] taxField;

        private bool taxableIndField;

        private bool taxableIndFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Commission")]
        public CommissionType[] Commission
        {
            get
            {
                return commissionField;
            }
            set
            {
                commissionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Fee")]
        public FeeType[] Fee
        {
            get
            {
                return feeField;
            }
            set
            {
                feeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string MethodCode
        {
            get
            {
                return methodCodeField;
            }
            set
            {
                methodCodeField = value;
            }
        }

        /// <remarks/>
        public AmountType SettlementAmount
        {
            get
            {
                return settlementAmountField;
            }
            set
            {
                settlementAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Surcharge")]
        public SurchargeType[] Surcharge
        {
            get
            {
                return surchargeField;
            }
            set
            {
                surchargeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Tax")]
        public TaxType[] Tax
        {
            get
            {
                return taxField;
            }
            set
            {
                taxField = value;
            }
        }

        /// <remarks/>
        public bool TaxableInd
        {
            get
            {
                return taxableIndField;
            }
            set
            {
                taxableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TaxableIndSpecified
        {
            get
            {
                return taxableIndFieldSpecified;
            }
            set
            {
                taxableIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CommissionType
    {

        private AmountType amountField;

        private string commissionCodeField;

        private decimal percentageField;

        private bool percentageFieldSpecified;

        private AmountType percentageAppliedToAmountField;

        private bool taxableIndField;

        private bool taxableIndFieldSpecified;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return amountField;
            }
            set
            {
                amountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CommissionCode
        {
            get
            {
                return commissionCodeField;
            }
            set
            {
                commissionCodeField = value;
            }
        }

        /// <remarks/>
        public decimal Percentage
        {
            get
            {
                return percentageField;
            }
            set
            {
                percentageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PercentageSpecified
        {
            get
            {
                return percentageFieldSpecified;
            }
            set
            {
                percentageFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType PercentageAppliedToAmount
        {
            get
            {
                return percentageAppliedToAmountField;
            }
            set
            {
                percentageAppliedToAmountField = value;
            }
        }

        /// <remarks/>
        public bool TaxableInd
        {
            get
            {
                return taxableIndField;
            }
            set
            {
                taxableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TaxableIndSpecified
        {
            get
            {
                return taxableIndFieldSpecified;
            }
            set
            {
                taxableIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AmountType
    {

        private string curCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string CurCode
        {
            get
            {
                return curCodeField;
            }
            set
            {
                curCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public decimal Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FeeType
    {

        private AmountType amountField;

        private bool approximateIndField;

        private bool approximateIndFieldSpecified;

        private string descTextField;

        private string desigTextField;

        private System.DateTime guaranteeTimeLimitDateTimeField;

        private bool guaranteeTimeLimitDateTimeFieldSpecified;

        private AmountType localAmountField;

        private AmountType maximumAmountField;

        private AmountType minimumAmountField;

        private decimal percentField;

        private bool percentFieldSpecified;

        private bool refundIndField;

        private bool refundIndFieldSpecified;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return amountField;
            }
            set
            {
                amountField = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return approximateIndField;
            }
            set
            {
                approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return approximateIndFieldSpecified;
            }
            set
            {
                approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public string DesigText
        {
            get
            {
                return desigTextField;
            }
            set
            {
                desigTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime GuaranteeTimeLimitDateTime
        {
            get
            {
                return guaranteeTimeLimitDateTimeField;
            }
            set
            {
                guaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool GuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return guaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                guaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType LocalAmount
        {
            get
            {
                return localAmountField;
            }
            set
            {
                localAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MaximumAmount
        {
            get
            {
                return maximumAmountField;
            }
            set
            {
                maximumAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MinimumAmount
        {
            get
            {
                return minimumAmountField;
            }
            set
            {
                minimumAmountField = value;
            }
        }

        /// <remarks/>
        public decimal Percent
        {
            get
            {
                return percentField;
            }
            set
            {
                percentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PercentSpecified
        {
            get
            {
                return percentFieldSpecified;
            }
            set
            {
                percentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool RefundInd
        {
            get
            {
                return refundIndField;
            }
            set
            {
                refundIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RefundIndSpecified
        {
            get
            {
                return refundIndFieldSpecified;
            }
            set
            {
                refundIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SurchargeType
    {

        private bool allRefundableIndField;

        private bool allRefundableIndFieldSpecified;

        private FeeType[] breakdownField;

        private AmountType totalAmountField;

        /// <remarks/>
        public bool AllRefundableInd
        {
            get
            {
                return allRefundableIndField;
            }
            set
            {
                allRefundableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AllRefundableIndSpecified
        {
            get
            {
                return allRefundableIndFieldSpecified;
            }
            set
            {
                allRefundableIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Breakdown")]
        public FeeType[] Breakdown
        {
            get
            {
                return breakdownField;
            }
            set
            {
                breakdownField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalAmount
        {
            get
            {
                return totalAmountField;
            }
            set
            {
                totalAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TaxType
    {

        private string addlFiledTaxCodeField;

        private string addlTaxCodeField;

        private AmountType amountField;

        private bool approximateIndField;

        private bool approximateIndFieldSpecified;

        private string appTypeCodeField;

        private bool collectionIndField;

        private bool collectionIndFieldSpecified;

        private CollectionPointTaxType[] collectionPointTaxField;

        private CountryType countryField;

        private CountrySubDivisionType countrySubDivisionField;

        private CurConversionType curConversionField;

        private string descTextField;

        private AmountType filedAmountField;

        private string filedTaxCodeField;

        private string qualifierCodeField;

        private bool refundIndField;

        private bool refundIndFieldSpecified;

        private string taxCodeField;

        private string taxNameField;

        private TaxTypeCodeType taxTypeCodeField;

        private bool taxTypeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AddlFiledTaxCode
        {
            get
            {
                return addlFiledTaxCodeField;
            }
            set
            {
                addlFiledTaxCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AddlTaxCode
        {
            get
            {
                return addlTaxCodeField;
            }
            set
            {
                addlTaxCodeField = value;
            }
        }

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return amountField;
            }
            set
            {
                amountField = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return approximateIndField;
            }
            set
            {
                approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return approximateIndFieldSpecified;
            }
            set
            {
                approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AppTypeCode
        {
            get
            {
                return appTypeCodeField;
            }
            set
            {
                appTypeCodeField = value;
            }
        }

        /// <remarks/>
        public bool CollectionInd
        {
            get
            {
                return collectionIndField;
            }
            set
            {
                collectionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool CollectionIndSpecified
        {
            get
            {
                return collectionIndFieldSpecified;
            }
            set
            {
                collectionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CollectionPointTax")]
        public CollectionPointTaxType[] CollectionPointTax
        {
            get
            {
                return collectionPointTaxField;
            }
            set
            {
                collectionPointTaxField = value;
            }
        }

        /// <remarks/>
        public CountryType Country
        {
            get
            {
                return countryField;
            }
            set
            {
                countryField = value;
            }
        }

        /// <remarks/>
        public CountrySubDivisionType CountrySubDivision
        {
            get
            {
                return countrySubDivisionField;
            }
            set
            {
                countrySubDivisionField = value;
            }
        }

        /// <remarks/>
        public CurConversionType CurConversion
        {
            get
            {
                return curConversionField;
            }
            set
            {
                curConversionField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType FiledAmount
        {
            get
            {
                return filedAmountField;
            }
            set
            {
                filedAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string FiledTaxCode
        {
            get
            {
                return filedTaxCodeField;
            }
            set
            {
                filedTaxCodeField = value;
            }
        }

        /// <remarks/>
        public string QualifierCode
        {
            get
            {
                return qualifierCodeField;
            }
            set
            {
                qualifierCodeField = value;
            }
        }

        /// <remarks/>
        public bool RefundInd
        {
            get
            {
                return refundIndField;
            }
            set
            {
                refundIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RefundIndSpecified
        {
            get
            {
                return refundIndFieldSpecified;
            }
            set
            {
                refundIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TaxCode
        {
            get
            {
                return taxCodeField;
            }
            set
            {
                taxCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TaxName
        {
            get
            {
                return taxNameField;
            }
            set
            {
                taxNameField = value;
            }
        }

        /// <remarks/>
        public TaxTypeCodeType TaxTypeCode
        {
            get
            {
                return taxTypeCodeField;
            }
            set
            {
                taxTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TaxTypeCodeSpecified
        {
            get
            {
                return taxTypeCodeFieldSpecified;
            }
            set
            {
                taxTypeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CollectionPointTaxType
    {

        private AmountType airportAmountField;

        private StationType stationField;

        /// <remarks/>
        public AmountType AirportAmount
        {
            get
            {
                return airportAmountField;
            }
            set
            {
                airportAmountField = value;
            }
        }

        /// <remarks/>
        public StationType Station
        {
            get
            {
                return stationField;
            }
            set
            {
                stationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class StationType
    {

        private string iATA_LocationCodeField;

        private string stationNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IATA_LocationCode
        {
            get
            {
                return iATA_LocationCodeField;
            }
            set
            {
                iATA_LocationCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string StationName
        {
            get
            {
                return stationNameField;
            }
            set
            {
                stationNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CountrySubDivisionType
    {

        private string countrySubDivisionCodeField;

        private string countrySubDivisionNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountrySubDivisionCode
        {
            get
            {
                return countrySubDivisionCodeField;
            }
            set
            {
                countrySubDivisionCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CountrySubDivisionName
        {
            get
            {
                return countrySubDivisionNameField;
            }
            set
            {
                countrySubDivisionNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CurConversionType
    {

        private AmountType amountField;

        private RateType conversionRateField;

        private AmountType localAmountField;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return amountField;
            }
            set
            {
                amountField = value;
            }
        }

        /// <remarks/>
        public RateType ConversionRate
        {
            get
            {
                return conversionRateField;
            }
            set
            {
                conversionRateField = value;
            }
        }

        /// <remarks/>
        public AmountType LocalAmount
        {
            get
            {
                return localAmountField;
            }
            set
            {
                localAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RateType
    {

        private string baseCurCodeField;

        private decimal baseMultiplierValueField;

        private bool baseMultiplierValueFieldSpecified;

        private string baseUnitCodeField;

        private string curCodeField;

        private string formatField;

        private decimal multiplierValueField;

        private bool multiplierValueFieldSpecified;

        private string unitCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string BaseCurCode
        {
            get
            {
                return baseCurCodeField;
            }
            set
            {
                baseCurCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public decimal BaseMultiplierValue
        {
            get
            {
                return baseMultiplierValueField;
            }
            set
            {
                baseMultiplierValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool BaseMultiplierValueSpecified
        {
            get
            {
                return baseMultiplierValueFieldSpecified;
            }
            set
            {
                baseMultiplierValueFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string BaseUnitCode
        {
            get
            {
                return baseUnitCodeField;
            }
            set
            {
                baseUnitCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string CurCode
        {
            get
            {
                return curCodeField;
            }
            set
            {
                curCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Format
        {
            get
            {
                return formatField;
            }
            set
            {
                formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public decimal MultiplierValue
        {
            get
            {
                return multiplierValueField;
            }
            set
            {
                multiplierValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MultiplierValueSpecified
        {
            get
            {
                return multiplierValueFieldSpecified;
            }
            set
            {
                multiplierValueFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string UnitCode
        {
            get
            {
                return unitCodeField;
            }
            set
            {
                unitCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public decimal Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TaxTypeCodeType
    {

        /// <remarks/>
        Applied,

        /// <remarks/>
        Exempt,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentFunctionsCriteriaType
    {

        private PaymentMethodCriteriaType paymentMethodCriteriaField;

        /// <remarks/>
        public PaymentMethodCriteriaType PaymentMethodCriteria
        {
            get
            {
                return paymentMethodCriteriaField;
            }
            set
            {
                paymentMethodCriteriaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentMethodCriteriaType
    {

        private ClearanceCriteriaType[] clearanceCriteriaField;

        private string paymentBrandCodeField;

        private PaymentCriteriaAddlInfoType paymentCriteriaAddlInfoField;

        private string paymentMethodCountryCodeField;

        private string paymentTypeCodeField;

        private PrefLevelType prefLevelField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ClearanceCriteria")]
        public ClearanceCriteriaType[] ClearanceCriteria
        {
            get
            {
                return clearanceCriteriaField;
            }
            set
            {
                clearanceCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaymentBrandCode
        {
            get
            {
                return paymentBrandCodeField;
            }
            set
            {
                paymentBrandCodeField = value;
            }
        }

        /// <remarks/>
        public PaymentCriteriaAddlInfoType PaymentCriteriaAddlInfo
        {
            get
            {
                return paymentCriteriaAddlInfoField;
            }
            set
            {
                paymentCriteriaAddlInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaymentMethodCountryCode
        {
            get
            {
                return paymentMethodCountryCodeField;
            }
            set
            {
                paymentMethodCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaymentTypeCode
        {
            get
            {
                return paymentTypeCodeField;
            }
            set
            {
                paymentTypeCodeField = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return prefLevelField;
            }
            set
            {
                prefLevelField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ClearanceCriteriaType
    {

        private string processRuleCodeField;

        private System.DateTime remittanceDateField;

        private bool remittanceDateFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ProcessRuleCode
        {
            get
            {
                return processRuleCodeField;
            }
            set
            {
                processRuleCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime RemittanceDate
        {
            get
            {
                return remittanceDateField;
            }
            set
            {
                remittanceDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RemittanceDateSpecified
        {
            get
            {
                return remittanceDateFieldSpecified;
            }
            set
            {
                remittanceDateFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentCriteriaAddlInfoType
    {

        private PaymentCardCriteriaAddlInfoType paymentCardCriteriaAddlInfoField;

        private RemarkType[] paymentOtherMethodAddlInfoField;

        /// <remarks/>
        public PaymentCardCriteriaAddlInfoType PaymentCardCriteriaAddlInfo
        {
            get
            {
                return paymentCardCriteriaAddlInfoField;
            }
            set
            {
                paymentCardCriteriaAddlInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Remark", IsNullable = false)]
        public RemarkType[] PaymentOtherMethodAddlInfo
        {
            get
            {
                return paymentOtherMethodAddlInfoField;
            }
            set
            {
                paymentOtherMethodAddlInfoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentCardCriteriaAddlInfoType
    {

        private string cardProductTypeCodeField;

        private string cardTypeTextField;

        private SecurePaymentAuthenticationVersionType securePaymentAuthenticationVersionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CardProductTypeCode
        {
            get
            {
                return cardProductTypeCodeField;
            }
            set
            {
                cardProductTypeCodeField = value;
            }
        }

        /// <remarks/>
        public string CardTypeText
        {
            get
            {
                return cardTypeTextField;
            }
            set
            {
                cardTypeTextField = value;
            }
        }

        /// <remarks/>
        public SecurePaymentAuthenticationVersionType SecurePaymentAuthenticationVersion
        {
            get
            {
                return securePaymentAuthenticationVersionField;
            }
            set
            {
                securePaymentAuthenticationVersionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SecurePaymentAuthenticationVersionType
    {

        private string[] cardEnrollmentVersionTextField;

        private string[] supportedVersionTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CardEnrollmentVersionText")]
        public string[] CardEnrollmentVersionText
        {
            get
            {
                return cardEnrollmentVersionTextField;
            }
            set
            {
                cardEnrollmentVersionTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SupportedVersionText")]
        public string[] SupportedVersionText
        {
            get
            {
                return supportedVersionTextField;
            }
            set
            {
                supportedVersionTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PolicyType
    {

        private DescType[] descField;

        private string ownerNameField;

        private PolicyNodeInfoType[] policyNodeInfoField;

        private string policyTypeTextField;

        private decimal versionNumberField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Desc")]
        public DescType[] Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return ownerNameField;
            }
            set
            {
                ownerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PolicyNodeInfo")]
        public PolicyNodeInfoType[] PolicyNodeInfo
        {
            get
            {
                return policyNodeInfoField;
            }
            set
            {
                policyNodeInfoField = value;
            }
        }

        /// <remarks/>
        public string PolicyTypeText
        {
            get
            {
                return policyTypeTextField;
            }
            set
            {
                policyTypeTextField = value;
            }
        }

        /// <remarks/>
        public decimal VersionNumber
        {
            get
            {
                return versionNumberField;
            }
            set
            {
                versionNumberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DescType
    {

        private string descTextField;

        private string markupStyleTextField;

        private string uRLField;

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public string MarkupStyleText
        {
            get
            {
                return markupStyleTextField;
            }
            set
            {
                markupStyleTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return uRLField;
            }
            set
            {
                uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PolicyNodeInfoType
    {

        private string pathTextField;

        private string tagNameField;

        /// <remarks/>
        public string PathText
        {
            get
            {
                return pathTextField;
            }
            set
            {
                pathTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TagName
        {
            get
            {
                return tagNameField;
            }
            set
            {
                tagNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ResponseParametersRequestType
    {

        private BDC_Type bDCField;

        private CurParameterType[] curParameterField;

        private DeviceType deviceField;

        private DeviceLocationType deviceLocationField;

        private bool inventoryGuaranteeIndField;

        private bool inventoryGuaranteeIndFieldSpecified;

        private LangUsageType[] langUsageField;

        private MeasurementSystemCodeType measurementSystemCodeField;

        private bool measurementSystemCodeFieldSpecified;

        private PaxGroupType paxGroupField;

        private PricingParameterType pricingParameterField;

        private string reasonCodeField;

        private RemarkType[] remarkField;

        /// <remarks/>
        public BDC_Type BDC
        {
            get
            {
                return bDCField;
            }
            set
            {
                bDCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CurParameter")]
        public CurParameterType[] CurParameter
        {
            get
            {
                return curParameterField;
            }
            set
            {
                curParameterField = value;
            }
        }

        /// <remarks/>
        public DeviceType Device
        {
            get
            {
                return deviceField;
            }
            set
            {
                deviceField = value;
            }
        }

        /// <remarks/>
        public DeviceLocationType DeviceLocation
        {
            get
            {
                return deviceLocationField;
            }
            set
            {
                deviceLocationField = value;
            }
        }

        /// <remarks/>
        public bool InventoryGuaranteeInd
        {
            get
            {
                return inventoryGuaranteeIndField;
            }
            set
            {
                inventoryGuaranteeIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool InventoryGuaranteeIndSpecified
        {
            get
            {
                return inventoryGuaranteeIndFieldSpecified;
            }
            set
            {
                inventoryGuaranteeIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LangUsage")]
        public LangUsageType[] LangUsage
        {
            get
            {
                return langUsageField;
            }
            set
            {
                langUsageField = value;
            }
        }

        /// <remarks/>
        public MeasurementSystemCodeType MeasurementSystemCode
        {
            get
            {
                return measurementSystemCodeField;
            }
            set
            {
                measurementSystemCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MeasurementSystemCodeSpecified
        {
            get
            {
                return measurementSystemCodeFieldSpecified;
            }
            set
            {
                measurementSystemCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PaxGroupType PaxGroup
        {
            get
            {
                return paxGroupField;
            }
            set
            {
                paxGroupField = value;
            }
        }

        /// <remarks/>
        public PricingParameterType PricingParameter
        {
            get
            {
                return pricingParameterField;
            }
            set
            {
                pricingParameterField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ReasonCode
        {
            get
            {
                return reasonCodeField;
            }
            set
            {
                reasonCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return remarkField;
            }
            set
            {
                remarkField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BDC_Type
    {

        private BagRuleCodeType bagRuleCodeField;

        private bool bagRuleCodeFieldSpecified;

        private string bDC_AnalysisResultCodeField;

        private string bDC_ReasonTextField;

        private string carrierDesigCodeField;

        private string carrierNameField;

        /// <remarks/>
        public BagRuleCodeType BagRuleCode
        {
            get
            {
                return bagRuleCodeField;
            }
            set
            {
                bagRuleCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool BagRuleCodeSpecified
        {
            get
            {
                return bagRuleCodeFieldSpecified;
            }
            set
            {
                bagRuleCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string BDC_AnalysisResultCode
        {
            get
            {
                return bDC_AnalysisResultCodeField;
            }
            set
            {
                bDC_AnalysisResultCodeField = value;
            }
        }

        /// <remarks/>
        public string BDC_ReasonText
        {
            get
            {
                return bDC_ReasonTextField;
            }
            set
            {
                bDC_ReasonTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierDesigCode
        {
            get
            {
                return carrierDesigCodeField;
            }
            set
            {
                carrierDesigCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierName
        {
            get
            {
                return carrierNameField;
            }
            set
            {
                carrierNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum BagRuleCodeType
    {

        /// <remarks/>
        D,

        /// <remarks/>
        N,

        /// <remarks/>
        Other,

        /// <remarks/>
        Y,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CurParameterType
    {

        private CurAppCodeType appCodeField;

        private bool appCodeFieldSpecified;

        private string curCodeField;

        private decimal decimalsAllowedNumberField;

        private bool decimalsAllowedNumberFieldSpecified;

        private string nameField;

        /// <remarks/>
        public CurAppCodeType AppCode
        {
            get
            {
                return appCodeField;
            }
            set
            {
                appCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AppCodeSpecified
        {
            get
            {
                return appCodeFieldSpecified;
            }
            set
            {
                appCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CurCode
        {
            get
            {
                return curCodeField;
            }
            set
            {
                curCodeField = value;
            }
        }

        /// <remarks/>
        public decimal DecimalsAllowedNumber
        {
            get
            {
                return decimalsAllowedNumberField;
            }
            set
            {
                decimalsAllowedNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DecimalsAllowedNumberSpecified
        {
            get
            {
                return decimalsAllowedNumberFieldSpecified;
            }
            set
            {
                decimalsAllowedNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum CurAppCodeType
    {

        /// <remarks/>
        Actual,

        /// <remarks/>
        Conversion,

        /// <remarks/>
        Display,

        /// <remarks/>
        Other,

        /// <remarks/>
        Requested,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DeviceType
    {

        private string deviceCodeField;

        private OwnershipTypeCodeType deviceOwnerTypeCodeField;

        private bool deviceOwnerTypeCodeFieldSpecified;

        private PresenceTypeCodeType presenceTypeCodeField;

        private bool presenceTypeCodeFieldSpecified;

        /// <remarks/>
        public string DeviceCode
        {
            get
            {
                return deviceCodeField;
            }
            set
            {
                deviceCodeField = value;
            }
        }

        /// <remarks/>
        public OwnershipTypeCodeType DeviceOwnerTypeCode
        {
            get
            {
                return deviceOwnerTypeCodeField;
            }
            set
            {
                deviceOwnerTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DeviceOwnerTypeCodeSpecified
        {
            get
            {
                return deviceOwnerTypeCodeFieldSpecified;
            }
            set
            {
                deviceOwnerTypeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PresenceTypeCodeType PresenceTypeCode
        {
            get
            {
                return presenceTypeCodeField;
            }
            set
            {
                presenceTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PresenceTypeCodeSpecified
        {
            get
            {
                return presenceTypeCodeFieldSpecified;
            }
            set
            {
                presenceTypeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum OwnershipTypeCodeType
    {

        /// <remarks/>
        Customer,

        /// <remarks/>
        Seller,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum PresenceTypeCodeType
    {

        /// <remarks/>
        CN,

        /// <remarks/>
        CP,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DeviceLocationType
    {

        private CityType cityField;

        private CountryType countryField;

        private GeospatialLocationType geospatialLocationField;

        /// <remarks/>
        public CityType City
        {
            get
            {
                return cityField;
            }
            set
            {
                cityField = value;
            }
        }

        /// <remarks/>
        public CountryType Country
        {
            get
            {
                return countryField;
            }
            set
            {
                countryField = value;
            }
        }

        /// <remarks/>
        public GeospatialLocationType GeospatialLocation
        {
            get
            {
                return geospatialLocationField;
            }
            set
            {
                geospatialLocationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class GeospatialLocationType
    {

        private string naturalAreaGeocodeField;

        private decimal pointElevationNumberField;

        private bool pointElevationNumberFieldSpecified;

        private decimal pointLatitudeNumberField;

        private bool pointLatitudeNumberFieldSpecified;

        private decimal pointLongitudeNumberField;

        private bool pointLongitudeNumberFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string NaturalAreaGeocode
        {
            get
            {
                return naturalAreaGeocodeField;
            }
            set
            {
                naturalAreaGeocodeField = value;
            }
        }

        /// <remarks/>
        public decimal PointElevationNumber
        {
            get
            {
                return pointElevationNumberField;
            }
            set
            {
                pointElevationNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PointElevationNumberSpecified
        {
            get
            {
                return pointElevationNumberFieldSpecified;
            }
            set
            {
                pointElevationNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal PointLatitudeNumber
        {
            get
            {
                return pointLatitudeNumberField;
            }
            set
            {
                pointLatitudeNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PointLatitudeNumberSpecified
        {
            get
            {
                return pointLatitudeNumberFieldSpecified;
            }
            set
            {
                pointLatitudeNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal PointLongitudeNumber
        {
            get
            {
                return pointLongitudeNumberField;
            }
            set
            {
                pointLongitudeNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PointLongitudeNumberSpecified
        {
            get
            {
                return pointLongitudeNumberFieldSpecified;
            }
            set
            {
                pointLongitudeNumberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum MeasurementSystemCodeType
    {

        /// <remarks/>
        Imperial,

        /// <remarks/>
        Metric,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxGroupType
    {

        private ContactInfoType[] contactInfoField;

        private decimal intendedPaxQtyField;

        private bool intendedPaxQtyFieldSpecified;

        private string paxGroupIDField;

        private string paxGroupNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ContactInfo")]
        public ContactInfoType[] ContactInfo
        {
            get
            {
                return contactInfoField;
            }
            set
            {
                contactInfoField = value;
            }
        }

        /// <remarks/>
        public decimal IntendedPaxQty
        {
            get
            {
                return intendedPaxQtyField;
            }
            set
            {
                intendedPaxQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IntendedPaxQtySpecified
        {
            get
            {
                return intendedPaxQtyFieldSpecified;
            }
            set
            {
                intendedPaxQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxGroupID
        {
            get
            {
                return paxGroupIDField;
            }
            set
            {
                paxGroupIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxGroupName
        {
            get
            {
                return paxGroupNameField;
            }
            set
            {
                paxGroupNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PricingParameterType
    {

        private bool autoExchIndField;

        private bool autoExchIndFieldSpecified;

        private bool awardIncludedIndField;

        private bool awardIncludedIndFieldSpecified;

        private bool awardOnlyIndField;

        private bool awardOnlyIndFieldSpecified;

        private bool includeUpsellOffersIndField;

        private bool includeUpsellOffersIndFieldSpecified;

        private string overrideCurCodeField;

        private bool simplePricingIndField;

        private bool simplePricingIndFieldSpecified;

        private TaxSummaryType[] taxExemptionField;

        /// <remarks/>
        public bool AutoExchInd
        {
            get
            {
                return autoExchIndField;
            }
            set
            {
                autoExchIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AutoExchIndSpecified
        {
            get
            {
                return autoExchIndFieldSpecified;
            }
            set
            {
                autoExchIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool AwardIncludedInd
        {
            get
            {
                return awardIncludedIndField;
            }
            set
            {
                awardIncludedIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AwardIncludedIndSpecified
        {
            get
            {
                return awardIncludedIndFieldSpecified;
            }
            set
            {
                awardIncludedIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool AwardOnlyInd
        {
            get
            {
                return awardOnlyIndField;
            }
            set
            {
                awardOnlyIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AwardOnlyIndSpecified
        {
            get
            {
                return awardOnlyIndFieldSpecified;
            }
            set
            {
                awardOnlyIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool IncludeUpsellOffersInd
        {
            get
            {
                return includeUpsellOffersIndField;
            }
            set
            {
                includeUpsellOffersIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IncludeUpsellOffersIndSpecified
        {
            get
            {
                return includeUpsellOffersIndFieldSpecified;
            }
            set
            {
                includeUpsellOffersIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OverrideCurCode
        {
            get
            {
                return overrideCurCodeField;
            }
            set
            {
                overrideCurCodeField = value;
            }
        }

        /// <remarks/>
        public bool SimplePricingInd
        {
            get
            {
                return simplePricingIndField;
            }
            set
            {
                simplePricingIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool SimplePricingIndSpecified
        {
            get
            {
                return simplePricingIndFieldSpecified;
            }
            set
            {
                simplePricingIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("TaxExemption")]
        public TaxSummaryType[] TaxExemption
        {
            get
            {
                return taxExemptionField;
            }
            set
            {
                taxExemptionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TaxSummaryType
    {

        private bool allRefundableIndField;

        private bool allRefundableIndFieldSpecified;

        private bool approximateIndField;

        private bool approximateIndFieldSpecified;

        private bool collectionIndField;

        private bool collectionIndFieldSpecified;

        private System.DateTime guaranteeTimeLimitDateTimeField;

        private bool guaranteeTimeLimitDateTimeFieldSpecified;

        private string refundMethodTextField;

        private TaxType[] taxField;

        private AmountType totalRefundableTaxAmountField;

        private AmountType totalTaxAmountField;

        /// <remarks/>
        public bool AllRefundableInd
        {
            get
            {
                return allRefundableIndField;
            }
            set
            {
                allRefundableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AllRefundableIndSpecified
        {
            get
            {
                return allRefundableIndFieldSpecified;
            }
            set
            {
                allRefundableIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return approximateIndField;
            }
            set
            {
                approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return approximateIndFieldSpecified;
            }
            set
            {
                approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool CollectionInd
        {
            get
            {
                return collectionIndField;
            }
            set
            {
                collectionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool CollectionIndSpecified
        {
            get
            {
                return collectionIndFieldSpecified;
            }
            set
            {
                collectionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime GuaranteeTimeLimitDateTime
        {
            get
            {
                return guaranteeTimeLimitDateTimeField;
            }
            set
            {
                guaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool GuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return guaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                guaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string RefundMethodText
        {
            get
            {
                return refundMethodTextField;
            }
            set
            {
                refundMethodTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Tax")]
        public TaxType[] Tax
        {
            get
            {
                return taxField;
            }
            set
            {
                taxField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalRefundableTaxAmount
        {
            get
            {
                return totalRefundableTaxAmountField;
            }
            set
            {
                totalRefundableTaxAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalTaxAmount
        {
            get
            {
                return totalTaxAmountField;
            }
            set
            {
                totalTaxAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceListCoreRequestType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OfferRequest", typeof(ServiceListOfferRequestType))]
        [System.Xml.Serialization.XmlElement("OrderRequest", typeof(ServiceListOrderRequestType))]
        [System.Xml.Serialization.XmlElement("PaxSegmentRequest", typeof(PaxSegmentRequestType))]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceListOfferRequestType
    {

        private ServiceListOfferType[] offerField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Offer")]
        public ServiceListOfferType[] Offer
        {
            get
            {
                return offerField;
            }
            set
            {
                offerField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceListOfferType
    {

        private string offerIDField;

        private ServiceListOfferItemType[] offerItemField;

        private string ownerCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OfferID
        {
            get
            {
                return offerIDField;
            }
            set
            {
                offerIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OfferItem")]
        public ServiceListOfferItemType[] OfferItem
        {
            get
            {
                return offerItemField;
            }
            set
            {
                offerItemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceListOfferItemType
    {

        private string[] datedOperatingLegRefIDField;

        private string offerItemIDField;

        private string ownerCodeField;

        private string[] paxRefIDField;

        private string[] paxSegmentRefIDField;

        private ServiceIdKeyType[] serviceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return datedOperatingLegRefIDField;
            }
            set
            {
                datedOperatingLegRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OfferItemID
        {
            get
            {
                return offerItemIDField;
            }
            set
            {
                offerItemIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return paxSegmentRefIDField;
            }
            set
            {
                paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Service")]
        public ServiceIdKeyType[] Service
        {
            get
            {
                return serviceField;
            }
            set
            {
                serviceField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceIdKeyType
    {

        private string serviceIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ServiceID
        {
            get
            {
                return serviceIDField;
            }
            set
            {
                serviceIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceListOrderRequestType
    {

        private ServiceRequestOrderType orderField;

        /// <remarks/>
        public ServiceRequestOrderType Order
        {
            get
            {
                return orderField;
            }
            set
            {
                orderField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceRequestOrderType
    {

        private string orderIDField;

        private ServiceRequestOrderItemType[] orderItemField;

        private string ownerCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OrderID
        {
            get
            {
                return orderIDField;
            }
            set
            {
                orderIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OrderItem")]
        public ServiceRequestOrderItemType[] OrderItem
        {
            get
            {
                return orderItemField;
            }
            set
            {
                orderItemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceRequestOrderItemType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private ShoppingRequestCabinTypeType cabinTypeUpgradeField;

        private string[] datedOperatingLegRefIDField;

        private string orderItemIDField;

        private string ownerCodeField;

        private string[] paxRefIDField;

        private string[] paxSegmentRefIDField;

        private ServiceKeyType[] serviceField;

        private SpecialServiceType specialServiceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public ShoppingRequestCabinTypeType CabinTypeUpgrade
        {
            get
            {
                return cabinTypeUpgradeField;
            }
            set
            {
                cabinTypeUpgradeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return datedOperatingLegRefIDField;
            }
            set
            {
                datedOperatingLegRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OrderItemID
        {
            get
            {
                return orderItemIDField;
            }
            set
            {
                orderItemIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return paxSegmentRefIDField;
            }
            set
            {
                paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Service")]
        public ServiceKeyType[] Service
        {
            get
            {
                return serviceField;
            }
            set
            {
                serviceField = value;
            }
        }

        /// <remarks/>
        public SpecialServiceType SpecialService
        {
            get
            {
                return specialServiceField;
            }
            set
            {
                specialServiceField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ShoppingRequestCabinTypeType
    {

        private string cabinTypeCodeField;

        private PrefLevelType prefLevelField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CabinTypeCode
        {
            get
            {
                return cabinTypeCodeField;
            }
            set
            {
                cabinTypeCodeField = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return prefLevelField;
            }
            set
            {
                prefLevelField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceKeyType
    {

        private BookingRefType[] bookingRefField;

        private string[] datedOperatingLegRefField;

        private string serviceIDField;

        private CarrierType validatingCarrierField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("BookingRef")]
        public BookingRefType[] BookingRef
        {
            get
            {
                return bookingRefField;
            }
            set
            {
                bookingRefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DatedOperatingLegRefID", DataType = "token", IsNullable = false)]
        public string[] DatedOperatingLegRef
        {
            get
            {
                return datedOperatingLegRefField;
            }
            set
            {
                datedOperatingLegRefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ServiceID
        {
            get
            {
                return serviceIDField;
            }
            set
            {
                serviceIDField = value;
            }
        }

        /// <remarks/>
        public CarrierType ValidatingCarrier
        {
            get
            {
                return validatingCarrierField;
            }
            set
            {
                validatingCarrierField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BookingRefType
    {

        private BookingEntityType bookingEntityField;

        private string bookingIDField;

        private string bookingRefTypeCodeField;

        /// <remarks/>
        public BookingEntityType BookingEntity
        {
            get
            {
                return bookingEntityField;
            }
            set
            {
                bookingEntityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string BookingID
        {
            get
            {
                return bookingIDField;
            }
            set
            {
                bookingIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string BookingRefTypeCode
        {
            get
            {
                return bookingRefTypeCodeField;
            }
            set
            {
                bookingRefTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BookingEntityType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Carrier", typeof(CarrierType))]
        [System.Xml.Serialization.XmlElement("Org", typeof(OrgType))]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxSegmentRequestType
    {

        private ShoppingRequestPaxSegmentType[] paxSegmentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxSegment")]
        public ShoppingRequestPaxSegmentType[] PaxSegment
        {
            get
            {
                return paxSegmentField;
            }
            set
            {
                paxSegmentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ShoppingRequestPaxSegmentType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private CabinTypeAssociationChoiceType cabinTypeAssociationChoiceField;

        private string datedMarketingSegmentRefIdField;

        private string[] fareBasisCodeField;

        private string paxSegmentIDField;

        private MediaResourceRefType[] richMediaField;

        private SettlementInfoType settlementInfoField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public CabinTypeAssociationChoiceType CabinTypeAssociationChoice
        {
            get
            {
                return cabinTypeAssociationChoiceField;
            }
            set
            {
                cabinTypeAssociationChoiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DatedMarketingSegmentRefId
        {
            get
            {
                return datedMarketingSegmentRefIdField;
            }
            set
            {
                datedMarketingSegmentRefIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("FareBasisCode", DataType = "token")]
        public string[] FareBasisCode
        {
            get
            {
                return fareBasisCodeField;
            }
            set
            {
                fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxSegmentID
        {
            get
            {
                return paxSegmentIDField;
            }
            set
            {
                paxSegmentIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }

        /// <remarks/>
        public SettlementInfoType SettlementInfo
        {
            get
            {
                return settlementInfoField;
            }
            set
            {
                settlementInfoField = value;
            }
        }
    }

}
