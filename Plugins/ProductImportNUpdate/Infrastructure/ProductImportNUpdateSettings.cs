using Nop.Core.Configuration;

namespace ProductImportNUpdate.Infrastructure
{
    public class ProductImportNUpdateSettings : ISettings
    {
        public string GlobalPath { get; set; }
        public char Separator { get; set; }
        public bool EnableAutoImport { get; set; }
    }
}