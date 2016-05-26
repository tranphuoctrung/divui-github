using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
namespace Nop.Services.Catalog
{
    public partial class ProductService
    {
        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="markedAsNewOnly">A values indicating whether to load only products marked as "new"; "false" to load all records; "true" to load "marked as new" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>Products</returns>
        public virtual IPagedList<Product> DvSearchProducts(
            out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = 2147483647,  //Int32.MaxValue
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();

            //search by keyword
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages().Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();

            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //stored procedures are enabled and supported by the database. 
                //It's much faster than the LINQ implementation below 

                #region Use stored procedure

                //pass category identifiers as comma-delimited string
                string commaSeparatedCategoryIds = categoryIds == null ? "" : string.Join(",", categoryIds);


                //pass customer role identifiers as comma-delimited string
                string commaSeparatedAllowedCustomerRoleIds = string.Join(",", allowedCustomerRolesIds);


                //pass specification identifiers as comma-delimited string
                string commaSeparatedSpecIds = "";
                if (filteredSpecs != null)
                {
                    ((List<int>)filteredSpecs).Sort();
                    commaSeparatedSpecIds = string.Join(",", filteredSpecs);
                }

                //some databases don't support int.MaxValue
                if (pageSize == int.MaxValue)
                    pageSize = int.MaxValue - 1;

                //prepare parameters
                var pCategoryIds = _dataProvider.GetParameter();
                pCategoryIds.ParameterName = "CategoryIds";
                pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
                pCategoryIds.DbType = DbType.String;

                var pManufacturerId = _dataProvider.GetParameter();
                pManufacturerId.ParameterName = "ManufacturerId";
                pManufacturerId.Value = manufacturerId;
                pManufacturerId.DbType = DbType.Int32;

                var pStoreId = _dataProvider.GetParameter();
                pStoreId.ParameterName = "StoreId";
                pStoreId.Value = !_catalogSettings.IgnoreStoreLimitations ? storeId : 0;
                pStoreId.DbType = DbType.Int32;

                var pVendorId = _dataProvider.GetParameter();
                pVendorId.ParameterName = "VendorId";
                pVendorId.Value = vendorId;
                pVendorId.DbType = DbType.Int32;

                var pWarehouseId = _dataProvider.GetParameter();
                pWarehouseId.ParameterName = "WarehouseId";
                pWarehouseId.Value = warehouseId;
                pWarehouseId.DbType = DbType.Int32;

                var pProductTypeId = _dataProvider.GetParameter();
                pProductTypeId.ParameterName = "ProductTypeId";
                pProductTypeId.Value = productType.HasValue ? (object)productType.Value : DBNull.Value;
                pProductTypeId.DbType = DbType.Int32;

                var pVisibleIndividuallyOnly = _dataProvider.GetParameter();
                pVisibleIndividuallyOnly.ParameterName = "VisibleIndividuallyOnly";
                pVisibleIndividuallyOnly.Value = visibleIndividuallyOnly;
                pVisibleIndividuallyOnly.DbType = DbType.Int32;

                var pMarkedAsNewOnly = _dataProvider.GetParameter();
                pMarkedAsNewOnly.ParameterName = "MarkedAsNewOnly";
                pMarkedAsNewOnly.Value = markedAsNewOnly;
                pMarkedAsNewOnly.DbType = DbType.Int32;

                var pProductTagId = _dataProvider.GetParameter();
                pProductTagId.ParameterName = "ProductTagId";
                pProductTagId.Value = productTagId;
                pProductTagId.DbType = DbType.Int32;

                var pFeaturedProducts = _dataProvider.GetParameter();
                pFeaturedProducts.ParameterName = "FeaturedProducts";
                pFeaturedProducts.Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value;
                pFeaturedProducts.DbType = DbType.Boolean;

                var pPriceMin = _dataProvider.GetParameter();
                pPriceMin.ParameterName = "PriceMin";
                pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
                pPriceMin.DbType = DbType.Decimal;

                var pPriceMax = _dataProvider.GetParameter();
                pPriceMax.ParameterName = "PriceMax";
                pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
                pPriceMax.DbType = DbType.Decimal;

                var pKeywords = _dataProvider.GetParameter();
                pKeywords.ParameterName = "Keywords";
                pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
                pKeywords.DbType = DbType.String;

                var pSearchDescriptions = _dataProvider.GetParameter();
                pSearchDescriptions.ParameterName = "SearchDescriptions";
                pSearchDescriptions.Value = searchDescriptions;
                pSearchDescriptions.DbType = DbType.Boolean;

                var pSearchSku = _dataProvider.GetParameter();
                pSearchSku.ParameterName = "SearchSku";
                pSearchSku.Value = searchSku;
                pSearchSku.DbType = DbType.Boolean;

                var pSearchProductTags = _dataProvider.GetParameter();
                pSearchProductTags.ParameterName = "SearchProductTags";
                pSearchProductTags.Value = searchProductTags;
                pSearchProductTags.DbType = DbType.Boolean;

                var pUseFullTextSearch = _dataProvider.GetParameter();
                pUseFullTextSearch.ParameterName = "UseFullTextSearch";
                pUseFullTextSearch.Value = _commonSettings.UseFullTextSearch;
                pUseFullTextSearch.DbType = DbType.Boolean;

                var pFullTextMode = _dataProvider.GetParameter();
                pFullTextMode.ParameterName = "FullTextMode";
                pFullTextMode.Value = (int)_commonSettings.FullTextMode;
                pFullTextMode.DbType = DbType.Int32;

                var pFilteredSpecs = _dataProvider.GetParameter();
                pFilteredSpecs.ParameterName = "FilteredSpecs";
                pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
                pFilteredSpecs.DbType = DbType.String;

                var pLanguageId = _dataProvider.GetParameter();
                pLanguageId.ParameterName = "LanguageId";
                pLanguageId.Value = searchLocalizedValue ? languageId : 0;
                pLanguageId.DbType = DbType.Int32;

                var pOrderBy = _dataProvider.GetParameter();
                pOrderBy.ParameterName = "OrderBy";
                pOrderBy.Value = (int)orderBy;
                pOrderBy.DbType = DbType.Int32;

                var pAllowedCustomerRoleIds = _dataProvider.GetParameter();
                pAllowedCustomerRoleIds.ParameterName = "AllowedCustomerRoleIds";
                pAllowedCustomerRoleIds.Value = commaSeparatedAllowedCustomerRoleIds;
                pAllowedCustomerRoleIds.DbType = DbType.String;

                var pPageIndex = _dataProvider.GetParameter();
                pPageIndex.ParameterName = "PageIndex";
                pPageIndex.Value = pageIndex;
                pPageIndex.DbType = DbType.Int32;

                var pPageSize = _dataProvider.GetParameter();
                pPageSize.ParameterName = "PageSize";
                pPageSize.Value = pageSize;
                pPageSize.DbType = DbType.Int32;

                var pShowHidden = _dataProvider.GetParameter();
                pShowHidden.ParameterName = "ShowHidden";
                pShowHidden.Value = showHidden;
                pShowHidden.DbType = DbType.Boolean;

                var pOverridePublished = _dataProvider.GetParameter();
                pOverridePublished.ParameterName = "OverridePublished";
                pOverridePublished.Value = overridePublished != null ? (object)overridePublished.Value : DBNull.Value;
                pOverridePublished.DbType = DbType.Boolean;

                var pLoadFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pLoadFilterableSpecificationAttributeOptionIds.ParameterName = "LoadFilterableSpecificationAttributeOptionIds";
                pLoadFilterableSpecificationAttributeOptionIds.Value = loadFilterableSpecificationAttributeOptionIds;
                pLoadFilterableSpecificationAttributeOptionIds.DbType = DbType.Boolean;

                var pFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
                pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
                pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
                pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

                var pTotalRecords = _dataProvider.GetParameter();
                pTotalRecords.ParameterName = "TotalRecords";
                pTotalRecords.Direction = ParameterDirection.Output;
                pTotalRecords.DbType = DbType.Int32;

                //invoke stored procedure
                var products = _dbContext.ExecuteStoredProcedureList<Product>(
                    "DvProductLoadAllPaged",
                    pCategoryIds,
                    pManufacturerId,
                    pStoreId,
                    pVendorId,
                    pWarehouseId,
                    pProductTypeId,
                    pVisibleIndividuallyOnly,
                    pMarkedAsNewOnly,
                    pProductTagId,
                    pFeaturedProducts,
                    pPriceMin,
                    pPriceMax,
                    pKeywords,
                    pSearchDescriptions,
                    pSearchSku,
                    pSearchProductTags,
                    pUseFullTextSearch,
                    pFullTextMode,
                    pFilteredSpecs,
                    pLanguageId,
                    pOrderBy,
                    pAllowedCustomerRoleIds,
                    pPageIndex,
                    pPageSize,
                    pShowHidden,
                    pOverridePublished,
                    pLoadFilterableSpecificationAttributeOptionIds,
                    pFilterableSpecificationAttributeOptionIds,
                    pTotalRecords);
                //get filterable specification attribute option identifier
                string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
                if (loadFilterableSpecificationAttributeOptionIds &&
                    !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
                {
                    filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                       .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => Convert.ToInt32(x.Trim()))
                       .ToList();
                }
                //return products
                int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
                return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

                #endregion
            }
            else
            {
                //stored procedures aren't supported. Use LINQ

                #region Search products

                //products
                var query = _productRepository.Table;
                query = query.Where(p => !p.Deleted);
                if (!overridePublished.HasValue)
                {
                    //process according to "showHidden"
                    if (!showHidden)
                    {
                        query = query.Where(p => p.Published);
                    }
                }
                else if (overridePublished.Value)
                {
                    //published only
                    query = query.Where(p => p.Published);
                }
                else if (!overridePublished.Value)
                {
                    //unpublished only
                    query = query.Where(p => !p.Published);
                }
                if (visibleIndividuallyOnly)
                {
                    query = query.Where(p => p.VisibleIndividually);
                }
                //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                //That's why we pass the date value
                var nowUtc = DateTime.UtcNow;
                if (markedAsNewOnly)
                {
                    query = query.Where(p => p.MarkAsNew);
                    query = query.Where(p =>
                        (!p.MarkAsNewStartDateTimeUtc.HasValue || p.MarkAsNewStartDateTimeUtc.Value < nowUtc) &&
                        (!p.MarkAsNewEndDateTimeUtc.HasValue || p.MarkAsNewEndDateTimeUtc.Value > nowUtc));
                }
                if (productType.HasValue)
                {
                    var productTypeId = (int)productType.Value;
                    query = query.Where(p => p.ProductTypeId == productTypeId);
                }

                if (priceMin.HasValue)
                {
                    //min price
                    query = query.Where(p =>
                                        //special price (specified price and valid date range)
                                        ((p.SpecialPrice.HasValue &&
                                          ((!p.SpecialPriceStartDateTimeUtc.HasValue ||
                                            p.SpecialPriceStartDateTimeUtc.Value < nowUtc) &&
                                           (!p.SpecialPriceEndDateTimeUtc.HasValue ||
                                            p.SpecialPriceEndDateTimeUtc.Value > nowUtc))) &&
                                         (p.SpecialPrice >= priceMin.Value))
                                        ||
                                        //regular price (price isn't specified or date range isn't valid)
                                        ((!p.SpecialPrice.HasValue ||
                                          ((p.SpecialPriceStartDateTimeUtc.HasValue &&
                                            p.SpecialPriceStartDateTimeUtc.Value > nowUtc) ||
                                           (p.SpecialPriceEndDateTimeUtc.HasValue &&
                                            p.SpecialPriceEndDateTimeUtc.Value < nowUtc))) &&
                                         (p.Price >= priceMin.Value)));
                }
                if (priceMax.HasValue)
                {
                    //max price
                    query = query.Where(p =>
                                        //special price (specified price and valid date range)
                                        ((p.SpecialPrice.HasValue &&
                                          ((!p.SpecialPriceStartDateTimeUtc.HasValue ||
                                            p.SpecialPriceStartDateTimeUtc.Value < nowUtc) &&
                                           (!p.SpecialPriceEndDateTimeUtc.HasValue ||
                                            p.SpecialPriceEndDateTimeUtc.Value > nowUtc))) &&
                                         (p.SpecialPrice <= priceMax.Value))
                                        ||
                                        //regular price (price isn't specified or date range isn't valid)
                                        ((!p.SpecialPrice.HasValue ||
                                          ((p.SpecialPriceStartDateTimeUtc.HasValue &&
                                            p.SpecialPriceStartDateTimeUtc.Value > nowUtc) ||
                                           (p.SpecialPriceEndDateTimeUtc.HasValue &&
                                            p.SpecialPriceEndDateTimeUtc.Value < nowUtc))) &&
                                         (p.Price <= priceMax.Value)));
                }
                if (!showHidden)
                {
                    //available dates
                    query = query.Where(p =>
                        (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < nowUtc) &&
                        (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > nowUtc));
                }

                //searching by keyword
                if (!String.IsNullOrWhiteSpace(keywords))
                {
                    query = from p in query
                            join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into p_lp
                            from lp in p_lp.DefaultIfEmpty()
                            from pt in p.ProductTags.DefaultIfEmpty()
                            where (p.Name.Contains(keywords)) ||
                                  (searchDescriptions && p.ShortDescription.Contains(keywords)) ||
                                  (searchDescriptions && p.FullDescription.Contains(keywords)) ||
                                  (searchProductTags && pt.Name.Contains(keywords)) ||
                                  //localized values
                                  (searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.Contains(keywords)) ||
                                  (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue.Contains(keywords)) ||
                                  (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "FullDescription" && lp.LocaleValue.Contains(keywords))
                            select p;
                }

                if (!showHidden && !_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    query = from p in query
                            join acl in _aclRepository.Table
                            on new { c1 = p.Id, c2 = "Product" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into p_acl
                            from acl in p_acl.DefaultIfEmpty()
                            where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                            select p;
                }

                if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    query = from p in query
                            join sm in _storeMappingRepository.Table
                            on new { c1 = p.Id, c2 = "Product" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into p_sm
                            from sm in p_sm.DefaultIfEmpty()
                            where !p.LimitedToStores || storeId == sm.StoreId
                            select p;
                }

                //search by specs
                if (filteredSpecs != null && filteredSpecs.Count > 0)
                {
                    query = from p in query
                            where !filteredSpecs
                                       .Except(
                                           p.ProductSpecificationAttributes.Where(psa => psa.AllowFiltering).Select(
                                               psa => psa.SpecificationAttributeOptionId))
                                       .Any()
                            select p;
                }

                //category filtering
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    query = from p in query
                            from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
                            where (!featuredProducts.HasValue || featuredProducts.Value == pc.IsFeaturedProduct)
                            select p;
                }

                //manufacturer filtering
                if (manufacturerId > 0)
                {
                    query = from p in query
                            from pm in p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                            where (!featuredProducts.HasValue || featuredProducts.Value == pm.IsFeaturedProduct)
                            select p;
                }

                //vendor filtering
                if (vendorId > 0)
                {
                    query = query.Where(p => p.VendorId == vendorId);
                }

                //warehouse filtering
                if (warehouseId > 0)
                {
                    var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                    query = query.Where(p =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (p.ManageInventoryMethodId == manageStockInventoryMethodId &&
                         p.UseMultipleWarehouses &&
                         p.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((p.ManageInventoryMethodId != manageStockInventoryMethodId ||
                          !p.UseMultipleWarehouses) &&
                          p.WarehouseId == warehouseId));
                }

                //related products filtering
                //if (relatedToProductId > 0)
                //{
                //    query = from p in query
                //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
                //            where (relatedToProductId == rp.ProductId1)
                //            select p;
                //}

                //tag filtering
                if (productTagId > 0)
                {
                    query = from p in query
                            from pt in p.ProductTags.Where(pt => pt.Id == productTagId)
                            select p;
                }

                //only distinct products (group by ID)
                //if we use standard Distinct() method, then all fields will be compared (low performance)
                //it'll not work in SQL Server Compact when searching products by a keyword)
                query = from p in query
                        group p by p.Id
                        into pGroup
                        orderby pGroup.Key
                        select pGroup.FirstOrDefault();

                //sort products
                if (orderBy == ProductSortingEnum.Position && categoryIds != null && categoryIds.Count > 0)
                {
                    //category position
                    var firstCategoryId = categoryIds[0];
                    query = query.OrderBy(p => p.ProductCategories.FirstOrDefault(pc => pc.CategoryId == firstCategoryId).DisplayOrder);
                }
                else if (orderBy == ProductSortingEnum.Position && manufacturerId > 0)
                {
                    //manufacturer position
                    query =
                        query.OrderBy(p => p.ProductManufacturers.FirstOrDefault(pm => pm.ManufacturerId == manufacturerId).DisplayOrder);
                }
                else if (orderBy == ProductSortingEnum.Position)
                {
                    //otherwise sort by name
                    query = query.OrderBy(p => p.Name);
                }
                else if (orderBy == ProductSortingEnum.NameAsc)
                {
                    //Name: A to Z
                    query = query.OrderBy(p => p.Name);
                }
                else if (orderBy == ProductSortingEnum.NameDesc)
                {
                    //Name: Z to A
                    query = query.OrderByDescending(p => p.Name);
                }
                else if (orderBy == ProductSortingEnum.PriceAsc)
                {
                    //Price: Low to High
                    query = query.OrderBy(p => p.Price);
                }
                else if (orderBy == ProductSortingEnum.PriceDesc)
                {
                    //Price: High to Low
                    query = query.OrderByDescending(p => p.Price);
                }
                else if (orderBy == ProductSortingEnum.CreatedOn)
                {
                    //creation date
                    query = query.OrderByDescending(p => p.CreatedOnUtc);
                }
                else if (orderBy == ProductSortingEnum.ReviewDesc)
                {
                    //product review
                    query = query.OrderByDescending(p => p.ApprovedRatingSum);
                }
                else if (orderBy == ProductSortingEnum.ReviewAsc)
                {
                    //product review
                    query = query.OrderBy(p => p.ApprovedRatingSum);
                }
                else
                {
                    //actually this code is not reachable
                    query = query.OrderBy(p => p.Name);
                }

                var products = new PagedList<Product>(query, pageIndex, pageSize);

                //get filterable specification attribute option identifier
                if (loadFilterableSpecificationAttributeOptionIds)
                {
                    var querySpecs = from p in query
                                     join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                                     where psa.AllowFiltering
                                     select psa.SpecificationAttributeOptionId;
                    //only distinct attributes
                    filterableSpecificationAttributeOptionIds = querySpecs
                        .Distinct()
                        .ToList();
                }

                //return products
                return products;

                #endregion
            }
        }

    }
}
