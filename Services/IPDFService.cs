using DinkToPdf;

namespace AppFoods.Services
{
    public interface IPDFService
    {
        byte[] GeneratePDF(string contentHtml, Orientation orientation = Orientation.Portrait, PaperKind paperKind = PaperKind.A4);
    }
}