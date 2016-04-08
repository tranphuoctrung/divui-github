using Nop.Core;
using Nop.Core.Domain.Divui.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Divui.Catalog
{
    public partial interface IProductOptionService
    {
        /// <summary>
        /// Delete productOption
        /// </summary>
        /// <param name="productOption">DvGroupProduct</param>
        void DeleteProductOption(ProductOption productOption);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="productOptionName">DvGroupProduct name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        IPagedList<ProductOption> GetAllProductOptions(string productOptionName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, int productId = 0, bool showHidden = false);
        
        /// <summary>
        /// Gets a productOption
        /// </summary>
        /// <param name="productOptionId">DvGroupProduct identifier</param>
        /// <returns>DvGroupProduct</returns>
        ProductOption GetProductOptionById(int productOptionId);

        /// <summary>
        /// Inserts productOption
        /// </summary>
        /// <param name="productOption">DvGroupProduct</param>
        void InsertProductOption(ProductOption productOption);

        /// <summary>
        /// Updates the productOption
        /// </summary>
        /// <param name="productOption">DvGroupProduct</param>
        void UpdateProductOption(ProductOption productOption);
        
    }
}
