using Nop.Core.Domain.Divui.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Divui.Catalog
{
    public partial class BannerMap : NopEntityTypeConfiguration<Banner>
    {
        public BannerMap()
        {
            this.ToTable("DvBanner");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(400);
            this.Property(c => c.Description).IsMaxLength();
            this.Property(c => c.Url).IsMaxLength();

            this.HasRequired(b => b.Picture)
               .WithMany()
               .HasForeignKey(b => b.PictureId);

        }
    }
}
