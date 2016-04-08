using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Divui.Catalog
{
    public partial class AvailabilitySetupMap : NopEntityTypeConfiguration<AvailabilitySetup>
    {
        public AvailabilitySetupMap()
        {
            this.ToTable("AvailabilitySetup");
            this.HasKey(pc => pc.Id);

            this.HasRequired(ps => ps.Product)
                .WithMany(p => p.AvailabilitySetups)
                .HasForeignKey(pc => pc.ProductId);

            this.HasRequired(ps => ps.CustomerRole)
                .WithMany(cr => cr.AvailabilitySetups)
                .HasForeignKey(ps => ps.CustomerRoleId);
        }
    }
}
