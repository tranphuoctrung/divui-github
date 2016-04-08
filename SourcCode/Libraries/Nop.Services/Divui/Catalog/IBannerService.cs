using Nop.Core;
using Nop.Core.Domain.Divui.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Divui.Catalog
{
    public partial interface IBannerService
    {
        /// <summary>
        /// Delete banner
        /// </summary>
        /// <param name="banner">Banner</param>
        void DeleteBanner(Banner banner);

        /// <summary>
        /// Gets all banners
        /// </summary>
        /// <param name="bannerName">Banner name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Banners</returns>
        IPagedList<Banner> GetAllBanners(string bannerName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets a banner
        /// </summary>
        /// <param name="bannerId">Banner identifier</param>
        /// <returns>Banner</returns>
        Banner GetBannerById(int bannerId);

        /// <summary>
        /// Inserts banner
        /// </summary>
        /// <param name="banner">Banner</param>
        void InsertBanner(Banner banner);

        /// <summary>
        /// Updates the banner
        /// </summary>
        /// <param name="banner">Banner</param>
        void UpdateBanner(Banner banner);
    }
}
