using System;
using System.Windows.Forms;
using TwainDotNet;
using TwainDotNet.WinFroms;

namespace TestApp
{
	using TwainDotNet.TwainNative;

	public partial class MainForm : Form
	{
		private static readonly AreaSettings AreaSettings = new AreaSettings( 0.1f, 5.7f, 0.1F + 2.6f, 5.7f + 2.6f );

#pragma warning disable IDE0069 // Disposable fields should be disposed
		readonly Twain _twain;
#pragma warning restore IDE0069 // Disposable fields should be disposed
		ScanSettings _settings;

		public MainForm()
		{
			InitializeComponent();

			_twain = new Twain( new WinFormsWindowMessageHook( this ) );
			_twain.TransferImage += delegate ( Object sender, TransferImageEventArgs args )
			{
				if( args.HBitmap != IntPtr.Zero )
				{
					using( var renderer = new TwainDotNet.Win32.BitmapRenderer( args.HBitmap ) )
					{
						pictureBox1.Image = renderer.RenderToBitmap();
					}

					widthLabel.Text = "Width: " + pictureBox1.Image.Width;
					heightLabel.Text = "Height: " + pictureBox1.Image.Height;
				}
			};
			_twain.ScanningComplete += delegate
			{
				Enabled = true;
			};

			this.FormClosed += MainForm_FormClosed;
		}

		private void MainForm_FormClosed( object sender, FormClosedEventArgs e )
		{
			this.FormClosed -= MainForm_FormClosed;

			if( _twain != null )
			{
				_twain.Dispose();
			}
		}

		private void SelectSource_Click( object sender, EventArgs e )
		{
			_twain.SelectSource();
		}

		private void Scan_Click( object sender, EventArgs e )
		{
			Enabled = false;

			_settings = new ScanSettings
			{
				Units = Units.Centimeters,
				UseDocumentFeeder = useAdfCheckBox.Checked,
				ShowTwainUI = useUICheckBox.Checked,
				ShowProgressIndicatorUI = showProgressIndicatorUICheckBox.Checked,
				UseDuplex = useDuplexCheckBox.Checked,
				Area = !checkBoxArea.Checked ? null : AreaSettings,
				ShouldTransferAllPages = true,
				DebugCapabilities = true,
			};

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
				_twain.StartScanning( _settings );
			}
			catch( Exception ex )
			{
				MessageBox.Show( ex.Message );
				Enabled = true;
			}
		}

		private void SaveButton_Click( object sender, EventArgs e )
		{
			if( pictureBox1.Image != null )
			{
				using( SaveFileDialog sfd = new SaveFileDialog() )
				{
					if( sfd.ShowDialog() == DialogResult.OK )
					{
						pictureBox1.Image.Save( sfd.FileName );
					}
				}
			}
		}

		private void Diagnostics_Click( object sender, EventArgs e )
		{
			new Diagnostics( new WinFormsWindowMessageHook( this ) );
		}
	}
}
