using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount
    /// </summary>
    public partial class Discount : BaseEntity
    {
        private ICollection<Collection> _appliedToCollections;
        private ICollection<Attraction> _appliedToAttractions;

        /// <summary>
        /// Gets or sets the collections
        /// </summary>
        public virtual ICollection<Collection> AppliedToCollections
        {
            get { return _appliedToCollections ?? (_appliedToCollections = new List<Collection>()); }
            protected set { _appliedToCollections = value; }
        }

        /// <summary>
        /// Gets or sets the attractions
        /// </summary>
        public virtual ICollection<Attraction> AppliedToAttractions
        {
            get { return _appliedToAttractions ?? (_appliedToAttractions = new List<Attraction>()); }
            protected set { _appliedToAttractions = value; }
        }
    }
}
