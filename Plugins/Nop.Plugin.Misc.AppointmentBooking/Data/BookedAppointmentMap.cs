using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.AppointmentBooking.Domains;

namespace Nop.Plugin.Misc.AppointmentBooking.Data
{
    public partial class BookedAppointmentMap : NopEntityTypeConfiguration<BookedAppointment>
    {
        public override void Configure(EntityTypeBuilder<BookedAppointment> builder)
        {
            builder.ToTable(nameof(BookedAppointment));

            builder.HasKey(ba => ba.Id);
        }
    }
}
