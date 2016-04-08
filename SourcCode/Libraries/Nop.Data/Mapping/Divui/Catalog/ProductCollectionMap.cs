using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductCollectionMap : NopEntityTypeConfiguration<ProductCollection>
    {
        public ProductCollectionMap()
        {
            this.ToTable("Product_Collection_Mapping");
            this.HasKey(pc => pc.Id);
            
            this.HasRequired(pc => pc.Collection)
                .WithMany()
                .HasForeignKey(pc => pc.CollectionId);


            this.HasRequired(pc => pc.Product)
                .WithMany(p => p.ProductCollections)
                .HasForeignKey(pc => pc.ProductId);
        }
    }
}