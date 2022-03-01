namespace Nop.Core.Configuration
{
    public partial class NopConfig
    {
        /// <summary>
        /// Gets or sets file path that store the image and files
        /// </summary>
        public string SharedFileStorageContainerName { get; set; }

        public string[] SchedultasksList { get; set; }

        public string[] EmployeeIpList { get; set; }
    }
}
