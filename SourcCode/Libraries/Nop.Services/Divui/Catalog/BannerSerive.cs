using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Divui.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Caching;
using Nop.Core.Data;

namespace Nop.Services.Divui.Catalog
{
    public partial class BannerSerive : IBannerService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : attraction ID
        /// </remarks>
        private const string BANNERS_BY_ID_KEY = "Nop.banner.id-{0}";
        
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string BANNERS_PATTERN_KEY = "Nop.banner.";
       

        #endregion
        #region Fields

        private readonly IRepository<Banner> _bannerRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="attractionRepository">Attraction repository</param>
        public BannerSerive(ICacheManager cacheManager,
            IRepository<Banner> bannerRepository
            )
        {
            this._cacheManager = cacheManager;
            this._bannerRepository = bannerRepository;
        }

        #endregion
        public void DeleteBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            banner.Deleted = true;
            UpdateBanner(banner);
        }

        public IPagedList<Banner> GetAllBanners(string bannerName = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _bannerRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(bannerName))
                query = query.Where(c => c.Name.Contains(bannerName));
            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.DisplayOrder);

            //paging
            return new PagedList<Banner>(query, pageIndex, pageSize);
        }

        public Banner GetBannerById(int bannerId)
        {
            if (bannerId == 0)
                return null;

            return _bannerRepository.GetById(bannerId);
        }

        public void InsertBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");
            _bannerRepository.Insert(banner);
        }

        public void UpdateBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");
            _bannerRepository.Update(banner);

        }
    }
}
