using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.eShopSMS.Domains;

namespace Nop.Plugin.Misc.eShopSMS.Data
{
    public partial class SMSTemplateMap : NopEntityTypeConfiguration<SMSTemplate>
    {
        public override void Configure(EntityTypeBuilder<SMSTemplate> builder)
        {
            builder.ToTable(nameof(SMSTemplate));

            builder.HasKey(rcoh => rcoh.Id);
        }
    }
}
