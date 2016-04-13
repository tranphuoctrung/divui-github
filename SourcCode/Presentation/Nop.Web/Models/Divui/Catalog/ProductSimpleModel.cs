using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Web.Models.Catalog
{
    public class ProductSimpleModel
    {
        public ProductSimpleModel()
        {
            AvaliableQuantities = new List<SelectListItem>();
            ProductSpecifications = new List<ProductSpecificationModel>();
            ProductPrice = new ProductDetailsModel.ProductPriceModel();
            ProductAttributes = new List<ProductDetailsModel.ProductAttributeModel>();
        }
        public ProductDetailsModel.ProductPriceModel ProductPrice { get; set; }
        public string AgeRangeName { get; set; }
        public int AgeRange { get; set; }
        public string AgeRangeCondition { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }

        public string SeName { get; set; }

        public string Overview { get; set; }

        public bool IsNew { get; set; }

        public bool IsSpecial { get; set; }

        public List<SelectListItem> AvaliableQuantities { get; set; }

        public List<ProductDetailsModel.ProductAttributeModel> ProductAttributes { get; set; }
        public List<ProductSpecificationModel> ProductSpecifications { get; set; }

        public DateTime SelectedDate { get; set; }
    }
}