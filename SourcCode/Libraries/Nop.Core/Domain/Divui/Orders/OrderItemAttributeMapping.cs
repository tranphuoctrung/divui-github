using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute mapping
    /// </summary>
    public partial class OrderItemAttributeMapping : BaseEntity, ILocalizedEntity
    {
        //private ICollection<ProductAttributeValue> _productAttributeValues;

        /// <summary>
        /// Gets or sets the OrderItem identifier
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the product attribute identifier
        /// </summary>
        public int ProductAttributeId { get; set; }



        /// <summary>
        /// Gets or sets a value indicating whether the entity is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute control type identifier
        /// </summary>
        public int AttributeControlTypeId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        //validation fields

        /// <summary>
        /// Gets or sets the default value (for textbox and multiline textbox)
        /// </summary>
        public string Value { get; set; }


        /// <summary>
        /// Gets the attribute control type
        /// </summary>
        public AttributeControlType AttributeControlType
        {
            get
            {
                return (AttributeControlType)this.AttributeControlTypeId;
            }
            set
            {
                this.AttributeControlTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets the product attribute
        /// </summary>
        public virtual ProductAttribute ProductAttribute { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual OrderItem OrderItem { get; set; }

        ///// <summary>
        ///// Gets the product attribute values
        ///// </summary>
        //public virtual ICollection<ProductAttributeValue> ProductAttributeValues
        //{
        //    get { return _productAttributeValues ?? (_productAttributeValues = new List<ProductAttributeValue>()); }
        //    protected set { _productAttributeValues = value; }
        //}

    }

}
