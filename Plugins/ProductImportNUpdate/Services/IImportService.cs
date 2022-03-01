using System.IO;

namespace ProductImportNUpdate.Services
{
    public partial interface IImportService
    {
        ImportResult ImportProductsFromStream(Stream stream);
        ImportResult ImportProductsFromText(string text);
    }

    public class ImportResult
    {
        public int Failures { get; set; }
        public int Successes { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Total { get; set; }
        public bool IsError => Failures > 0 || Total != Inserted + Updated;
        public override string ToString()
        {
            return IsError
                ? $"From {Total} entries, {Successes} Products imported successfully and {Failures} Products has errors, check the logs for more details" : 
                $"Products imported successfully, check the logs for more details";
        }

        public string Statistics =>
            $"Total entries = {Total}, Updated = {Updated}, New = {Inserted}, Success = {Successes}, Error = {Failures}";
    }
}