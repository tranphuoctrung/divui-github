using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Divui.Orders
{
    public partial class ShoppingCartItemAttributeMappingMap : NopEntityTypeConfiguration<ShoppingCartItemAttributeMapping>
    {
        public ShoppingCartItemAttributeMappingMap()
        {
            this.ToTable("ShoppingCartItem_ProductAttribute_Mapping");
            this.HasKey(pam => pam.Id);
            this.Ignore(pam => pam.AttributeControlType);

            this.HasRequired(pam => pam.ShoppingCartItem)
                .WithMany(p => p.ShoppingCartItemAttributeMappings)
                .HasForeignKey(pam => pam.ShoppingCartItemId);

            this.HasRequired(pam => pam.ProductAttribute)
                .WithMany()
                .HasForeignKey(pam => pam.ProductAttributeId);
        }
    }
}
