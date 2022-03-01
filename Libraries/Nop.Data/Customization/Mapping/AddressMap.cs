using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;

namespace Nop.Data.Mapping.Common
{
    /// <summary>
    /// Represents an address mapping configuration
    /// </summary>
    public partial class AddressMap
    {
        #region Methods

        protected override void PostConfigure(EntityTypeBuilder<Address> builder)
        {
            builder.Property(address => address.StudentID);
            builder.Property(address => address.UploadStudentID);
            builder.Property(address => address.UploadID);
            builder.Property(address => address.BuildingNo);
            builder.Ignore(address => address.Civility);
            builder.Ignore(address => address.Nationality);
            builder.Ignore(address => address.NationalityType);
            base.PostConfigure(builder);
        }

        #endregion
    }
}

