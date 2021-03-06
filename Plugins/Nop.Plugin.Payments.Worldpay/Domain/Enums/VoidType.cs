
namespace Nop.Plugin.Payments.Worldpay.Domain.Enums
{
    /// <summary>
    /// Void type enumeration. Indicates whether the void is merchant generated or system generated.
    /// </summary>
    public enum VoidType
    {
        /// <summary>
        /// Generated by merchant
        /// </summary>
        MerchantGenerated = 1,

        /// <summary>
        /// Generated by system
        /// </summary>
        SystemGenerated = 2,
    }
}