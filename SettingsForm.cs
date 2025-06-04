using System;
using System.IO;
using System.Windows.Forms;

namespace SnipToPdf
{
    public class SettingsForm : Form
    {
        private Label   lblFolder;
        private TextBox txtFolder;
        private Button  btnBrowse;
        private Button  btnSave;
        private Button  btnCancel;

        public SettingsForm()
        {
            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Width  = 500;
            Height = 160;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Label
            lblFolder = new Label
            {
                Text = "Default output folder:",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 15)
            };

            // TextBox (read-only)
            txtFolder = new TextBox
            {
                ReadOnly = true,
                Location = new System.Drawing.Point(10, 40),
                Width = 360
            };

            // Browse button
            btnBrowse = new Button
            {
                Text = "Browse…",
                Location = new System.Drawing.Point(380, 38),
                Width = 90
            };
            btnBrowse.Click += BtnBrowse_Click;

            // Save button
            btnSave = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(310, 80),
                Width = 75
            };
            btnSave.Click += BtnSave_Click;

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(395, 80),
                Width = 75
            };
            btnCancel.Click += (s, e) => Close();

            // Add controls to form
            Controls.Add(lblFolder);
            Controls.Add(txtFolder);
            Controls.Add(btnBrowse);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            // Load current setting
            txtFolder.Text = Properties.Settings.Default.DefaultOutputFolder;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select default folder to save PDFs";
                if (Directory.Exists(txtFolder.Text))
                    fbd.SelectedPath = txtFolder.Text;

                if (fbd.ShowDialog() == DialogResult.OK)
                    txtFolder.Text = fbd.SelectedPath;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string chosen = txtFolder.Text.Trim();
            if (string.IsNullOrEmpty(chosen))
            {
                MessageBox.Show("Please choose a folder.", "Warning", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!Directory.Exists(chosen))
                    Directory.CreateDirectory(chosen);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot create folder:\n{ex.Message}", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Properties.Settings.Default.DefaultOutputFolder = chosen;
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
