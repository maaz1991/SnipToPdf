using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SnipToPdf
{
    public class SnipOverlayForm : Form
    {
        public event EventHandler<Bitmap>? SnippingCompleted;

        private Point _startScreen;
        private Point _endScreen;
        private bool  _drawing;

        public SnipOverlayForm()
        {
            AutoScaleMode   = AutoScaleMode.None;          // no DPI scaling
            FormBorderStyle = FormBorderStyle.None;
            Bounds          = SystemInformation.VirtualScreen;
            DoubleBuffered  = true;
            BackColor       = Color.White;
            Opacity         = 0.25;
            Cursor          = Cursors.Cross;
            TopMost         = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _drawing = true;
            GetCursorPos(out _startScreen);
            _endScreen = _startScreen;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_drawing) return;
            GetCursorPos(out _endScreen);
            Invalidate();                       // redraw red outline
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!_drawing || e.Button != MouseButtons.Left) return;
            _drawing = false;
            GetCursorPos(out _endScreen);

            Rectangle r = RectFromPoints(_startScreen, _endScreen);
            if (r.Width > 0 && r.Height > 0)
            {
                // Hide the overlay so we don’t capture the white tint
                Hide();
                Application.DoEvents();
                System.Threading.Thread.Sleep(40); // ≈1 frame

                Bitmap bmp = new Bitmap(r.Width, r.Height,
                                         System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(r.Location, Point.Empty, r.Size);
                }
                bmp.SetResolution(96, 96);

                SnippingCompleted?.Invoke(this, bmp);
            }
            Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_drawing) return;
            Rectangle rc = RectFromPoints(
                PointToClient(_startScreen), PointToClient(_endScreen));
            using var pen = new Pen(Color.Red, 2);
            e.Graphics.DrawRectangle(pen, rc);
        }

        private static Rectangle RectFromPoints(Point a, Point b) =>
            new Rectangle(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y),
                          Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

        [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point p);
    }
}

