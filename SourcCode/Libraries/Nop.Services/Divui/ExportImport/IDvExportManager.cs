using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ExportImport
{
    public partial interface IExportManager
    {
        /// <summary>
        /// Export collection list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        string ExportCollectionsToXml();

        /// <summary>
        /// Export attraction list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        string ExportAttractionsToXml();
    }
}
