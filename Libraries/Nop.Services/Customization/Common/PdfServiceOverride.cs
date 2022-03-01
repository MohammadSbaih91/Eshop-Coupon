// RTL Support provided by Credo inc (www.credo.co.il  ||   info@credo.co.il)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace Nop.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfServiceOverride : PdfService
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly PdfSettings _pdfSettings;

        #endregion

        #region Ctor

        public PdfServiceOverride(AddressSettings addressSettings, CatalogSettings catalogSettings,
            CurrencySettings currencySettings, IAddressAttributeFormatter addressAttributeFormatter,
            ICurrencyService currencyService, IDateTimeHelper dateTimeHelper, ILanguageService languageService,
            ILocalizationService localizationService, IMeasureService measureService, INopFileProvider fileProvider,
            IOrderService orderService, IPaymentService paymentService, IPictureService pictureService,
            IPriceFormatter priceFormatter, IProductService productService, ISettingService settingService,
            IStoreContext storeContext, IStoreService storeService, IVendorService vendorService,
            IWorkContext workContext, MeasureSettings measureSettings, PdfSettings pdfSettings, TaxSettings taxSettings,
            VendorSettings vendorSettings) : base(addressSettings, catalogSettings, currencySettings,
            addressAttributeFormatter, currencyService, dateTimeHelper, languageService, localizationService,
            measureService, fileProvider, orderService, paymentService, pictureService, priceFormatter, productService,
            settingService, storeContext, storeService, vendorService, workContext, measureSettings, pdfSettings,
            taxSettings, vendorSettings)
        {
            _addressSettings = addressSettings;
            _addressAttributeFormatter = addressAttributeFormatter;
            _currencyService = currencyService;
            _languageService = languageService;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _settingService = settingService;
            _workContext = workContext;
            _pdfSettings = pdfSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get font
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <returns>Font</returns>
        protected override Font GetFont(string fontFileName)
        {
            return new Font(base.GetFont(fontFileName)) { Size = 12 };
        }

        /// <summary>
        /// Print products
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font</param>
        protected override void PrintProducts(int vendorId, Language lang, Font titleFont, Document doc, Order order,
            Font font, Font attributesFont)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
            };

            var cellProducts = GetPdfCell("PDFInvoice.Product(s)", lang, titleFont);
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);

            var orderItems = order.OrderItems;

            var count = 3;

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
            };

            productsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            var widths = new Dictionary<int, int[]>
            {
                {3, new[] {45, 20, 20}},
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            //product name
            var cellProductItem = GetPdfCell("PDFInvoice.ProductName", lang, font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellProductItem.Border = Rectangle.NO_BORDER;
            //productsTable.AddCell(cellProductItem);

            //price
            cellProductItem = GetPdfCell("PDFInvoice.ProductPrice", lang, font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellProductItem.Border = Rectangle.NO_BORDER;

            //productsTable.AddCell(cellProductItem);

            //qty
            cellProductItem = GetPdfCell("PDFInvoice.ProductQuantity", lang, font);
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellProductItem.Border = Rectangle.NO_BORDER;
            //productsTable.AddCell(cellProductItem);

            foreach (var orderItem in orderItems)
            {
                var p = orderItem.Product;

                //a vendor should have access only to his products
                if (vendorId > 0 && p.VendorId != vendorId)
                    continue;

                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                //product name
                var name = _localizationService.GetLocalized(p, x => x.Name, lang.Id);
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;
                pAttribTable.AddCell(new Paragraph("\u25FE " + name, font));

                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.AddElement(new Paragraph("\u25FE " + name, titleFont));

                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);

                    pAttribTable.AddCell(attributesParagraph);
                }

                //rental info
                if (orderItem.Product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalStartDateUtc.Value)
                        : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalEndDateUtc.Value)
                        : string.Empty;
                    var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);

                    var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                    pAttribTable.AddCell(rentalInfoParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, false);
                }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);
            }

            doc.Add(productsTable);
        }

        /// <summary>
        /// Print shipping info
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">PDF table for address</param>
        protected override void PrintShippingInfo(Language lang, Order order, Font titleFont, Font font,
            PdfPTable addressTable)
        {
            var shippingAddress = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang)
            };
            shippingAddress.DefaultCell.Border = Rectangle.NO_BORDER;

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                //cell = new PdfPCell();
                //cell.Border = Rectangle.NO_BORDER;
                const string indent = "";

                if (!order.PickUpInStore)
                {
                    if (order.ShippingAddress == null)
                        throw new NopException(
                            $"Shipping is required, but address is not available. Order ID = {order.Id}");

                    shippingAddress.AddCell(GetParagraph("PDFInvoice.ShippingInformation", lang,
                        new Font(titleFont) { Size = 28f }));
                    shippingAddress.AddCell(new Phrase());
                    shippingAddress.AddCell(new Phrase());
                    shippingAddress.AddCell(new Phrase());
                    if (!string.IsNullOrEmpty(order.ShippingAddress.Company))
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Company", indent, lang, font,
                            order.ShippingAddress.Company));
                    shippingAddress.AddCell(GetParagraph("PDFInvoice.Name", indent, lang, font,
                        order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Phone", indent, lang, font,
                            order.ShippingAddress.PhoneNumber));
                    if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(order.ShippingAddress.FaxNumber))
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Fax", indent, lang, font,
                            order.ShippingAddress.FaxNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Address", indent, lang, font,
                            order.ShippingAddress.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.ShippingAddress.Address2))
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Address2", indent, lang, font,
                            order.ShippingAddress.Address2));
                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{indent}{order.ShippingAddress.City}, " +
                                          $"{(!string.IsNullOrEmpty(order.ShippingAddress.County) ? $"{order.ShippingAddress.County}, " : string.Empty)}" +
                                          $"{(order.ShippingAddress.StateProvince != null ? _localizationService.GetLocalized(order.ShippingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                                          $"{order.ShippingAddress.ZipPostalCode}";
                        shippingAddress.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                        shippingAddress.AddCell(
                            new Paragraph(
                                indent + _localizationService.GetLocalized(order.ShippingAddress.Country, x => x.Name,
                                    lang.Id), font));
                    //custom attributes
                    var customShippingAddressAttributes =
                        _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        //TODO: we should add padding to each line (in case if we have several custom address attributes)
                        shippingAddress.AddCell(new Paragraph(
                            indent + HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true),
                            font));
                    }

                    shippingAddress.AddCell(new Paragraph(" "));
                }
                else if (order.PickupAddress != null)
                {
                    shippingAddress.AddCell(GetParagraph("PDFInvoice.Pickup", lang, new Font(titleFont) { Size = 28f }));
                    shippingAddress.AddCell(new Phrase());
                    if (!string.IsNullOrEmpty(order.PickupAddress.Address1))
                        shippingAddress.AddCell(new Paragraph(
                            $"{indent}{string.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.PickupAddress.Address1)}",
                            font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.City))
                        shippingAddress.AddCell(new Paragraph($"{indent}{order.PickupAddress.City}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.County))
                        shippingAddress.AddCell(new Paragraph($"{indent}{order.PickupAddress.County}", font));
                    if (order.PickupAddress.Country != null)
                        shippingAddress.AddCell(
                            new Paragraph(
                                $"{indent}{_localizationService.GetLocalized(order.PickupAddress.Country, x => x.Name, lang.Id)}",
                                font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.ZipPostalCode))
                        shippingAddress.AddCell(new Paragraph($"{indent}{order.PickupAddress.ZipPostalCode}", font));
                    shippingAddress.AddCell(new Paragraph(" "));
                }

                addressTable.AddCell(shippingAddress);
            }
            else
            {
                shippingAddress.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddress);
            }
        }

        /// <summary>
        /// Print billing info
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">Address PDF table</param>
        protected override void PrintBillingInfo(int vendorId, Language lang, Font titleFont, Order order, Font font,
            PdfPTable addressTable)
        {
            const string indent = "";
            var billingAddress = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            billingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
            billingAddress.AddCell(
                GetParagraph("PDFInvoice.BillingInformation", lang, new Font(titleFont) { Size = 28f }));
            billingAddress.AddCell(new Phrase());
            billingAddress.AddCell(new Phrase());
            billingAddress.AddCell(new Phrase());

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(order.BillingAddress.Company))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Company", indent, lang, font,
                    order.BillingAddress.Company));

            billingAddress.AddCell(GetParagraph("PDFInvoice.Name", indent, lang, font,
                order.BillingAddress.FirstName + " " + order.BillingAddress.LastName));

            if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(order.BillingAddress.FaxNumber))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Fax", indent, lang, font,
                    order.BillingAddress.FaxNumber));
            if (_addressSettings.StreetAddressEnabled)
                billingAddress.AddCell(GetParagraph("PDFInvoice.Address", indent, lang, font,
                    order.BillingAddress.Address1));
            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.BillingAddress.Address2))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Address2", indent, lang, font,
                    order.BillingAddress.Address2));
            if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
            {
                var addressLine = $"{indent}{order.BillingAddress.City}, " +
                                  $"{(!string.IsNullOrEmpty(order.BillingAddress.County) ? $"{order.BillingAddress.County}, " : string.Empty)}" +
                                  $"{(order.BillingAddress.StateProvince != null ? _localizationService.GetLocalized(order.BillingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                                  $"{order.BillingAddress.ZipPostalCode}";
                billingAddress.AddCell(new Paragraph(addressLine, font));
            }

            if (_addressSettings.CountryEnabled && order.BillingAddress.Country != null)
                billingAddress.AddCell(new Paragraph(
                    indent + _localizationService.GetLocalized(order.BillingAddress.Country, x => x.Name, lang.Id),
                    font));

            if (_addressSettings.PhoneEnabled)
                billingAddress.AddCell(GetParagraph("PDFInvoice.Phone", indent, lang, font,
                    order.BillingAddress.PhoneNumber));

            if (!string.IsNullOrWhiteSpace(order?.BillingAddress?.Email))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Email", indent, lang, font,
                    order.BillingAddress.Email));

            //VAT number
            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddress.AddCell(GetParagraph("PDFInvoice.VATNumber", indent, lang, font, order.VatNumber));

            //custom attributes
            var customBillingAddressAttributes =
                _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            {
                //TODO: we should add padding to each line (in case if we have several custom address attributes)
                billingAddress.AddCell(
                    new Paragraph(
                        indent + HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true), font));
            }

            //Customer Id
            billingAddress.AddCell(GetParagraph("PDFInvoice.CustomerId", indent, lang, font, order.BillingAddress.IdentityCardOrPassport.ToString()));

            addressTable.AddCell(billingAddress);
        }

        /// <summary>
        /// Print header
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        protected override void PrintHeader(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font,
            Font titleFont, Document doc)
        {
            //logo
            var logoPicture = _pictureService.GetPictureById(pdfSettingsByStore.LogoPictureId);
            var logoExists = logoPicture != null;

            //header
            var headerTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            headerTable.DefaultCell.BackgroundColor = BaseColor.Black;

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.9f, 0.1f } : new[] { 0.1f, 0.9f });
            headerTable.WidthPercentage = 100f;

            var cellHeader = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = GetAlignment(lang),
                BackgroundColor = BaseColor.Black,
                Padding = 15f
            };

            //logo               
            if (logoExists)
            {
                var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                var logo = Image.GetInstance(logoFilePath);
                logo.Alignment = GetAlignment(lang);
                logo.ScaleToFit(30f, 30f);
                cellHeader.AddElement(logo);
            }

            headerTable.AddCell(cellHeader);

            var fnt = new Font(titleFont) { Color = BaseColor.White, Size = 28f };
            cellHeader = GetPdfCell("PDFInvoice.Confirmation", lang, fnt);
            cellHeader.HorizontalAlignment = GetAlignment(lang, lang.Rtl);
            cellHeader.PaddingBottom = 15f;
            cellHeader.VerticalAlignment = Element.ALIGN_BOTTOM;
            cellHeader.BackgroundColor = BaseColor.Black;

            headerTable.AddCell(cellHeader);

            doc.Add(headerTable);

            var descriptionTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            descriptionTable.DefaultCell.Border = Rectangle.NO_BORDER;
            var greet = _localizationService.GetResource("PDFInvoice.Greet", lang.Id);
            var bAddress = order?.BillingAddress;
            var name = $"{bAddress?.Civility} {bAddress?.FirstName} {bAddress?.LastName},";

            cellHeader = GetPdfCell($"{greet} {name}", new Font(titleFont) { Size = 12 });
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.PaddingTop = 15f;
            cellHeader.Border = Rectangle.NO_BORDER;
            descriptionTable.AddCell(cellHeader);

            cellHeader = GetPdfCell("PDFInvoice.Message", lang, new Font(font) { Size = 12 });
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Border = Rectangle.NO_BORDER;
            cellHeader.PaddingBottom = 20f;
            descriptionTable.AddCell(cellHeader);
            doc.Add(descriptionTable);
            var line = new LineSeparator(0.8f, 100f, BaseColor.Black, Element.ALIGN_LEFT, 1);
            doc.Add(line);
            var summaryTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            summaryTable.DefaultCell.Border = Rectangle.NO_BORDER;
            fnt = new Font(titleFont) { Size = 28f };
            cellHeader = GetPdfCell("PDFInvoice.YourOrder", lang, fnt);
            cellHeader.HorizontalAlignment = GetAlignment(lang, lang.Rtl);
            cellHeader.PaddingBottom = 15f;
            cellHeader.Border = Rectangle.NO_BORDER;

            summaryTable.AddCell(cellHeader);
            doc.Add(summaryTable);

            summaryTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                DefaultCell = { Border = Rectangle.NO_BORDER },
                WidthPercentage = 100f,
            };
            summaryTable.SetWidths(new[] { 48, 48 });
            var summaryInfoTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                DefaultCell = { Border = Rectangle.NO_BORDER },
                WidthPercentage = 100f,
            };
            summaryInfoTable.SetWidths(new[] { 48, 48 });
            cellHeader = GetPdfCell("PDFInvoice.OrderNo", lang, titleFont);
            cellHeader.HorizontalAlignment = GetAlignment(lang, lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            summaryInfoTable.AddCell(cellHeader);

            var orangeFnt = new Font(titleFont) { Color = new BaseColor(255, 121, 0) };
            cellHeader = GetPdfCell(order?.Id, orangeFnt);
            cellHeader.HorizontalAlignment = GetAlignment(lang, !lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            if (!lang.Rtl)
                cellHeader.PaddingRight = 8f;
            else
                cellHeader.PaddingLeft = 8f;
            summaryInfoTable.AddCell(cellHeader);


            cellHeader = GetPdfCell("PDFInvoice.OrderDate", lang, titleFont);
            cellHeader.HorizontalAlignment = GetAlignment(lang, lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            summaryInfoTable.AddCell(cellHeader);

            cellHeader = GetPdfCell(order?.CreatedOnUtc.ToShortDateString(), font);
            cellHeader.HorizontalAlignment = GetAlignment(lang, !lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            if (!lang.Rtl)
                cellHeader.PaddingRight = 8f;
            else
                cellHeader.PaddingLeft = 8f;
            summaryInfoTable.AddCell(cellHeader);

            cellHeader = GetPdfCell("PDFInvoice.OrderTime", lang, titleFont);
            cellHeader.HorizontalAlignment = GetAlignment(lang, lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            summaryInfoTable.AddCell(cellHeader);

            cellHeader = GetPdfCell(order?.CreatedOnUtc.ToShortTimeString(), font);
            cellHeader.HorizontalAlignment = GetAlignment(lang, !lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            if (!lang.Rtl)
                cellHeader.PaddingRight = 8f;
            else
                cellHeader.PaddingLeft = 8f;
            summaryInfoTable.AddCell(cellHeader);
            summaryTable.AddCell(summaryInfoTable);

            summaryInfoTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                DefaultCell = { Border = Rectangle.NO_BORDER },
                WidthPercentage = 100f,
                HorizontalAlignment = GetAlignment(lang, true)
            };


            summaryInfoTable.SetWidths(new[] { 48, 48 });
            cellHeader = GetPdfCell("PDFInvoice.OrderTotalWithTax", lang, titleFont);
            cellHeader.HorizontalAlignment = GetAlignment(lang);
            cellHeader.Border = Rectangle.NO_BORDER;
            if (lang.Rtl)
                cellHeader.PaddingRight = 8f;
            else
                cellHeader.PaddingLeft = 8f;
            summaryInfoTable.AddCell(cellHeader);

            var totalInclTax = order?.OrderTotal ?? decimal.Zero;
            var totalInclTaxInCustomerCurrency =
                _currencyService.ConvertCurrency(totalInclTax, order.CurrencyRate);
            var orderTotalInclTaxStr = _priceFormatter.FormatPrice(totalInclTaxInCustomerCurrency, true,
                order.CustomerCurrencyCode, lang, true);

            cellHeader = GetPdfCell(orderTotalInclTaxStr, orangeFnt);
            cellHeader.HorizontalAlignment = GetAlignment(lang, !lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            summaryInfoTable.AddCell(cellHeader);
            summaryInfoTable.AddCell(new Phrase());
            summaryInfoTable.AddCell(new Phrase());

            cellHeader = GetPdfCell("PDFInvoice.OrderTotalWithoutTax", lang, titleFont);
            cellHeader.HorizontalAlignment = GetAlignment(lang);
            cellHeader.Border = Rectangle.NO_BORDER;
            if (lang.Rtl)
                cellHeader.PaddingRight = 8f;
            else
                cellHeader.PaddingLeft = 8f;
            summaryInfoTable.AddCell(cellHeader);

            var totalExclTax = order?.OrderTotal - order?.OrderTax ?? decimal.Zero;
            var totalExclTaxInCustomerCurrency =
                _currencyService.ConvertCurrency(totalExclTax, order.CurrencyRate);
            var orderTotalExclTaxStr = _priceFormatter.FormatPrice(totalExclTaxInCustomerCurrency, true,
                order.CustomerCurrencyCode, lang, false);
            cellHeader = GetPdfCell(orderTotalExclTaxStr, font);
            cellHeader.HorizontalAlignment = GetAlignment(lang, !lang.Rtl);
            cellHeader.Border = Rectangle.NO_BORDER;
            summaryInfoTable.AddCell(cellHeader);
            summaryTable.AddCell(summaryInfoTable);
            doc.Add(summaryTable);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
        public override void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var ordCount = orders.Count;
            var ordNum = 0;

            foreach (var order in orders)
            {
                //by default _pdfSettings contains settings for the current active store
                //and we need PdfSettings for the store which was used to place an order
                //so let's load it based on a store of the current order
                var pdfSettingsByStore = _settingService.LoadSetting<PdfSettings>(order.StoreId);

                var lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = _workContext.WorkingLanguage;

                //header
                PrintHeader(pdfSettingsByStore, lang, order, font, titleFont, doc);

                //products
                PrintProducts(vendorId, lang, titleFont, doc, order, font, attributesFont);

                //payment and shipping methods    
                AddMethods(vendorId, lang, order, titleFont, font, doc);

                //addresses
                PrintAddresses(vendorId, lang, titleFont, order, font, doc);

                //footer
                EShopFooter(lang, titleFont, font, doc);

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        protected virtual void EShopFooter(Language lang, Font titleFont, Font font, Document doc)
        {
            var line = new LineSeparator(0.8f, 100f, BaseColor.Black, Element.ALIGN_LEFT, -5);
            doc.Add(line);
            var descriptionTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            descriptionTable.DefaultCell.Border = Rectangle.NO_BORDER;
            var cellHeader = GetPdfCell("PDFInvoice.Regards", lang, titleFont);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.PaddingTop = 15f;
            cellHeader.Border = Rectangle.NO_BORDER;
            descriptionTable.AddCell(cellHeader);

            cellHeader = GetPdfCell("PDFInvoice.eShopTeam", lang, font);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Border = Rectangle.NO_BORDER;
            cellHeader.PaddingBottom = 20f;
            descriptionTable.AddCell(cellHeader);
            doc.Add(descriptionTable);

            line = new LineSeparator(30f, 100f, BaseColor.Black, Element.ALIGN_LEFT, -5);
            doc.Add(line);
        }

        protected virtual void AddMethods(int vendorId, Language lang, Order order, Font titleFont, Font font,
            Document doc)
        {
            const string indent = "   ";
            var pyMethod = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            //vendors payment details
            if (vendorId == 0)
            {
                //payment method
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
                var paymentMethodStr = paymentMethod != null
                    ? _localizationService.GetLocalizedFriendlyName(paymentMethod, lang.Id)
                    : order.PaymentMethodSystemName;
                if (!string.IsNullOrEmpty(paymentMethodStr))
                {
                    var cellPyMethods = GetPdfCell(paymentMethodStr, titleFont);
                    cellPyMethods.Border = Rectangle.NO_BORDER;
                    pyMethod.AddCell(cellPyMethods);
                    //                      methodsHeader.AddCell(GetParagraph("PDFInvoice.PaymentMethod", indent, lang, font, paymentMethodStr));
                }

                //custom values
                var customValues = _paymentService.DeserializeCustomValues(order);
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        pyMethod.AddCell(new Paragraph(" "));
                        pyMethod.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, font));
                        pyMethod.AddCell(new Paragraph());
                    }
                }

                if (!string.IsNullOrEmpty(paymentMethodStr) || customValues != null)
                {
                    doc.Add(pyMethod);
                }
            }

            var spMethods = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            if (!string.IsNullOrEmpty(order.ShippingMethod))
            {
                var cellSpMethods = GetPdfCell(order.ShippingMethod, titleFont);
                cellSpMethods.Border = Rectangle.NO_BORDER;
                spMethods.AddCell(cellSpMethods);
            }
            
            doc.Add(spMethods);
            doc.Add(new Paragraph(" "));
        }

        #endregion
    }
}