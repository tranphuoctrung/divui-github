using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Core.Domain.Catalog;

namespace Nop.Web.Models.Catalog
{
    public partial class SearchModel
    {
        public SearchModel()
        {
            this.PagingFilteringContext = new CatalogPagingFilteringModel();
            this.Products = new List<ProductOverviewModel>();

            this.AvailableCategories = new List<CategorySimpleModel>();
            this.AvailableManufacturers = new List<SelectListItem>();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableCities = new List<SelectListItem>();
            this.AvailableCollections = new List<CategorySimpleModel>();
            this.AvailableAttractions = new List<CategorySimpleModel>();
            SpecificationAttributes = new List<SpecificationAttribute>();
            AlreadyFilteredSpecOptionIds = new List<int>();
        }

        public List<SelectListItem> AvailableCountries { get; set; }
        public List<SelectListItem> AvailableCities { get; set; }

        public List<CategorySimpleModel> AvailableCategories { get; set; }

        public int collectionId { get; set; }
        public List<CategorySimpleModel> AvailableCollections { get; set; }

        public int attractionId { get; set; }
        public List<CategorySimpleModel> AvailableAttractions { get; set; }

        public int countryId { get; set; }

        public int cityId { get; set; }

        public List<SpecificationAttribute> SpecificationAttributes { get; set; }

        public List<int> AlreadyFilteredSpecOptionIds { get; set; }

        public int categoryId { get; set; }

        public string CityOrCountryName { get; set; }

        public string AttractionName { get; set; }
        public string CategoryName { get; set; }
        public string CollectionName { get; set; }

        #region Nested classes

        //public partial class CategoryModel 
        //{
        //    public int ParentCategoryId { get; set; }
        //    public string Name { get; set; }
        //}
        #endregion
    }
}