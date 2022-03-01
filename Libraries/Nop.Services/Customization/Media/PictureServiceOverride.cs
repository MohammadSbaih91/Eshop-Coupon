using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Seo;

namespace Nop.Services.Media
{
    /// <summary>
    /// Picture service for Customer
    /// </summary>
    public partial class PictureServiceOverride : PictureService
    {
        #region Fields

        private readonly IStaticCacheManager _cacheManager;
        private readonly INopFileProvider _fileProvider;
        private readonly NopConfig _config;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public PictureServiceOverride(IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            IStaticCacheManager cacheManager,
            NopConfig config)
            : base(dataProvider,
                dbContext,
                eventPublisher,
                fileProvider,
                productAttributeParser,
                pictureRepository,
                pictureBinaryRepository,
                productPictureRepository,
                settingService,
                urlRecordService,
                webHelper,
                mediaSettings)
        {
            _cacheManager = cacheManager;
            _fileProvider = EngineContext.Current.ResolveNamed<INopFileProvider>("Images");
            _config = config;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override void DeletePictureThumbs(Picture picture)
        {
            var filter = $"{picture.Id:0000000}*.*";
            var currentFiles = _fileProvider.GetFiles(_fileProvider.GetAbsolutePath(NopMediaDefaults.ImageThumbsPath),
                filter, false);
            foreach (var currentFileName in currentFiles)
            {
                var thumbFilePath = GetThumbLocalPath(currentFileName);
                _fileProvider.DeleteFile(thumbFilePath);
            }

            _cacheManager.RemoveByPattern(NopMediaDefaults.ThumbsPatternCacheKey);
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {
            return $"{_config.SharedFileStorageContainerName}/{NopMediaDefaults.ImageThumbsPath}/{thumbFileName}";
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            //return $"{_config.SharedFileStorageContainerName}/{thumbFileName}";
            storeLocation = !string.IsNullOrEmpty(storeLocation)
                ? storeLocation
                : _webHelper.GetStoreLocation();

            var url = storeLocation + "app-images/thumbs/";

            url = url + thumbFileName;
            return url;
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected override void SaveThumb(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            var thumbsDirectoryPath = $"{_config.SharedFileStorageContainerName}/{NopMediaDefaults.ImageThumbsPath}";
            _fileProvider.CreateDirectory(thumbsDirectoryPath);

            //save
            _fileProvider.WriteAllBytes(thumbFilePath, binary);

            _cacheManager.RemoveByPattern(NopMediaDefaults.ThumbsPatternCacheKey);
        }

        /// <summary>
        /// Get picture local path. Used when images stored on file system (not in the database)
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Local picture path</returns>
        protected override string GetPictureLocalPath(string fileName)
        {
            return _fileProvider.GetAbsolutePath("images", fileName);
        }

        #endregion
    }
}