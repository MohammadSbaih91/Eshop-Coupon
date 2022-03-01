using System;
using System.IO;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Seo;
using ProductImportNUpdate.Infrastructure;

namespace ProductImportNUpdate.Services
{
    public partial class ImportServiceCsv : IImportService
    {
        private const string BACK_END_CODE_COLUMN = "backEndCode";
        private const string STOCK_LEVEL_COLUMN = "stockLevel";
        private const string DISPLAY_NAME_COLUMN = "displayName";
        private const string PRICE_COLUMN = "Price";
        private readonly ProductImportNUpdateSettings _importNUpdateSettings;

        #region Fields

        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public ImportServiceCsv(IProductService productService,
            ILogger logger, 
            IUrlRecordService urlRecordService,
            ProductImportNUpdateSettings importNUpdateSettings)
        {
            _productService = productService;
            _logger = logger;
            _urlRecordService = urlRecordService;
            _importNUpdateSettings = importNUpdateSettings;
        }

        #endregion

        #region ImportProductMethods

        public virtual ImportResult ImportProductsFromStream(Stream stream)
        {
            return ImportProductsFromCsvDoc(new CsvDoc(stream, _logger, _importNUpdateSettings.Separator));
        }

        public virtual ImportResult ImportProductsFromText(string text)
        {
            return ImportProductsFromCsvDoc(new CsvDoc(text, _logger, _importNUpdateSettings.Separator));
        }

        public virtual ImportResult ImportProductsFromCsvDoc(CsvDoc csvDoc)
        {
            csvDoc.ValidateColumnsOrThrowException(BACK_END_CODE_COLUMN, STOCK_LEVEL_COLUMN, DISPLAY_NAME_COLUMN,
                PRICE_COLUMN);

            var result = new ImportResult {Total = csvDoc.Rows.Count};
            foreach (var record in csvDoc.ToTuple())
            {
                try
                {
                    var product = _productService.GetProductBySku(record[BACK_END_CODE_COLUMN]) ??
                                  new Product {Published = true, CreatedOnUtc = DateTime.UtcNow};
                    Exception err = null;
                    try
                    {
                        var stockQty = Convert.ToInt32(record[STOCK_LEVEL_COLUMN]);
                        if (stockQty != default(int))
                            product.StockQuantity = stockQty;
                    }
                    catch (Exception e)
                    {
                        err = e;
                    }

                    try
                    {
                        var productPrice = Convert.ToDecimal(record[PRICE_COLUMN].ToValue<decimal>());
                        if (productPrice != default(decimal))
                            product.Price = productPrice;
                    }
                    catch (Exception e)
                    {
                        err = e;
                    }

                    product.Sku = record[BACK_END_CODE_COLUMN];
                    product.Name = record[DISPLAY_NAME_COLUMN];
                    product.Deleted = false;

                    if (product.Id == default(int))
                    {
                        product.ProductType = ProductType.SimpleProduct;
                        product.VisibleIndividually = true;
                        product.OrderMinimumQuantity = 1;
                        product.OrderMaximumQuantity = 1000;
                        product.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                        product.UpdatedOnUtc = DateTime.UtcNow;
                        product.IsShipEnabled = true;
                        product.AllowCustomerReviews = false;
                        _productService.InsertProduct(product);
                        result.Inserted++;
                    }
                    else
                    {
                        product.UpdatedOnUtc = DateTime.UtcNow;
                        _productService.UpdateProduct(product);
                        result.Updated++;
                    }

                    _urlRecordService.SaveSlug(product,
                        _urlRecordService.ValidateSeName(product, string.Empty, product.Name, true), 0);

                    if (err != null)
                        throw new Exception(
                            $"{nameof(ProductImportNUpdate)} - can not parse numeric value (other field updated)", err);

                    result.Successes++;
                }
                catch (Exception e)
                {
                    result.Failures++;
                    _logger.Error(
                        $"{nameof(ProductImportNUpdate)} - error while importing product Name = {record[DISPLAY_NAME_COLUMN]}, Sku = {record[BACK_END_CODE_COLUMN]}",
                        e);
                }
            }

            _logger.Information(
                $"{nameof(ProductImportNUpdate)} - {result.Statistics}");
            return result;
        }

        #endregion
    }
}