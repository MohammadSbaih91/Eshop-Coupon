using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Nop.Plugin.Shipping.Aramex.Models.Responce
{
    [XmlRoot(ElementName = "Transaction", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
    public class Transaction
    {
        [XmlElement(ElementName = "Reference1", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string Reference1 { get; set; }
        [XmlElement(ElementName = "Reference2", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string Reference2 { get; set; }
        [XmlElement(ElementName = "Reference3", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string Reference3 { get; set; }
        [XmlElement(ElementName = "Reference4", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string Reference4 { get; set; }
        [XmlElement(ElementName = "Reference5", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string Reference5 { get; set; }
        [XmlAttribute(AttributeName = "i", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string I { get; set; }
    }

    [XmlRoot(ElementName = "Notifications", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
    public class Notifications
    {
        [XmlAttribute(AttributeName = "i", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string I { get; set; }
    }

    [XmlRoot(ElementName = "TrackingResult", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
    public class TrackingResult
    {
        [XmlElement(ElementName = "WaybillNumber", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string WaybillNumber { get; set; }
        [XmlElement(ElementName = "UpdateCode", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string UpdateCode { get; set; }
        [XmlElement(ElementName = "UpdateDescription", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string UpdateDescription { get; set; }
        [XmlElement(ElementName = "UpdateDateTime", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public DateTime UpdateDateTime { get; set; }
        [XmlElement(ElementName = "UpdateLocation", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string UpdateLocation { get; set; }
        [XmlElement(ElementName = "Comments", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string Comments { get; set; }
        [XmlElement(ElementName = "ProblemCode", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string ProblemCode { get; set; }
        [XmlElement(ElementName = "GrossWeight", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string GrossWeight { get; set; }
        [XmlElement(ElementName = "ChargeableWeight", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string ChargeableWeight { get; set; }
        [XmlElement(ElementName = "WeightUnit", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string WeightUnit { get; set; }
    }

    [XmlRoot(ElementName = "Value", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
    public class Value
    {
        [XmlElement(ElementName = "TrackingResult", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public List<TrackingResult> TrackingResult { get; set; }
    }

    [XmlRoot(ElementName = "KeyValueOfstringArrayOfTrackingResultmFAkxlpY", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
    public class KeyValueOfstringArrayOfTrackingResultmFAkxlpY
    {
        [XmlElement(ElementName = "Key", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public string Key { get; set; }
        [XmlElement(ElementName = "Value", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public Value Value { get; set; }
    }

    [XmlRoot(ElementName = "TrackingResults", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
    public class TrackingResults
    {
        [XmlElement(ElementName = "KeyValueOfstringArrayOfTrackingResultmFAkxlpY", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public KeyValueOfstringArrayOfTrackingResultmFAkxlpY KeyValueOfstringArrayOfTrackingResultmFAkxlpY { get; set; }
        [XmlAttribute(AttributeName = "a", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string A { get; set; }
        [XmlAttribute(AttributeName = "i", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string I { get; set; }
    }

    [XmlRoot(ElementName = "NonExistingWaybills", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
    public class NonExistingWaybills
    {
        [XmlAttribute(AttributeName = "a", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string A { get; set; }
        [XmlAttribute(AttributeName = "i", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string I { get; set; }
    }

    [XmlRoot(ElementName = "ShipmentTrackingResponse", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
    public class ShipmentTrackingResponse
    {
        [XmlElement(ElementName = "Transaction", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public Transaction Transaction { get; set; }
        [XmlElement(ElementName = "Notifications", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public Notifications Notifications { get; set; }
        [XmlElement(ElementName = "HasErrors", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public string HasErrors { get; set; }
        [XmlElement(ElementName = "TrackingResults", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public TrackingResults TrackingResults { get; set; }
        [XmlElement(ElementName = "NonExistingWaybills", Namespace = "http://ws.aramex.net/ShippingAPI/v1/")]
        public NonExistingWaybills NonExistingWaybills { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

}
