using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Customers
{
    public partial class CustomerRole
    {
        private ICollection<PriceSetup> _priceSetups;

        private ICollection<AvailabilitySetup> _availabilitySetups;

        /// <summary>
        /// Gets or sets the collection of AvailabilitySetups
        /// </summary>
        public virtual ICollection<AvailabilitySetup> AvailabilitySetups
        {
            get { return _availabilitySetups ?? (_availabilitySetups = new List<AvailabilitySetup>()); }
            protected set { _availabilitySetups = value; }
        }

        /// <summary>
        /// Gets or sets the collection of PriceSetups
        /// </summary>
        public virtual ICollection<PriceSetup> PriceSetups
        {
            get { return _priceSetups ?? (_priceSetups = new List<PriceSetup>()); }
            protected set { _priceSetups = value; }
        }
    }
}
