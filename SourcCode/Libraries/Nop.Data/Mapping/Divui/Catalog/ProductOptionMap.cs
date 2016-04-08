using Nop.Core.Domain.Divui.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Divui.Catalog
{
    public partial class ProductOptionMap : NopEntityTypeConfiguration<ProductOption>
    {
        public ProductOptionMap()
        {
            this.ToTable("ProductOption");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(400);

            this.HasRequired(g => g.Product)
               .WithMany()
               .HasForeignKey(g => g.ProductId);
        }
    }
}
