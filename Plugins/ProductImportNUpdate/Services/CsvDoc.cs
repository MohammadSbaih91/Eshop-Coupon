using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nop.Services.Logging;

namespace ProductImportNUpdate.Services
{
    public class CsvDoc
    {
        private readonly Stream _stream;
        private readonly bool _containsHeader;
        private readonly ILogger _logger;


        #region Properties

        public IEnumerable<string> DocData { get; private set; }
        public string ColumnData { get; private set; }
        public IEnumerable<string> RowData { get; private set; }
        public string[] Columns { get; set; }
        public List<string[]> Rows { get; set; }

        #endregion

        #region Ctor

        public CsvDoc(Stream stream, ILogger logger, char separator = ',')
        {
            _logger = logger;
            Rows = new List<string[]>();
            _stream = stream ?? new MemoryStream();
            _containsHeader = stream?.GetType().Name != nameof(MemoryStream);
            ParseStream();
            ReadDataBySeparator(separator);
        }

        public CsvDoc(string csvText, ILogger logger, char separator = ',')
        {
            _logger = logger;
            Rows = new List<string[]>();
            ParseText(csvText);
            ReadDataBySeparator(separator);
        }

        #endregion

        #region Utility

        protected CsvDoc ParseStream()
        {
            if (_stream.Length <= 0) return this;
            using (var stReader = new StreamReader(_stream, Encoding.GetEncoding("iso-8859-1")))
            {
                var csvText = stReader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(csvText)) return this;
//              DocData = _containsHeader ? data.Skip(4).SkipLast(7) : data;
                DocData = csvText.Split(new[] {'\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
                var docData = DocData as string[] ?? DocData.ToArray();
                ColumnData = docData.Take(1).FirstOrDefault();
                RowData = docData.Skip(1);
            }

            _stream?.Dispose();
            return this;
        }

        protected CsvDoc ParseText(string csvText)
        {
            if (string.IsNullOrWhiteSpace(csvText)) return this;
            DocData = csvText.Split(new[] {'\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
            var docData = DocData as string[] ?? DocData.ToArray();
            ColumnData = docData.Take(1).FirstOrDefault();
            RowData = docData.Skip(1);
            return this;
        }

        #endregion

        #region Methods

        public CsvDoc ValidateColumnsOrThrowException(params string[] columns)
        {
            foreach (var column in columns.Where(column => !Columns.Contains(column)))
                throw new Exception(
                    $"{nameof(ProductImportNUpdate)} - Column '{column}' is required, but not found in file");

            return this;
        }

        public CsvDoc ReadDataBySeparator(char separator)
        {
            if (separator == default(char) || !RowData.Any() || string.IsNullOrWhiteSpace(ColumnData)) return this;

            Columns = ColumnData.Split(new[] {separator, '\r'}, StringSplitOptions.RemoveEmptyEntries);
            var nColumn = Columns.Count();

            var i = 1;
            foreach (var row in RowData)
            {
                i++;
                try
                {
                    var fields = row.Split(separator);
                    var nField = fields.Count();
                    if (nField < nColumn)
                        throw new Exception(
                            $"{nameof(ProductImportNUpdate)} - Corrupted row, field count({nField}) must be same as column count({nColumn}), at line no. {i} \n ");

                    Rows.Add(fields.ToArray());
                }
                catch (Exception e)
                {
                    _logger.Error($"{nameof(ProductImportNUpdate)} - Invalid sequence at row number - {i} ", e);
                }
            }

            if (Columns == null || !Rows.Any() || Columns.Count() > Rows.FirstOrDefault()?.Count())
                throw new InvalidDataException(
                    $"{nameof(ProductImportNUpdate)} - Invalid separator ({separator}) or sequence of row data.");
            return this;
        }

        public IEnumerable<NullableDictionary<string, string>> ToTuple()
        {
            if (!Rows.Any() || Columns == null || !Columns.Any()) yield break;

            var tuple = new NullableDictionary<string, string>();
            foreach (var record in Rows)
            {
                tuple.Clear();
                for (var field = 0; field < record.Count(); field++)
                {
                    try
                    {
                        if (field < 0 || Columns.Length <= field) continue;

                        var key = Columns[field];
                        var value = record[field];
                        tuple.Add(key, value);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                yield return tuple;
            }
        }

        #endregion
    }
}