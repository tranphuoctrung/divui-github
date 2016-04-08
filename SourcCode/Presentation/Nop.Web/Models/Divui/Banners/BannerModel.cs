using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Divui.Banners
{
    public class BannerModel
    {
        public string Name { get; set; }

        public string Url { get; set; }
        
        public PictureModel PictureModel { get; set; }
    }
}