// PdfSaver.cs – OCR-enabled, searchable PDFs
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Tesseract;

namespace SnipToPdf
{
    internal static class PdfSaver
    {
        // 14 400 pt = 200 inch, the max size allowed by the PDF spec
        private const double MAX_PT = 14_400;

        internal static void SaveBitmapAsPdf(Bitmap bmp)
        {
            //-------------------------------------------------------------
            // 1. Ensure output folder (ask once)
            //-------------------------------------------------------------
            string folder = Properties.Settings.Default.DefaultOutputFolder;
            if (string.IsNullOrWhiteSpace(folder))
            {
                using var dlg = new FolderBrowserDialog
                {
                    Description = "Select folder for Snip-to-PDF files"
                };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                folder = dlg.SelectedPath;
                Properties.Settings.Default.DefaultOutputFolder = folder;
                Properties.Settings.Default.Save();
            }
            Directory.CreateDirectory(folder);

            //-------------------------------------------------------------
            // 2. File name yyyy-MM-dd_X.pdf
            //-------------------------------------------------------------
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            int    idx  = 1;
            while (File.Exists(Path.Combine(folder, $"{date}_{idx}.pdf")))
                idx++;
            string pdfPath = Path.Combine(folder, $"{date}_{idx}.pdf");

            //-------------------------------------------------------------
            // 3. Flatten bitmap -> JPEG (better for PDF size & OCR contrast)
            //-------------------------------------------------------------
            string tmpJpg = Path.Combine(
                Path.GetTempPath(), $"snip_{Guid.NewGuid():N}.jpg");
            bmp.SetResolution(300, 300);  // higher DPI improves OCR
            bmp.Save(tmpJpg, ImageFormat.Jpeg);

            //-------------------------------------------------------------
            // 4. Compute page size (scale if over PDF limit)
            //-------------------------------------------------------------
            double imgWpt = bmp.Width  * 72.0 / bmp.HorizontalResolution;
            double imgHpt = bmp.Height * 72.0 / bmp.VerticalResolution;
            double scale  = 1.0;
            if (imgWpt > MAX_PT || imgHpt > MAX_PT)
                scale = Math.Min(MAX_PT / imgWpt, MAX_PT / imgHpt);

            double pageWpt = imgWpt * scale;
            double pageHpt = imgHpt * scale;

            //-------------------------------------------------------------
            // 5. Create PDF page + draw image
            //-------------------------------------------------------------
            using var doc  = new PdfDocument();
            var page       = doc.AddPage();
            page.Width     = pageWpt;
            page.Height    = pageHpt;

            using var gfx  = XGraphics.FromPdfPage(page);
            using var ximg = XImage.FromFile(tmpJpg);
            gfx.DrawImage(ximg, 0, 0, pageWpt, pageHpt);

            //-------------------------------------------------------------
            // 6. OCR with Tesseract & overlay invisible text
            //-------------------------------------------------------------
            string tessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            using var engine = new TesseractEngine(tessPath, "eng", EngineMode.DEFAULT);
            using var pix    = PixConverter.ToPix(bmp);
            using var pageOcr = engine.Process(pix);

            foreach (var block in pageOcr.GetTextLines())
            {
                foreach (var word in block)
                {
                    string text = word.GetText().Trim();
                    if (text.Length == 0) continue;

                    // word.BBox is in image pixels → convert to PDF points
                    double x  = word.BBox.X1 * 72.0 / bmp.HorizontalResolution * scale;
                    double y  = word.BBox.Y1 * 72.0 / bmp.VerticalResolution * scale;
                    double h  = (word.BBox.Y2 - word.BBox.Y1) * 72.0 / bmp.VerticalResolution * scale;

                    // PDF Y origin is bottom-left; convert
                    double yPdf = pageHpt - y - h;

                    gfx.Save();
                    gfx.IntersectClip(new XRect(x, yPdf, word.BBox.Width * 72.0 / bmp.HorizontalResolution * scale, h));

                    // Draw text in 100% transparent color (invisible but selectable)
                    var font = new XFont("Helvetica", h, XFontStyle.Regular);
                    var brush = new XSolidBrush(new XColor { A = 0, R = 0, G = 0, B = 0 });
                    gfx.DrawString(text, font, brush, new XPoint(x, yPdf + h * 0.85));
                    gfx.Restore();
                }
            }

            //-------------------------------------------------------------
            // 7. Save & cleanup
            //-------------------------------------------------------------
            doc.Save(pdfPath);
            try { File.Delete(tmpJpg); } catch { /* ignore */ }

            MessageBox.Show($"Saved searchable PDF:\n{pdfPath}", "SnipToPdf",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
