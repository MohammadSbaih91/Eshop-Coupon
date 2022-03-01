using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;

namespace Nop.Plugin.Widgets.AnywhereSlider.Data
{
    public partial class SliderGroupMap : NopEntityTypeConfiguration<SliderGroup>
    {
        public override void Configure(EntityTypeBuilder<SliderGroup> builder)
        {
            builder.ToTable(nameof(SliderGroup));

            builder.HasKey(s => s.Id);

            base.Configure(builder);
        }
    }
}
