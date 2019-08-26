using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TwainDotNet;
using System.IO;
using TwainDotNet.WinFroms;

namespace TestApp
{
    using TwainDotNet.TwainNative;

    public partial class MainForm : Form
    {
        private static AreaSettings AreaSettings = new AreaSettings(0.1f, 5.7f, 0.1F + 2.6f, 5.7f + 2.6f);

        Twain _twain;
        ScanSettings _settings;

        public MainForm()
        {
            InitializeComponent();

            _twain = new Twain(new WinFormsWindowMessageHook(this));
            _twain.TransferImage += delegate(Object sender, TransferImageEventArgs args)
            {
                if (args.Image != null)
                {
                    pictureBox1.Image = args.Image;

                    widthLabel.Text = "Width: " + pictureBox1.Image.Width;
                    heightLabel.Text = "Height: " + pictureBox1.Image.Height;
                }
            };
            _twain.ScanningComplete += delegate
            {
                Enabled = true;
            };
        }

        private void selectSource_Click(object sender, EventArgs e)
        {
            _twain.SelectSource();
        }

        private void scan_Click(object sender, EventArgs e)
        {
            Enabled = false;

            _settings = new ScanSettings();
			_settings.Units = Units.Centimeters;
            _settings.UseDocumentFeeder = useAdfCheckBox.Checked;
            _settings.ShowTwainUI = useUICheckBox.Checked;
            _settings.ShowProgressIndicatorUI = showProgressIndicatorUICheckBox.Checked;
            _settings.UseDuplex = useDuplexCheckBox.Checked;
            _settings.Area = !checkBoxArea.Checked ? null : AreaSettings;
            _settings.ShouldTransferAllPages = true;

            if( blackAndWhiteCheckBox.Checked )
            {
                _settings.ColourSetting = ColourSetting.BlackAndWhite;
                _settings.Dpi = 200;
            }
            else
            {
                _settings.ColourSetting = ColourSetting.Colour;
                _settings.Dpi = 300;
            }

            _settings.AutomaticRotate = autoRotateCheckBox.Checked;
            _settings.AutomaticBorderDetection = autoDetectBorderCheckBox.Checked;

            try
            {
                _twain.StartScanning(_settings);
            }
            catch (TwainException ex)
            {
                MessageBox.Show(ex.Message);
                Enabled = true;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image.Save(sfd.FileName);
                }
            }
        }

        private void diagnostics_Click(object sender, EventArgs e)
        {
            var diagnostics = new Diagnostics(new WinFormsWindowMessageHook(this));
        }
    }
}
