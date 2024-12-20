using FabLab_Etiquette.Models;
using FabLab_Etiquette.Services;
using System.Collections.ObjectModel;

public class TestPdfService
{
    public static void TestMethod()
    {
        var labels = new ObservableCollection<LabelModel>
        {
            new LabelModel
            {
                X = 10, Y = 20, Width = 100, Height = 50,
                Shape = "Rectangle", Text = "Test", FontSize = 12, BorderThickness = 1
            }
        };

        string path = @"C:\Temp\Test.pdf";
        PdfService.CreateLabelsPdf(labels, path);
    }
}
