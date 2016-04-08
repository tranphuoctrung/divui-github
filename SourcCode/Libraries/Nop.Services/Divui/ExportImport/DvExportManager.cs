using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Stores;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Nop.Services.ExportImport
{
    public partial class ExportManager
    {
        #region Fields

        private readonly ICollectionService _collectionService;

        private readonly IAttractionService _attractionService;

        #endregion

        #region Ctor

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreService storeService,
            ICollectionService collectionService,
            IAttractionService attractionService
            )
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._storeService = storeService;
            this._collectionService = collectionService;
            this._attractionService = attractionService;


        }

        #endregion

        /// <summary>
        /// Export collection list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCollectionsToXml()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Collections");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
            WriteCollections(xmlWriter, 0);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export attraction list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        public virtual string ExportAttractionsToXml()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Attractions");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
            WriteAttractions(xmlWriter, 0);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        #region Utilities

        protected virtual void WriteCollections(XmlWriter xmlWriter, int parentCollectionId)
        {
            var collections = _collectionService.GetAllCollectionsByParentCollectionId(parentCollectionId, true);
            if (collections != null && collections.Count > 0)
            {
                foreach (var collection in collections)
                {
                    xmlWriter.WriteStartElement("Collection");
                    xmlWriter.WriteElementString("Id", null, collection.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, collection.Name);
                    xmlWriter.WriteElementString("Description", null, collection.Description);
                    xmlWriter.WriteElementString("CollectionTemplateId", null, collection.CollectionTemplateId.ToString());
                    xmlWriter.WriteElementString("MetaKeywords", null, collection.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, collection.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, collection.MetaTitle);
                    xmlWriter.WriteElementString("SeName", null, collection.GetSeName(0));
                    xmlWriter.WriteElementString("ParentCollectionId", null, collection.ParentCollectionId.ToString());
                    xmlWriter.WriteElementString("PictureId", null, collection.PictureId.ToString());
                    xmlWriter.WriteElementString("PageSize", null, collection.PageSize.ToString());
                    xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, collection.AllowCustomersToSelectPageSize.ToString());
                    xmlWriter.WriteElementString("PageSizeOptions", null, collection.PageSizeOptions);
                    xmlWriter.WriteElementString("PriceRanges", null, collection.PriceRanges);
                    xmlWriter.WriteElementString("ShowOnHomePage", null, collection.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("IncludeInTopMenu", null, collection.IncludeInTopMenu.ToString());
                    xmlWriter.WriteElementString("Published", null, collection.Published.ToString());
                    xmlWriter.WriteElementString("Deleted", null, collection.Deleted.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, collection.DisplayOrder.ToString());
                    xmlWriter.WriteElementString("CreatedOnUtc", null, collection.CreatedOnUtc.ToString());
                    xmlWriter.WriteElementString("UpdatedOnUtc", null, collection.UpdatedOnUtc.ToString());


                    xmlWriter.WriteStartElement("Products");
                    var productCollections = _collectionService.GetProductCollectionsByCollectionId(collection.Id, showHidden: true);
                    foreach (var productCollection in productCollections)
                    {
                        var product = productCollection.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductCollection");
                            xmlWriter.WriteElementString("ProductCollectionId", null, productCollection.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productCollection.ProductId.ToString());
                            xmlWriter.WriteElementString("ProductName", null, product.Name);
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productCollection.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productCollection.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SubCollections");
                    WriteCollections(xmlWriter, collection.Id);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
            }
        }

        protected virtual void WriteAttractions(XmlWriter xmlWriter, int parentAttractionId)
        {
            var attractions = _attractionService.GetAllAttractionsByParentAttractionId(parentAttractionId, true);
            if (attractions != null && attractions.Count > 0)
            {
                foreach (var attraction in attractions)
                {
                    xmlWriter.WriteStartElement("Attraction");
                    xmlWriter.WriteElementString("Id", null, attraction.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, attraction.Name);
                    xmlWriter.WriteElementString("Description", null, attraction.Description);
                    xmlWriter.WriteElementString("AttractionTemplateId", null, attraction.AttractionTemplateId.ToString());
                    xmlWriter.WriteElementString("MetaKeywords", null, attraction.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, attraction.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, attraction.MetaTitle);
                    xmlWriter.WriteElementString("SeName", null, attraction.GetSeName(0));
                    xmlWriter.WriteElementString("ParentAttractionId", null, attraction.ParentAttractionId.ToString());
                    xmlWriter.WriteElementString("PictureId", null, attraction.PictureId.ToString());
                    xmlWriter.WriteElementString("PageSize", null, attraction.PageSize.ToString());
                    xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, attraction.AllowCustomersToSelectPageSize.ToString());
                    xmlWriter.WriteElementString("PageSizeOptions", null, attraction.PageSizeOptions);
                    xmlWriter.WriteElementString("PriceRanges", null, attraction.PriceRanges);
                    xmlWriter.WriteElementString("ShowOnHomePage", null, attraction.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("IncludeInTopMenu", null, attraction.IncludeInTopMenu.ToString());
                    xmlWriter.WriteElementString("Published", null, attraction.Published.ToString());
                    xmlWriter.WriteElementString("Deleted", null, attraction.Deleted.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, attraction.DisplayOrder.ToString());
                    xmlWriter.WriteElementString("CreatedOnUtc", null, attraction.CreatedOnUtc.ToString());
                    xmlWriter.WriteElementString("UpdatedOnUtc", null, attraction.UpdatedOnUtc.ToString());


                    xmlWriter.WriteStartElement("Products");
                    var productAttractions = _attractionService.GetProductAttractionsByAttractionId(attraction.Id, showHidden: true);
                    foreach (var productAttraction in productAttractions)
                    {
                        var product = productAttraction.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductAttraction");
                            xmlWriter.WriteElementString("ProductAttractionId", null, productAttraction.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productAttraction.ProductId.ToString());
                            xmlWriter.WriteElementString("ProductName", null, product.Name);
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productAttraction.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productAttraction.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SubAttractions");
                    WriteAttractions(xmlWriter, attraction.Id);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
            }
        }
        #endregion
    }
}
