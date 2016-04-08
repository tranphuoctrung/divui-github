using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Catalog
{
    public static class PriceSetupExtension
    {
        /// <summary>
        /// Filter tier prices for a customer
        /// </summary>
        /// <param name="source">setup prices</param>
        /// <param name="customer">Customer</param>
        /// <returns>Filtered tier prices</returns>
        public static IList<PriceSetup> FilterForCustomer(this IList<PriceSetup> source,
            Customer customer)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<PriceSetup>();
            foreach (var priceSetup in source)
            {
                //check customer role requirement
                if (priceSetup.CustomerRole != null)
                {
                    if (customer == null)
                        continue;

                    var customerRoles = customer.CustomerRoles.Where(cr => cr.Active).ToList();
                    if (customerRoles.Count == 0)
                        continue;

                    bool roleIsFound = false;
                    foreach (var customerRole in customerRoles)
                        if (customerRole == priceSetup.CustomerRole)
                            roleIsFound = true;

                    if (!roleIsFound)
                        continue;

                }

                result.Add(priceSetup);
            }

            return result;
        }
    }
}
