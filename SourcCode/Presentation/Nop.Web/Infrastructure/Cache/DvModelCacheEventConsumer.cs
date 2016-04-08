using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;

namespace Nop.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer
    {
        /// <summary>
        /// Key for ProductSpecificationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// </remarks>
        public const string CATEGORY_SPECS_MODEL_KEY = "Nop.pres.category.specs-{0}-{1}";
        public const string CATEGORY_SPECS_PATTERN_KEY = "Nop.pres.category.specs";

        public const string CATEGORY_NEW_HOMEPAGE_KEY = "Nop.pres.category.homepage-{0}-{1}-{2}-{3}-{4}";

        /// <summary>
        /// Key for collection on the search page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SEARCH_COLLECTIONS_MODEL_KEY = "Nop.pres.search.collection-{0}-{1}-{2}";
        public const string SEARCH_COLLECTIONS_PATTERN_KEY = "Nop.pres.search.collection";

        /// <summary>
        /// Key for CategoryNavigationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string COLLECTION_NAVIGATION_MODEL_KEY = "Nop.pres.collection.navigation-{0}-{1}-{2}";
        public const string COLLECTION_NAVIGATION_PATTERN_KEY = "Nop.pres.collection.navigation";

        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string COLLECTION_MENU_MODEL_KEY = "Nop.pres.collection.menu-{0}-{1}-{2}";
        public const string COLLECTION_MENU_PATTERN_KEY = "Nop.pres.collection.menu";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : comma separated list of customer roles
        /// {1} : current store ID
        /// {2} : collection ID
        /// </remarks>
        public const string COLLECTION_NUMBER_OF_PRODUCTS_MODEL_KEY = "Nop.pres.collection.numberofproducts-{0}-{1}-{2}";
        public const string COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY = "Nop.pres.collection.numberofproducts";

        /// <summary>
        /// Key for caching of a value indicating whether a collection has featured products
        /// </summary>
        /// <remarks>
        /// {0} : collection id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string COLLECTION_HAS_FEATURED_PRODUCTS_KEY = "Nop.pres.collection.hasfeaturedproducts-{0}-{1}-{2}";
        public const string COLLECTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "Nop.pres.collection.hasfeaturedproducts";

        /// <summary>
        /// Key for caching of collection breadcrumb
        /// </summary>
        /// <remarks>
        /// {0} : collection id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// </remarks>
        public const string COLLECTION_BREADCRUMB_KEY = "Nop.pres.collection.breadcrumb-{0}-{1}-{2}-{3}";
        public const string COLLECTION_BREADCRUMB_PATTERN_KEY = "Nop.pres.collection.breadcrumb";

        /// <summary>
        /// Key for caching of subcollection of certain collection
        /// </summary>
        /// <remarks>
        /// {0} : collection id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// {4} : is connection SSL secured (included in a collection picture URL)
        /// </remarks>
        public const string COLLECTION_SUBCOLLECTIONS_KEY = "Nop.pres.collection.subcollection-{0}-{1}-{2}-{3}-{4}";
        public const string COLLECTION_SUBCOLLECTIONS_PATTERN_KEY = "Nop.pres.collection.subcollection";

        /// <summary>
        /// Key for caching of collection displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : roles of the current user
        /// {1} : current store ID
        /// {2} : language ID
        /// {3} : is connection SSL secured (included in a collection picture URL)
        /// </remarks>
        public const string COLLECTION_HOMEPAGE_KEY = "Nop.pres.collection.homepage-{0}-{1}-{2}-{3}";
        public const string COLLECTION_HOMEPAGE_PATTERN_KEY = "Nop.pres.collection.homepage";

        /// <summary>
        /// Key for GetChildCategoryIds method results caching
        /// </summary>
        /// <remarks>
        /// {0} : parent collection id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string COLLECTION_CHILD_IDENTIFIERS_MODEL_KEY = "Nop.pres.collection.childidentifiers-{0}-{1}-{2}";
        public const string COLLECTION_CHILD_IDENTIFIERS_PATTERN_KEY = "Nop.pres.collection.childidentifiers";


        /// <summary>
        /// Key for collection picture caching
        /// </summary>
        /// <remarks>
        /// {0} : attraction id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized collection name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string COLLECTION_PICTURE_MODEL_KEY = "Nop.pres.collection.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string COLLECTION_PICTURE_PATTERN_KEY = "Nop.pres.collection.picture";

        /// <summary>
        /// Key for attraction on the search page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SEARCH_ATTRACTIONS_MODEL_KEY = "Nop.pres.search.attraction-{0}-{1}-{2}";
        public const string SEARCH_ATTRACTIONS_PATTERN_KEY = "Nop.pres.search.attraction";

        /// <summary>
        /// Key for CategoryNavigationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string ATTRACTION_NAVIGATION_MODEL_KEY = "Nop.pres.attraction.navigation-{0}-{1}-{2}";
        public const string ATTRACTION_NAVIGATION_PATTERN_KEY = "Nop.pres.attraction.navigation";

        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string ATTRACTION_MENU_MODEL_KEY = "Nop.pres.attraction.menu-{0}-{1}-{2}";
        public const string ATTRACTION_MENU_PATTERN_KEY = "Nop.pres.attraction.menu";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : comma separated list of customer roles
        /// {1} : current store ID
        /// {2} : attraction ID
        /// </remarks>
        public const string ATTRACTION_NUMBER_OF_PRODUCTS_MODEL_KEY = "Nop.pres.attraction.numberofproducts-{0}-{1}-{2}";
        public const string ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY = "Nop.pres.attraction.numberofproducts";

        /// <summary>
        /// Key for caching of a value indicating whether a attraction has featured products
        /// </summary>
        /// <remarks>
        /// {0} : attraction id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string ATTRACTION_HAS_FEATURED_PRODUCTS_KEY = "Nop.pres.attraction.hasfeaturedproducts-{0}-{1}-{2}";
        public const string ATTRACTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "Nop.pres.attraction.hasfeaturedproducts";

        /// <summary>
        /// Key for caching of attraction breadcrumb
        /// </summary>
        /// <remarks>
        /// {0} : attraction id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// </remarks>
        public const string ATTRACTION_BREADCRUMB_KEY = "Nop.pres.attraction.breadcrumb-{0}-{1}-{2}-{3}";
        public const string ATTRACTION_BREADCRUMB_PATTERN_KEY = "Nop.pres.attraction.breadcrumb";

        /// <summary>
        /// Key for caching of subattraction of certain attraction
        /// </summary>
        /// <remarks>
        /// {0} : attraction id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// {4} : is connection SSL secured (included in a attraction picture URL)
        /// </remarks>
        public const string ATTRACTION_SUBATTRACTIONS_KEY = "Nop.pres.attraction.subattraction-{0}-{1}-{2}-{3}-{4}";
        public const string ATTRACTION_SUBATTRACTIONS_PATTERN_KEY = "Nop.pres.attraction.subattraction";

        /// <summary>
        /// Key for caching of attraction displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : roles of the current user
        /// {1} : current store ID
        /// {2} : language ID
        /// {3} : is connection SSL secured (included in a attraction picture URL)
        /// </remarks>
        public const string ATTRACTION_HOMEPAGE_KEY = "Nop.pres.attraction.homepage-{0}-{1}-{2}-{3}";
        public const string ATTRACTION_HOMEPAGE_PATTERN_KEY = "Nop.pres.attraction.homepage";

        /// <summary>
        /// Key for GetChildCategoryIds method results caching
        /// </summary>
        /// <remarks>
        /// {0} : parent attraction id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string ATTRACTION_CHILD_IDENTIFIERS_MODEL_KEY = "Nop.pres.attraction.childidentifiers-{0}-{1}-{2}";
        public const string ATTRACTION_CHILD_IDENTIFIERS_PATTERN_KEY = "Nop.pres.attraction.childidentifiers";

        /// <summary>
        /// Key for attraction picture caching
        /// </summary>
        /// <remarks>
        /// {0} : attraction id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized attraction name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string ATTRACTION_PICTURE_MODEL_KEY = "Nop.pres.attraction.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string ATTRACTION_PICTURE_PATTERN_KEY = "Nop.pres.attraction.picture";



        //languages
        public void HandleEvent(EntityInserted<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
        }

        public void HandleEvent(EntityUpdated<Setting> eventMessage)
        {
            //clear models which depend on settings
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY); //depends on CatalogSettings.NumberOfProductTags
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY); //depends on CatalogSettings.ManufacturersBlockItemsToDisplay
            _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY); //depends on VendorSettings.VendorBlockItemsToDisplay
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY); //depends on CatalogSettings.ShowCategoryProductNumber and CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            _cacheManager.RemoveByPattern(CATEGORY_MENU_PATTERN_KEY); //depends on CatalogSettings.ShowCategoryProductNumber and CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY); //depends on CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY); //depends on CatalogSettings.NumberOfBestsellersOnHomepage
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY); //depends on CatalogSettings.ProductsAlsoPurchasedNumber
            _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY); //depends on BlogSettings.NumberOfTags
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY); //depends on NewsSettings.MainPageNewsCount
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY); //depends on distinct sitemap settings
            _cacheManager.RemoveByPattern(WIDGET_PATTERN_KEY); //depends on WidgetSettings and certain settings of widgets

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_MENU_PATTERN_KEY);


            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_MENU_PATTERN_KEY);

        }

        //categories
        public void HandleEvent(EntityInserted<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);

            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_SUBCOLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HOMEPAGE_PATTERN_KEY);


            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_SUBATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HOMEPAGE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_SUBCOLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HOMEPAGE_PATTERN_KEY);


            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_SUBATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HOMEPAGE_PATTERN_KEY);

        }
        public void HandleEvent(EntityDeleted<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_SUBCOLLECTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HOMEPAGE_PATTERN_KEY);


            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_CHILD_IDENTIFIERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_SUBATTRACTIONS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HOMEPAGE_PATTERN_KEY);
        }

        //product categories
        public void HandleEvent(EntityInserted<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY);


        }
        public void HandleEvent(EntityUpdated<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_MENU_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }

        //Pictures
        public void HandleEvent(EntityInserted<Picture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(VENDOR_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_SUBCOLLECTIONS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_SUBATTRACTIONS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Picture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(VENDOR_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_SUBCOLLECTIONS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_SUBATTRACTIONS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Picture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(VENDOR_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);

            _cacheManager.RemoveByPattern(COLLECTION_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(COLLECTION_SUBCOLLECTIONS_PATTERN_KEY);

            _cacheManager.RemoveByPattern(ATTRACTION_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ATTRACTION_SUBATTRACTIONS_PATTERN_KEY);
        }


        


    }
}