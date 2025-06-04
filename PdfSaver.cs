// PdfSaver.cs – adds custom “Open Folder” button after save
using System;
using System.Diagnostics;
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
        private const double MAX_PT = 14_400;   // PDF spec page-size limit

        internal static void SaveBitmapAsPdf(Bitmap bmp)
        {
            //-------------------------------------------------------------
            // 1) ensure output folder
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
            // 2) file name yyyy-MM-dd_X.pdf
            //-------------------------------------------------------------
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            int n = 1;
            while (File.Exists(Path.Combine(folder, $"{date}_{n}.pdf"))) n++;
            string pdfPath = Path.Combine(folder, $"{date}_{n}.pdf");

            //-------------------------------------------------------------
            // 3) flatten screenshot to JPEG (better for OCR size/clarity)
            //-------------------------------------------------------------
            string tmpJpg = Path.GetTempFileName().Replace(".tmp", ".jpg");
            bmp.SetResolution(300, 300);
            bmp.Save(tmpJpg, System.Drawing.Imaging.ImageFormat.Jpeg);

            //-------------------------------------------------------------
            // 4) compute page size
            //-------------------------------------------------------------
            double imgWpt = bmp.Width * 72.0 / bmp.HorizontalResolution;
            double imgHpt = bmp.Height * 72.0 / bmp.VerticalResolution;
            double scale = 1.0;
            if (imgWpt > MAX_PT || imgHpt > MAX_PT)
                scale = Math.Min(MAX_PT / imgWpt, MAX_PT / imgHpt);

            double pageWpt = imgWpt * scale;
            double pageHpt = imgHpt * scale;

            //-------------------------------------------------------------
            // 5) create PDF & draw image
            //-------------------------------------------------------------
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            page.Width = pageWpt;
            page.Height = pageHpt;

            using var gfx = XGraphics.FromPdfPage(page);
            using var ximg = XImage.FromFile(tmpJpg);
            gfx.DrawImage(ximg, 0, 0, pageWpt, pageHpt);

            //-------------------------------------------------------------
            // 6) OCR with Tesseract 5 (optional)
            //-------------------------------------------------------------
            bool ocrOk = true;
            string tessDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            try
            {
                using var engine = new TesseractEngine(tessDir, "eng+eus+urd+chi_tra+chi_tra_vert+chi_sim+chi_sim_vert", EngineMode.Default);

                using var ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                using var pix = Pix.LoadFromMemory(ms.ToArray());
                using var pageOcr = engine.Process(pix);

                using var iter = pageOcr.GetIterator();
                iter.Begin();

                Rect rect = default;
                while (iter.TryGetBoundingBox(PageIteratorLevel.Word, out rect))
                {
                    string text = iter.GetText(PageIteratorLevel.Word)?.Trim() ?? "";
                    if (text.Length == 0)
                    {
                        iter.Next(PageIteratorLevel.Word);
                        continue;
                    }

                    double x = rect.X1 * 72.0 / bmp.HorizontalResolution * scale;
                    double y = rect.Y1 * 72.0 / bmp.VerticalResolution * scale;
                    double h = rect.Height * 72.0 / bmp.VerticalResolution * scale;
                    double yPdf = pageHpt - y - h;

                    var font = new XFont("Helvetica", h, XFontStyle.Regular);
                    var brush = new XSolidBrush(new XColor { A = 0, R = 0, G = 0, B = 0 });
                    gfx.DrawString(text, font, brush, new XPoint(x, yPdf + h * 0.85));

                    iter.Next(PageIteratorLevel.Word);
                }
            }
            catch (Exception ex) when (ex is TesseractException || ex is DllNotFoundException)
            {
                ocrOk = false;
                // optional silent fallback; remove the comment if you’d like a warning
                // MessageBox.Show("OCR disabled: " + ex.Message, "SnipToPdf",
                //                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            //-------------------------------------------------------------
            // 7) save & clean up
            //-------------------------------------------------------------
            doc.Save(pdfPath);
            try { File.Delete(tmpJpg); } catch { /* ignore */ }

            ShowSaveDialog(pdfPath, ocrOk);
        }

        /// <summary>Shows a custom dialog with “Open Folder” and “OK”.</summary>
private static void ShowSaveDialog(string pdfPath, bool ocrOk)
{
    var dlg = new Form
    {
        Text            = "SnipToPdf",
        FormBorderStyle = FormBorderStyle.FixedDialog,
        MaximizeBox     = false,
        MinimizeBox     = false,
        StartPosition   = FormStartPosition.CenterParent,
        ClientSize      = new Size(460, 160)
    };

    // message label – fixed height at the top
    var lbl = new Label
    {
        Dock      = DockStyle.Top,
        Height    = 70,
        Text      = $"Saved {(ocrOk ? "searchable" : "image-only")} PDF:\n{pdfPath}",
        TextAlign = ContentAlignment.MiddleLeft
    };
    dlg.Controls.Add(lbl);

    // Open-Folder button
    var btnOpen = new Button
    {
        Text         = "Open Folder",
        Width        = 110,
        Height       = 30,
        Left         = 110,
        Top          = 95,
        DialogResult = DialogResult.Yes
    };
    btnOpen.Click += (_, __) =>
    {
        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{pdfPath}\"");
        dlg.Close();
    };
    dlg.Controls.Add(btnOpen);

    // OK button
    var btnOk = new Button
    {
        Text         = "OK",
        Width        = 80,
        Height       = 30,
        Left         = 260,
        Top          = 95,
        DialogResult = DialogResult.OK
    };
    dlg.Controls.Add(btnOk);

    dlg.AcceptButton  = btnOk;
    dlg.CancelButton  = btnOk;
    dlg.ShowDialog();
}

    }
}

