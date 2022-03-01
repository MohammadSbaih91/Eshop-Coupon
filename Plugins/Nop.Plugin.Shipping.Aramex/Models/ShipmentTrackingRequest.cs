
using System;
using System.Collections.Generic;
using System.Text;


// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://ws.aramex.net/ShippingAPI/v1/", IsNullable = false)]
public partial class ShipmentTrackingRequest
{

    private ShipmentTrackingRequestClientInfo clientInfoField;

    private ShipmentTrackingRequestTransaction transactionField;

    private ShipmentTrackingRequestShipments shipmentsField;

    private bool getLastTrackingUpdateOnlyField;

    /// <remarks/>
    public ShipmentTrackingRequestClientInfo ClientInfo
    {
        get
        {
            return this.clientInfoField;
        }
        set
        {
            this.clientInfoField = value;
        }
    }

    /// <remarks/>
    public ShipmentTrackingRequestTransaction Transaction
    {
        get
        {
            return this.transactionField;
        }
        set
        {
            this.transactionField = value;
        }
    }

    /// <remarks/>
    public ShipmentTrackingRequestShipments Shipments
    {
        get
        {
            return this.shipmentsField;
        }
        set
        {
            this.shipmentsField = value;
        }
    }

    /// <remarks/>
    public bool GetLastTrackingUpdateOnly
    {
        get
        {
            return this.getLastTrackingUpdateOnlyField;
        }
        set
        {
            this.getLastTrackingUpdateOnlyField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
public partial class ShipmentTrackingRequestClientInfo
{

    private string userNameField;

    private string passwordField;

    private string versionField;

    private string accountNumberField;

    private uint accountPinField;

    private string accountEntityField;

    private string accountCountryCodeField;

    private string sourceField;

    /// <remarks/>
    public string UserName
    {
        get
        {
            return this.userNameField;
        }
        set
        {
            this.userNameField = value;
        }
    }

    /// <remarks/>
    public string Password
    {
        get
        {
            return this.passwordField;
        }
        set
        {
            this.passwordField = value;
        }
    }

    /// <remarks/>
    public string Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }

    /// <remarks/>
    public string AccountNumber
    {
        get
        {
            return this.accountNumberField;
        }
        set
        {
            this.accountNumberField = value;
        }
    }

    /// <remarks/>
    public uint AccountPin
    {
        get
        {
            return this.accountPinField;
        }
        set
        {
            this.accountPinField = value;
        }
    }

    /// <remarks/>
    public string AccountEntity
    {
        get
        {
            return this.accountEntityField;
        }
        set
        {
            this.accountEntityField = value;
        }
    }

    /// <remarks/>
    public string AccountCountryCode
    {
        get
        {
            return this.accountCountryCodeField;
        }
        set
        {
            this.accountCountryCodeField = value;
        }
    }

    /// <remarks/>
    public string Source
    {
        get
        {
            return this.sourceField;
        }
        set
        {
            this.sourceField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
public partial class ShipmentTrackingRequestTransaction
{

    private object reference1Field;

    private object reference2Field;

    private object reference3Field;

    private object reference4Field;

    private object reference5Field;

    /// <remarks/>
    public object Reference1
    {
        get
        {
            return this.reference1Field;
        }
        set
        {
            this.reference1Field = value;
        }
    }

    /// <remarks/>
    public object Reference2
    {
        get
        {
            return this.reference2Field;
        }
        set
        {
            this.reference2Field = value;
        }
    }

    /// <remarks/>
    public object Reference3
    {
        get
        {
            return this.reference3Field;
        }
        set
        {
            this.reference3Field = value;
        }
    }

    /// <remarks/>
    public object Reference4
    {
        get
        {
            return this.reference4Field;
        }
        set
        {
            this.reference4Field = value;
        }
    }

    /// <remarks/>
    public object Reference5
    {
        get
        {
            return this.reference5Field;
        }
        set
        {
            this.reference5Field = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
public partial class ShipmentTrackingRequestShipments
{

    private string stringField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
    public string @string
    {
        get
        {
            return this.stringField;
        }
        set
        {
            this.stringField = value;
        }
    }
}


