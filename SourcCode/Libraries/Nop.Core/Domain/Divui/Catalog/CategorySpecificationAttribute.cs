namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a category specification attribute
    /// </summary>
    public partial class CategorySpecificationAttribute : BaseEntity
    {
        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the attribute type ID
        /// </summary>
        public int AttributeTypeId { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute identifier
        /// </summary>
        public int SpecificationAttributeOptionId { get; set; }

        /// <summary>
        /// Gets or sets the custom value
        /// </summary>
        public string CustomValue { get; set; }

        /// <summary>
        /// Gets or sets whether the attribute can be filtered by
        /// </summary>
        public bool AllowFiltering { get; set; }

        /// <summary>
        /// Gets or sets whether the attribute will be shown on the category page
        /// </summary>
        public bool ShowOnCategoryPage { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets or sets the category
        /// </summary>
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option
        /// </summary>
        public virtual SpecificationAttributeOption SpecificationAttributeOption { get; set; }

        /// <summary>
        /// Gets the attribute control type
        /// </summary>
        public SpecificationAttributeType AttributeType
        {
            get
            {
                return (SpecificationAttributeType)this.AttributeTypeId;
            }
            set
            {
                this.AttributeTypeId = (int)value;
            }
        }
    }
}
