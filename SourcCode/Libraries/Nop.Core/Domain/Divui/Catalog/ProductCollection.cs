namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product collection mapping
    /// </summary>
    public partial class ProductCollection : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the collection identifier
        /// </summary>
        public int CollectionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is featured
        /// </summary>
        public bool IsFeaturedProduct { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the collection
        /// </summary>
        public virtual Collection Collection { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

    }

}
