using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Localization;
using System.Net;
using System.Text;
using Nop.Services.Seo;
using Nop.Services.Catalog;
using Nop.Data.Extensions;
using Nop.Core;

namespace Nop.Services.Common
{
    public static class UrlStrucutre
    {
        public static string UrlDecode(string slugURL)
        {
            if (string.IsNullOrEmpty(slugURL))
                return "";

                var slugs = slugURL.Split("/");
                var slug = slugs.LastOrDefault();

                StringBuilder structure = new StringBuilder();

                if (IsLocalizedUrl(slugURL, out Language _, out string languageurl))
                {
                    var isSlugFind = false;
                    structure.Insert(0, GetURL(slug, out isSlugFind));
                    if (!isSlugFind)
                        return slugURL;

                if (!structure.ToString().Contains(languageurl + "/"))
                {
                    structure.Insert(0, "/" + languageurl + "/");
                }
                else
                {
                    structure.Insert(0, "/");
                }
            }
            else
            {
                var isSlugFind = false;
                structure.Insert(0, "/" + GetURL(slug, out isSlugFind));
                if (!isSlugFind)
                    return slugURL;
            }

                if (slugURL.Contains("http"))
                {
                    if (slugURL.Contains("/en/"))
                        structure.Insert(0, "/en");
                    if (slugURL.Contains("/ar/"))
                        structure.Insert(0, "/ar");

                    var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                    var storeUrl = storeContext.CurrentStore.Url;
                    var lastChar = storeUrl.Substring(storeUrl.Length - 1);
                    if (lastChar == "/")
                    {
                        storeUrl = storeUrl.Substring(0, storeUrl.Length - 1);
                    }
                    structure.Insert(0, storeUrl);
                }
                var structureURL = WebUtility.UrlDecode(structure.ToString());
                return structureURL;
           
        }
        
        public static string GetURL(string slug, out bool isSlugFind)
        {
            isSlugFind = false;
            var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
            var categoryService = EngineContext.Current.Resolve<ICategoryService>();

            StringBuilder structure = new StringBuilder();
            
            if (!string.IsNullOrEmpty(slug))
            {
                structure.Append(slug);
                var urlRecord = urlRecordService.GetBySlug(slug);
                if (urlRecord != null)
                {
                    isSlugFind = true;

                    if (urlRecord.EntityName == "Product")
                    {
                        var catMap = categoryService.GetProductCategoriesByProductId(urlRecord.EntityId).FirstOrDefault();

                        if (catMap != null)
                        {
                            var categoryId = catMap.CategoryId;

                            structure.Insert(0, urlRecordService.GetSeName(catMap.Category) + "/");
                            if (catMap.Category.ParentCategoryId != 0)
                            {
                                var parentCatId = catMap.Category.ParentCategoryId;
                            L1:
                                var parCatMap = categoryService.GetCategoryById(parentCatId);
                                if (parCatMap != null)
                                {
                                    var parCatEentityName = parCatMap.GetUnproxiedEntityType().Name;
                                    structure.Insert(0, urlRecordService.GetSeName(parCatMap) + "/");
                                    if (parCatMap.ParentCategoryId != 0)
                                    {
                                        parentCatId = parCatMap.ParentCategoryId;
                                        goto L1;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(structure.ToString()))
                            {
                                var languageService = EngineContext.Current.Resolve<ILanguageService>();
                                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                                if (!structure.ToString().Contains("en/") && !structure.ToString().Contains("ar/"))
                                {
                                    if (workContext.WorkingLanguage.UniqueSeoCode.ToLower() == "en")
                                        structure.Insert(0, "en/");

                                    if (workContext.WorkingLanguage.UniqueSeoCode.ToLower() == "ar")
                                        structure.Insert(0, "ar/");
                                }
                            }
                        }
                    }
                    else if (urlRecord.EntityName == "Category")
                    {
                        var category = categoryService.GetCategoryById(urlRecord.EntityId);

                        if (category != null)
                        {
                            if (category.ParentCategoryId != 0)
                            {
                                var parentCatId = category.ParentCategoryId;
                            L1:
                                var parCatMap = categoryService.GetCategoryById(parentCatId);
                                if (parCatMap != null)
                                {
                                    structure.Insert(0, urlRecordService.GetSeName(parCatMap) + "/");
                                    if (parCatMap.ParentCategoryId != 0)
                                    {
                                        parentCatId = parCatMap.ParentCategoryId;
                                        goto L1;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return structure.ToString();
        }
        
        public static bool IsLocalizedUrl(string url, out Language language,out string languageurl)
        {
            language = null;
            languageurl = "";
            if (string.IsNullOrEmpty(url))
                return false;
            
            //get first segment of passed URL
            var firstSegment = url.Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrEmpty(firstSegment))
                return false;

            languageurl = firstSegment;

            //suppose that the first segment is the language code and try to get language
            var languageService = EngineContext.Current.Resolve<ILanguageService>();
            language = languageService.GetAllLanguages()
                .FirstOrDefault(urlLanguage => urlLanguage.UniqueSeoCode.Equals(firstSegment, System.StringComparison.InvariantCultureIgnoreCase));

            //if language exists and published passed URL is localized
            return language?.Published ?? false;
        }
        
    }
}
