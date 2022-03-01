using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public partial class CommonController
    {
        //SEO sitemap page
        [HttpsRequirement(SslRequirement.No)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult SitemapXml(int? id)
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            var languages = _workContext.WorkingLanguage.UniqueSeoCode;

            var directoryPaht = _fileProvider.GetAbsolutePath("sitemap");
            if (!_fileProvider.DirectoryExists(directoryPaht))
                _fileProvider.CreateDirectory(directoryPaht);

            var filePath = _fileProvider.GetAbsolutePath("sitemap", $"sitemap.{languages}.xml");
            var siteMap = "";
            if (_fileProvider.FileExists(filePath))
            {
                siteMap = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
            }
            else
            {
                siteMap = _commonModelFactory.PrepareSitemapXml(id);
                _fileProvider.WriteAllText(filePath, siteMap, Encoding.UTF8);
            }

            return Content(siteMap, "text/xml");
        }

        //SEO sitemap page
        [HttpsRequirement(SslRequirement.No)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual IActionResult SitemapIndexedXml()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            var directoryPaht = _fileProvider.GetAbsolutePath("sitemap");
            if (!_fileProvider.DirectoryExists(directoryPaht))
                _fileProvider.CreateDirectory(directoryPaht);

            var filePath = _fileProvider.GetAbsolutePath("sitemap", $"sitemap_index.xml");
            var siteMap = "";
            if (_fileProvider.FileExists(filePath))
            {
                siteMap = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
            }

            return Content(siteMap, "text/xml");
        }
    }
}
