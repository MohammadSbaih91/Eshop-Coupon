using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Nop.Core.Configuration;

namespace Nop.Core.Infrastructure
{
    /// <summary>
    /// IO functions using the on-disk file system
    /// </summary>
    public partial class NopFileProviderOverride : NopFileProvider
    {
        #region Fields
        private readonly NopConfig _config;
        #endregion

        /// <summary>
        /// Initializes a new instance of a NopFileProvider
        /// </summary>
        /// <param name="hostingEnvironment">Hosting environment</param>
        /// <param name="config"></param>
        public NopFileProviderOverride(IHostingEnvironment hostingEnvironment, NopConfig config)
            : base(hostingEnvironment)
        {
            _config = config;
        }

        /// <summary>
        /// Returns the absolute path to the directory
        /// </summary>
        /// <param name="paths">An array of parts of the path</param>
        /// <returns>The absolute path to the directory</returns>
        public override string GetAbsolutePath(params string[] paths)
        {
            var allPaths = paths.ToList();
            allPaths.Insert(0, _config.SharedFileStorageContainerName);

            return Path.Combine(allPaths.ToArray());
        }

    }
}