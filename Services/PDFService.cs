using DinkToPdf;
using DinkToPdf.Contracts;
using Org.BouncyCastle.Utilities;
using System.Drawing.Printing;
using PaperKind = DinkToPdf.PaperKind;

namespace AppFoods.Services
{
    public class PDFService : IPDFService
    {
        private readonly IConverter _converter;

        public PDFService(IConverter converter)
        {
            this._converter = converter;
        }

        public byte[] GeneratePDF(string contentHtml,
                                  Orientation orientation = Orientation.Portrait,
                                  PaperKind paperKind = PaperKind.A4)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = orientation,
                PaperSize = paperKind,
                Margins = new MarginSettings() { Top = 20, Bottom = 20, Left = 10, Right = 10 },
            };

            var objectSetting = new ObjectSettings()
            {
                PagesCount = true,
                HtmlContent = contentHtml,
                WebSettings = { DefaultEncoding = "utf-8" },
                //HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSetting }
            };

            return _converter.Convert(pdf);
        }
    }
}
