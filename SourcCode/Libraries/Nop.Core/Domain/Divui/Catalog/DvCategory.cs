using Nop.Core.Domain.Divui.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class Category
    {
        private ICollection<CategorySpecificationAttribute> _categorySpecificationAttribute;

        /// <summary>
        /// Gets or sets the ShortDescription
        /// </summary>
        public string ShortDescription { get; set; }

        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the product type identifier
        /// </summary>
        public int CategoryTypeId { get; set; }

        /// <summary>
        /// Gets or sets the product type
        /// </summary>
        public CategoryType CategoryType
        {
            get
            {
                return (CategoryType)this.CategoryTypeId;
            }
            set
            {
                this.CategoryTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the product specification attribute
        /// </summary>
        public virtual ICollection<CategorySpecificationAttribute> CategorySpecificationAttributes
        {
            get { return _categorySpecificationAttribute ?? (_categorySpecificationAttribute = new List<CategorySpecificationAttribute>()); }
            protected set { _categorySpecificationAttribute = value; }
        }
    }
}
