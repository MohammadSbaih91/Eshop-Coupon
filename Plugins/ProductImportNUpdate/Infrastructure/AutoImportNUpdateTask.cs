using System;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using Nop.Core.Infrastructure;
using Nop.Services.Tasks;
using ProductImportNUpdate.Services;

namespace ProductImportNUpdate.Infrastructure
{
    public partial class AutoImportNUpdateTask : IScheduleTask
    {
        #region Fields

        private readonly ProductImportNUpdateSettings _importNUpdateSettings;
        private readonly IImportService _importService;
        private readonly INopFileProvider _fileProvider;
        private string ImportFolder => $@"{_importNUpdateSettings.GlobalPath}\ToImport";
        private string CompletedFolder => $@"{_importNUpdateSettings.GlobalPath}\Imported\";
        private string CurrentBatchLogFile { get; set; }

        #endregion

        #region Ctor

        public AutoImportNUpdateTask(
            ProductImportNUpdateSettings importNUpdateSettings,
            IImportService importService,
            INopFileProvider fileProvider)
        {
            _importNUpdateSettings = importNUpdateSettings;
            _importService = importService;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utilities

        private void Log(string text)
        {
            var logText = $"{DateTime.UtcNow:dd-MM-yyyy hh:mm:ss tt} : {text}";
            using (var writer = new StreamWriter(CurrentBatchLogFile, true))
                writer.WriteLine(logText);
        }

        #endregion

        #region Methods

        public void Execute()
        {
            if (!_importNUpdateSettings.EnableAutoImport) return;
            
            try
            {
                EnsureFolderExists();
                var files = _fileProvider.GetFiles(ImportFolder, "*.csv");
                if (files.Any())
                    ImportFiles(files);
                else
                    Log($"file does not exists at {ImportFolder}");
            }

            catch (Exception ex)
            {
                Log($"Error while reading file  : {ex}");
            }
        }

        private void EnsureFolderExists()
        {
            var folder = _fileProvider.Combine(_importNUpdateSettings.GlobalPath, "Logs");
            _fileProvider.CreateDirectory(folder);
            CurrentBatchLogFile =
                _fileProvider.Combine(folder, $"auto-import-{DateTime.UtcNow:dd-MM-yyyy-hh-mm-ss-tt}.log");
            _fileProvider.CreateFile(CurrentBatchLogFile);
            _fileProvider.CreateDirectory(ImportFolder);
            _fileProvider.CreateDirectory(CompletedFolder);
        }

        private void ImportFiles(string[] files)
        {
            Log("------Import batch started------");
            foreach (var importFile in files)
            {
                try
                {
                    var fileName = _fileProvider.GetFileName(importFile);
                    var completedFile = $@"{CompletedFolder}{fileName}";
                    if (_fileProvider.FileExists(completedFile))
                    {
                        Log($"File '{fileName}' already imported");
                        //_fileProvider.DeleteFile(importFile);
                        continue;
                    }

                    Log($"File '{fileName}' started importing");
                    _importService.ImportProductsFromText(
                        _fileProvider.ReadAllText(importFile, Encoding.Default));
                    Log($"File '{fileName}' import complete");

                    _fileProvider.FileMove(importFile, completedFile);
                }
                catch (Exception ex)
                {
                    Log($"Error in File '{importFile}' , ErrorMessage : {ex}");
                }
            }

            Log("------Import batch complete------");
        }

        #endregion
    }
}