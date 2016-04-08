using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public class ProductOptionModel
    {
        public ProductOptionModel()
        {
            ProductSimples = new List<ProductSimpleModel>();
        }
        public string Name { get; set; }

        public int ProductOptionId { get; set; }

        public string Description { get; set; }

        public List<ProductSimpleModel> ProductSimples { get; set; }

        

    }
}