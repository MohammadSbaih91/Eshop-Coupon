using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public partial class BranchDetail
    {
        [JsonProperty("citiesMap")]
        public Dictionary<string, CitiesMap> CitiesMap { get; set; }

        [JsonProperty("branchesMap")]
        public Dictionary<string, List<Branch>> BranchesMap { get; set; }

        [JsonProperty("servicesMap")]
        public Dictionary<string, List<ServicesMap>> ServicesMap { get; set; }
    }
    
    public partial class CitiesMap
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("regionID")]
        public long RegionId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class ServicesMap
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("serviceGroupID")]
        public long ServiceGroupId { get; set; }

        [JsonProperty("departmentID")]
        public long DepartmentId { get; set; }

        [JsonProperty("availableForAppointment")]
        public bool AvailableForAppointment { get; set; }

        [JsonProperty("availableforWalkin")]
        public bool AvailableforWalkin { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    //public enum From { The0000, The0800, The0859, The0900, The1000, The1100, The1400 };

    //public enum Name { AfterSalesServices, BusnissSme, DevicesSales, DirectPayment, DirectRefill, InternetSales, MobileSales, OrangeMoney, Others };

    public partial class BranchDetail
    {
        public static BranchDetail FromJson(string json) => JsonConvert.DeserializeObject<BranchDetail>(json, Converter.Settings);
    }

    public static class BranchDetailSerialize
    {
        public static string ToJson(this BranchDetail self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
    

    //internal class FromConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(From) || t == typeof(From?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        switch (value)
    //        {
    //            case "00:00":
    //                return From.The0000;
    //            case "08:00":
    //                return From.The0800;
    //            case "08:59":
    //                return From.The0859;
    //            case "09:00":
    //                return From.The0900;
    //            case "10:00":
    //                return From.The1000;
    //            case "11:00":
    //                return From.The1100;
    //            case "14:00":
    //                return From.The1400;
    //        }
    //        throw new Exception("Cannot unmarshal type From");
    //    }

        //public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        //{
        //    if (untypedValue == null)
        //    {
        //        serializer.Serialize(writer, null);
        //        return;
        //    }
        //    var value = (From)untypedValue;
        //    switch (value)
        //    {
        //        case From.The0000:
        //            serializer.Serialize(writer, "00:00");
        //            return;
        //        case From.The0800:
        //            serializer.Serialize(writer, "08:00");
        //            return;
        //        case From.The0859:
        //            serializer.Serialize(writer, "08:59");
        //            return;
        //        case From.The0900:
        //            serializer.Serialize(writer, "09:00");
        //            return;
        //        case From.The1000:
        //            serializer.Serialize(writer, "10:00");
        //            return;
        //        case From.The1100:
        //            serializer.Serialize(writer, "11:00");
        //            return;
        //        case From.The1400:
        //            serializer.Serialize(writer, "14:00");
        //            return;
        //    }
        //    throw new Exception("Cannot marshal type From");
        //}

        //public static readonly FromConverter Singleton = new FromConverter();
    //

    //internal class NameConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        switch (value)
    //        {
    //            case "After Sales Services":
    //                return Name.AfterSalesServices;
    //            case "Busniss & SME":
    //                return Name.BusnissSme;
    //            case "Devices Sales":
    //                return Name.DevicesSales;
    //            case "Direct Payment":
    //                return Name.DirectPayment;
    //            case "Direct Refill":
    //                return Name.DirectRefill;
    //            case "Internet Sales":
    //                return Name.InternetSales;
    //            case "Mobile Sales":
    //                return Name.MobileSales;
    //            case "Orange Money":
    //                return Name.OrangeMoney;
    //            case "Others":
    //                return Name.Others;
    //        }
    //        throw new Exception("Cannot unmarshal type Name");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (Name)untypedValue;
    //        switch (value)
    //        {
    //            case Name.AfterSalesServices:
    //                serializer.Serialize(writer, "After Sales Services");
    //                return;
    //            case Name.BusnissSme:
    //                serializer.Serialize(writer, "Busniss & SME");
    //                return;
    //            case Name.DevicesSales:
    //                serializer.Serialize(writer, "Devices Sales");
    //                return;
    //            case Name.DirectPayment:
    //                serializer.Serialize(writer, "Direct Payment");
    //                return;
    //            case Name.DirectRefill:
    //                serializer.Serialize(writer, "Direct Refill");
    //                return;
    //            case Name.InternetSales:
    //                serializer.Serialize(writer, "Internet Sales");
    //                return;
    //            case Name.MobileSales:
    //                serializer.Serialize(writer, "Mobile Sales");
    //                return;
    //            case Name.OrangeMoney:
    //                serializer.Serialize(writer, "Orange Money");
    //                return;
    //            case Name.Others:
    //                serializer.Serialize(writer, "Others");
    //                return;
    //        }
    //        throw new Exception("Cannot marshal type Name");
    //    }

    //    public static readonly NameConverter Singleton = new NameConverter();
    //}
}

