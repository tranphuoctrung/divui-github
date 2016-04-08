using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductAttractionMap : NopEntityTypeConfiguration<ProductAttraction>
    {
        public ProductAttractionMap()
        {
            this.ToTable("Product_Attraction_Mapping");
            this.HasKey(pc => pc.Id);

            this.HasRequired(pc => pc.Attraction)
                .WithMany()
                .HasForeignKey(pc => pc.AttractionId);


            this.HasRequired(pc => pc.Product)
                .WithMany(p => p.ProductAttractions)
                .HasForeignKey(pc => pc.ProductId);
        }
    }
}