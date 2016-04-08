using Nop.Core.Domain.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Divui.Catalog
{
    public partial class Banner : BaseEntity
    {
        public virtual string Name { get; set; }

        public virtual string Url { get; set; }

        public virtual int PictureId { get; set; }

        public virtual int DisplayOrder { get; set; }

        public virtual bool Published { get; set; }

        public virtual bool Deleted { get; set; }

        public virtual string Target { get; set; }

        public virtual DateTime? StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

        public virtual string Description { get; set; }

        public virtual Picture Picture { get; set; }

    }
}
