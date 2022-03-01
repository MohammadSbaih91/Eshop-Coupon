using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nop.Web
{
    public static class EShopHelper
    {
        public static string CacheKey_CategoryHomepageSubcategories = "Nop.category.homepage.subcategories-{0}-{1}-{2}-{3}-{4}-{5}";
        public static string CacheKey_CategoryImages = "Nop.category.homepage.subcategories.Images-{0}-{1}-{2}-{3}-{4}-{5}";
        public static string SelectedSimCardNumber = "SelectedSimCardNumber";
        public static string DevicePackage = "DevicePackage";
        public static int Animationspeed = 10000;
        public static string MonthlyPriceAttributeSmartLife = "ProductAttribute.MonthlyPrice.SmartLife";
        public static string BookAppintmentId = "BookAppintmentId";
        public static string ProductCategoryIds = "Product.CategoryIds";
        
        public static string SimTypeAttribute = "productattribute.simtype";
        public static string SimCardNumber = "productattribute.simcardnumber";
        public static string DevicePackageAttribute = "ProductAttribute.DevicePackage";
        public static string ProductIdsWhichNotShowPickupInStore = "EShop.ProductIdsWhichNotShowPickupInStore";

        public static string GetPriceFormatting(string PriceValue)
        {
            if (PriceValue == null)
            {
                PriceValue = "JOD 0.0";
            }
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            string result = Regex.Replace(PriceValue, "[^0-9][^0-9.]", string.Empty);

            var price = "";
            var priceList = result.Split(".");
            if(priceList.Count() == 2)
                price = string.Format(localizationService.GetResource("Common.PriceFormate"), priceList[0], priceList[1]);
            else if(priceList.Count() == 1)
                price = string.Format(localizationService.GetResource("Common.PriceFormate"), priceList[0], 00);

            return price;
        }

        public static string GetSvgUrl(string defaultImage,string svgName)
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            
            var imagePath = defaultImage;
            var thumbsDirectoryPath = GetAbsolutePath(@"images\Cat_Svg", svgName);
            if (fileProvider.FileExists(thumbsDirectoryPath))
            {
                imagePath = webHelper.GetStoreLocation() + "app-images/Cat_Svg/" + svgName;
            }
            return imagePath;
        }

        public static string GetAbsolutePath(params string[] paths)
        {
            var nopConfig = EngineContext.Current.Resolve<NopConfig>();
            var allPaths = paths.ToList();
            allPaths.Insert(0, nopConfig.SharedFileStorageContainerName);

            return Path.Combine(allPaths.ToArray());
        }
    }
}
