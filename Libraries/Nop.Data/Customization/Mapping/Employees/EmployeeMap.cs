using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Employees;

namespace Nop.Data.Mapping.Employees
{
    public class EmployeeMap : NopEntityTypeConfiguration<Employee>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("EmployeeDetail");
            builder.HasKey(emp => emp.Id);
            
            base.Configure(builder);
        }

        #endregion
    }
}
