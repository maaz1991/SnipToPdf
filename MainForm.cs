using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnipToPdf
{
    public class MainForm : Form
    {
        private NotifyIcon _tray = new NotifyIcon();
        private const int  ID_HOTKEY = 9000;

        public MainForm()
        {
            Visible = false; ShowInTaskbar = false;

            // tray icon
            _tray.Icon = SystemIcons.Application;
            _tray.Text = "SnipToPdf";
            _tray.Visible = true;

            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (_, __) => Application.Exit());
            _tray.ContextMenuStrip = menu;

            // register Ctrl+Shift+S
            if (!HotkeyManager.RegisterHotKey(Handle, ID_HOTKEY,
                    HotkeyManager.MOD_CONTROL | HotkeyManager.MOD_SHIFT, Keys.S))
                MessageBox.Show("Hotkey registration failed.");
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotkeyManager.WM_HOTKEY && m.WParam.ToInt32() == ID_HOTKEY)
                FireSnip();
            base.WndProc(ref m);
        }

        private void FireSnip()
        {
            var overlay = new SnipOverlayForm();
            overlay.SnippingCompleted += (_, bmp) => PdfSaver.SaveBitmapAsPdf(bmp);
            overlay.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _tray.Visible = false;
            HotkeyManager.UnregisterHotKey(Handle, ID_HOTKEY);
            base.OnFormClosing(e);
        }
    }
}
