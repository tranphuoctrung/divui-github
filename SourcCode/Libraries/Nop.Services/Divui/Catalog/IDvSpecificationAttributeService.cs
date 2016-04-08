using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Catalog
{
    public partial interface ISpecificationAttributeService
    {

        #region Category specification attribute

        /// <summary>
        /// Deletes a category specification attribute mapping
        /// </summary>
        /// <param name="categorySpecificationAttribute">Category specification attribute</param>
        void DeleteCategorySpecificationAttribute(CategorySpecificationAttribute categorySpecificationAttribute);

        /// <summary>
        /// Gets a category specification attribute mapping collection
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="specificationAttributeOptionId">Specification attribute option identifier; 0 to load all records</param>
        /// <param name="allowFiltering">0 to load attributes with AllowFiltering set to false, 1 to load attributes with AllowFiltering set to true, null to load all attributes</param>
        /// <param name="showOnCategoryPage">0 to load attributes with ShowOnCategoryPage set to false, 1 to load attributes with ShowOnCategoryPage set to true, null to load all attributes</param>
        /// <returns>Category specification attribute mapping collection</returns>
        IList<CategorySpecificationAttribute> GetCategorySpecificationAttributes(int categoryId = 0,
            int specificationAttributeOptionId = 0, bool? allowFiltering = null, bool? showOnCategoryPage = null);

        /// <summary>
        /// Gets a category specification attribute mapping 
        /// </summary>
        /// <param name="categorySpecificationAttributeId">Category specification attribute mapping identifier</param>
        /// <returns>Category specification attribute mapping</returns>
        CategorySpecificationAttribute GetCategorySpecificationAttributeById(int categorySpecificationAttributeId);

        /// <summary>
        /// Inserts a category specification attribute mapping
        /// </summary>
        /// <param name="categorySpecificationAttribute">Category specification attribute mapping</param>
        void InsertCategorySpecificationAttribute(CategorySpecificationAttribute categorySpecificationAttribute);

        /// <summary>
        /// Updates the category specification attribute mapping
        /// </summary>
        /// <param name="categorySpecificationAttribute">Category specification attribute mapping</param>
        void UpdateCategorySpecificationAttribute(CategorySpecificationAttribute categorySpecificationAttribute);

        /// <summary>
        /// Gets a count of category specification attribute mapping records
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; 0 to load all records</param>
        /// <returns>Count</returns>
        int GetCategorySpecificationAttributeCount(int categoryId = 0, int specificationAttributeOptionId = 0);

        #endregion
    }
}
