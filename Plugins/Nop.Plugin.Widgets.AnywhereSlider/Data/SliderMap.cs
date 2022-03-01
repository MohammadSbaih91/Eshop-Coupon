using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;

namespace Nop.Plugin.Widgets.AnywhereSlider.Data
{
    public partial class SliderMap : NopEntityTypeConfiguration<Slider>
    {
        public override void Configure(EntityTypeBuilder<Slider> builder)
        {
            builder.ToTable(nameof(Slider));

            builder.HasKey(s => s.Id);

            base.Configure(builder);
        }
    }
}
