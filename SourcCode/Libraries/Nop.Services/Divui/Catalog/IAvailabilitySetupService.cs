using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Catalog
{
    public partial interface IAvailabilitySetupService
    {
        
        IList<AvailabilitySetup> GetAllAvailabilitySetups(int productId = 0, List<int> customerRoleIds = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
