using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;

namespace Nop.Services.Security
{
    public partial class StandardPermissionProvider
    {
        public static readonly PermissionRecord ManageCollections = new PermissionRecord { Name = "Admin area. Manage Collections", SystemName = "ManageCollections", Category = "Catalog" };
        public static readonly PermissionRecord ManageAttractions = new PermissionRecord { Name = "Admin area. Manage Attractions", SystemName = "ManageAttractions", Category = "Catalog" };
        public static readonly PermissionRecord ManageBanners = new PermissionRecord { Name = "Admin area. Manage Banners", SystemName = "ManageBanners", Category = "Content Management" };
    }
}
