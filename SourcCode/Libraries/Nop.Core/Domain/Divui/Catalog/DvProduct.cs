﻿using Nop.Core.Domain.Divui.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class Product
    {
        private ICollection<ProductCollection> _productCollections;

        private ICollection<ProductAttraction> _productAttractions;

        private ICollection<ProductOption> _productOptions;

        private ICollection<PriceSetup> _priceSetups;

        private ICollection<AvailabilitySetup> _availabilitySetups;

        public string Overview { get; set; }

        public string HightLight { get; set; }
        public string Condition { get; set; }

        public string GuideToUse { get; set; }

        public string Tip { get; set; }

        public int Type { get; set; }

        public string AgeRangeCondition { get; set; }

        public int AgeRange { get; set; }

        /// <summary>
        /// Gets or sets the product type
        /// </summary>
        public DvAgeRangeType AgeRangeType
        {
            get
            {
                return (DvAgeRangeType)this.AgeRange;
            }
            set
            {
                this.AgeRange = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of ProductCollection
        /// </summary>
        public virtual ICollection<ProductCollection> ProductCollections
        {
            get { return _productCollections ?? (_productCollections = new List<ProductCollection>()); }
            protected set { _productCollections = value; }
        }

        /// <summary>
        /// Gets or sets the collection of ProductAttraction
        /// </summary>
        public virtual ICollection<ProductAttraction> ProductAttractions
        {
            get { return _productAttractions ?? (_productAttractions = new List<ProductAttraction>()); }
            protected set { _productAttractions = value; }
        }

        /// <summary>
        /// Gets or sets the collection of ProductOptions
        /// </summary>
        public virtual ICollection<ProductOption> ProductOptions
        {
            get { return _productOptions ?? (_productOptions = new List<ProductOption>()); }
            protected set { _productOptions = value; }
        }

        /// <summary>
        /// Gets or sets the collection of PriceSetups
        /// </summary>
        public virtual ICollection<PriceSetup> PriceSetups
        {
            get { return _priceSetups ?? (_priceSetups = new List<PriceSetup>()); }
            protected set { _priceSetups = value; }
        }

        /// <summary>
        /// Gets or sets the collection of AvailabilitySetups
        /// </summary>
        public virtual ICollection<AvailabilitySetup> AvailabilitySetups
        {
            get { return _availabilitySetups ?? (_availabilitySetups = new List<AvailabilitySetup>()); }
            protected set { _availabilitySetups = value; }
        }
    }
}