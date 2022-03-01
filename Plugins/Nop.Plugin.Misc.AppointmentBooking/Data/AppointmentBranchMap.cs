using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.AppointmentBooking.Domains;

namespace Nop.Plugin.Misc.AppointmentBooking.Data
{
    public class AppointmentBranchMap : NopEntityTypeConfiguration<AppointmentBranch>
    {
        public override void Configure(EntityTypeBuilder<AppointmentBranch> builder)
        {
            builder.ToTable(nameof(AppointmentBranch));

            builder.HasKey(ba => ba.Id);
        }
    }
}
