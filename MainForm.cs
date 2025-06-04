// MainForm.cs – instruction window with Settings, Exit, and credit line
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnipToPdf
{
    public class MainForm : Form
    {
        private readonly NotifyIcon _tray = new();
        private const int HOTKEY_ID = 9000;

        public MainForm()
        {
            // -----------------------------------------------------------
            //  Main window
            // -----------------------------------------------------------
            Text = "SnipToPdf";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(360, 140);

            // 1-line usage tip
            var lblTip = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 50,
                Text = "Press  Ctrl + Shift + S  to snip.\nThe app lives in the system-tray.",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            Controls.Add(lblTip);

            // Settings button
            var btnSettings = new Button
            {
                Text = "Settings",
                Width = 100,
                Height = 28,
                Left = 60,
                Top = 65
            };
            btnSettings.Click += (_, __) => new SettingsForm().ShowDialog(this);
            Controls.Add(btnSettings);

            // Exit button
            var btnExit = new Button
            {
                Text = "Exit",
                Width = 100,
                Height = 28,
                Left = 200,
                Top = 65
            };
            btnExit.Click += (_, __) => Application.Exit();
            Controls.Add(btnExit);

            // Credit line
            var lblCredit = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 20,
                Text = "Developed by Maaz Siddiqui",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };
            Controls.Add(lblCredit);

            // -----------------------------------------------------------
            //  Tray icon
            // -----------------------------------------------------------
            _tray.Icon    = SystemIcons.Application;
            _tray.Text    = "SnipToPdf";
            _tray.Visible = true;

            var menu = new ContextMenuStrip();
            menu.Items.Add("Show Window", null, (_, __) => Show());
            menu.Items.Add("Settings…",    null, (_, __) => new SettingsForm().ShowDialog(this));
            menu.Items.Add("Exit",         null, (_, __) => Application.Exit());
            _tray.ContextMenuStrip = menu;

            // -----------------------------------------------------------
            //  Global hot-key  (Ctrl+Shift+S)
            // -----------------------------------------------------------
            if (!HotkeyManager.RegisterHotKey(Handle, HOTKEY_ID,
                    HotkeyManager.MOD_CONTROL | HotkeyManager.MOD_SHIFT, Keys.S))
            {
                MessageBox.Show("Could not register hot-key Ctrl + Shift + S",
                                "SnipToPdf", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //  Hot-key handler
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotkeyManager.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
                FireSnip();
            base.WndProc(ref m);
        }

        private void FireSnip()
        {
            var overlay = new SnipOverlayForm();
            overlay.SnippingCompleted += (_, bmp) => PdfSaver.SaveBitmapAsPdf(bmp);
            overlay.Show();
        }

        // Hide instead of quit when user clicks ✕
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            _tray.Visible = false;
            HotkeyManager.UnregisterHotKey(Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }
    }
}

