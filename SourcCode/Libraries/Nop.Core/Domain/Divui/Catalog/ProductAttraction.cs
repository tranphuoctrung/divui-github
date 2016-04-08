namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attraction mapping
    /// </summary>
    public partial class ProductAttraction : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the attraction identifier
        /// </summary>
        public int AttractionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is featured
        /// </summary>
        public bool IsFeaturedProduct { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the attraction
        /// </summary>
        public virtual Attraction Attraction { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

    }

}
