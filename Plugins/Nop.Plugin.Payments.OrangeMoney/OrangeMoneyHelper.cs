namespace Nop.Plugin.Payments.OrangeMoney
{
    public partial class OrangeMoneyHelper {
        public const string ORDER_POSTFIX_STRING = "OrderPostfixString";
    }

    public partial class StatusCode
    {
        // success
        public const string OPC00000 = "OPC-00000";
        
        //	Merchant Code Does Not Exist
        public const string OPC00130 = "OPC-00130";
        
        //Currency Does Not Exist
        public const string OPC00131 = "OPC-00131";

        //Transaction Not Found
        public const string OPC00132 = "OPC-00132";
    }
}
