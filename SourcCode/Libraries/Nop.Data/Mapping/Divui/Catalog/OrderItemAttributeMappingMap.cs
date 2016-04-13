using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class OrderItemAttributeMappingMap : NopEntityTypeConfiguration<OrderItemAttributeMapping>
    {
        public OrderItemAttributeMappingMap()
        {
            this.ToTable("OrderItem_OrderItemAttribute_Mapping");
            this.HasKey(pam => pam.Id);
            this.Ignore(pam => pam.AttributeControlType);

            this.HasRequired(pam => pam.OrderItem)
                .WithMany(p => p.OrderItemAttributeMappings)
                .HasForeignKey(pam => pam.OrderItemId);

            this.HasRequired(pam => pam.ProductAttribute)
                .WithMany()
                .HasForeignKey(pam => pam.ProductAttributeId);
        }
    }
}