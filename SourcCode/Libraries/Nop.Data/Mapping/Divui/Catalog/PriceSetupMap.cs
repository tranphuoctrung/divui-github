using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Divui.Catalog
{
    public partial class PriceSetupMap : NopEntityTypeConfiguration<PriceSetup>
    {
        public PriceSetupMap()
        {
            this.ToTable("PriceSetup");
            this.HasKey(pc => pc.Id);

            this.HasRequired(ps => ps.Product)
                .WithMany(p => p.PriceSetups)
                .HasForeignKey(pc => pc.ProductId);

            this.HasRequired(ps => ps.CustomerRole)
                .WithMany(cr => cr.PriceSetups)
                .HasForeignKey(ps => ps.CustomerRoleId);
        }
    }
}
