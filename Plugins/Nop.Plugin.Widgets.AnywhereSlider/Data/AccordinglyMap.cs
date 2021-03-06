using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;

namespace Nop.Plugin.Widgets.AnywhereSlider.Data
{
    public partial class AccordinglyMap : NopEntityTypeConfiguration<Accordingly>
    {
        public override void Configure(EntityTypeBuilder<Accordingly> builder)
        {
            builder.ToTable(nameof(Accordingly));

            builder.HasKey(s => s.Id);

            base.Configure(builder);
        }
    }
}
