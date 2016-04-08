using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Divui.Catalog
{
    public class SpecificationAttributeModel : BaseNopEntityModel
    {
        public SpecificationAttributeModel()
        {
            SpecificationAttributeOptions = new List<SpecificationAttributeOptionModel>();
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }


        public List<SpecificationAttributeOptionModel> SpecificationAttributeOptions { get; set; }
    }
}